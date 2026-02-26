using Quick.Protocol;
using YiWuLian.Server.Core.Interfaces.Core;
using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol.Utils;
using YiWuLian.Server.Models;
using YiWuLian.Server.Core.NoticeTypes;
using Quick.Utils;

namespace YiWuLian.Server.Core.Interfaces.Device
{
    public class Manager
    {
        public static Manager Instance { get; } = new Manager();

        private AllInterface allInterface;
        private AllInterfaceConfig config;
        private UnitStringConverting storageUnitStringConverting = UnitStringConverting.StorageUnitStringConverting;

        public void Init(IApplicationBuilder app, ConfigModel configModel)
        {
            config = new AllInterfaceConfig()
            {
                InterfaceName = "设备接口",
                InstructionSet = [YlIotProtocol.V1.Instruction.Instance],
                Password = configModel.DeviceInterfacePassword,
                WebSocketEnable = configModel.DeviceInterfaceWebSocketEnable,
                WebSocketPath = "/ws/device",
                PipeEnable = configModel.DeviceInterfacePipeEnable,
                PipeName = configModel.DeviceInterfacePipeName,
                TcpEnable = configModel.DeviceInterfaceTcpEnable,
                TcpListenAddress = configModel.DeviceInterfaceTcpListenAddress,
                TcpListenPort = configModel.DeviceInterfaceTcpListenPort
            };
            allInterface = new AllInterface(config, app);
        }

        private Dictionary<string, DeviceConnectionInfo> connectedDeviceDict;
        private Dictionary<string, CancellationTokenSource> disconnectNoticeCtsDict;

        public DeviceConnectionInfo[] ConnectedDevices { get; private set; } = [];

        public string GetDeviceConnectStatus(string deviceId)
        {
            var isConnected = false;
            lock (connectedDeviceDict)
                isConnected = connectedDeviceDict.ContainsKey(deviceId);
            return isConnected ? "已连接" : string.Empty;
        }

        public void Start()
        {
            connectedDeviceDict = new();
            disconnectNoticeCtsDict = new();
            allInterface.Start(config, commandExecuterManagerForRegister, noticeHandlerManager);
        }

        public void Stop()
        {
            allInterface.Stop();
            foreach (var connectedDeviceInfo in ConnectedDevices)
                connectedDeviceInfo.Channel.Disconnect();
        }

        private CommandExecuterManager commandExecuterManagerForRegister;
        private CommandExecuterManager commandExecuterManager;
        private NoticeHandlerManager noticeHandlerManager;

        private Manager()
        {
            commandExecuterManagerForRegister = new();
            commandExecuterManagerForRegister.Register(new YlIotProtocol.V1.Commands.Register.Request(), ExecuteRegister);

            commandExecuterManager = new();
            commandExecuterManager.Register(new YlIotProtocol.V1.Commands.GetNoticeTypes.Request(), GetNoticeTypes);
            commandExecuterManager.Register(new YlIotProtocol.V1.Commands.SendNotice.Request(), SendNotice);

            noticeHandlerManager = new NoticeHandlerManager();
        }

        private YlIotProtocol.V1.Commands.GetNoticeTypes.Response GetNoticeTypes(QpChannel channel, YlIotProtocol.V1.Commands.GetNoticeTypes.Request request)
        {
            return new()
            {
                Data = NoticeTypes.NoticeTypeManager.Instance.GetAll()
                    .Select(t => new YlIotProtocol.V1.Models.NoticeTypeInfo()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        HasTarget = t.HasTarget,
                        TargetTypeId = t.TargetTypeId,
                        TargetTypeName = t.TargetTypeName
                    }).ToArray()
            };
        }

        private YlIotProtocol.V1.Commands.SendNotice.Response SendNotice(QpChannel channel, YlIotProtocol.V1.Commands.SendNotice.Request request)
        {
            var noticeType = NoticeTypes.NoticeTypeManager.Instance.Get(request.NoticeTypeId);
            noticeType.SendNotice(((DeviceConnectionInfo)channel.Tag).Device, request);
            return new();
        }

        private YlIotProtocol.V1.Commands.Register.Response ExecuteRegister(QpChannel channel, YlIotProtocol.V1.Commands.Register.Request request)
        {
            var deviceId = request.DeviceId;
            YIS_Device device;
            using (var dbContext = new ConfigDbContext())
            {
                device = dbContext.Find<YIS_Device>(deviceId);
                if (device == null)
                    throw new ApplicationException($"未找到编号为[{deviceId}]的设备");
                var correctAnswer = CryptographyUtils.ComputeMD5Hash(deviceId + channel.AuthenticateQuestion + device.ICCID);
                if (correctAnswer != request.AuthAnswer)
                    throw new ApplicationException($"认证失败");
            }
            //如果此设备已经有连接，则断开之前的连接
            DeviceConnectionInfo preConnectedDeviceInfo = null;
            lock (connectedDeviceDict)
                connectedDeviceDict.TryGetValue(device.Id, out preConnectedDeviceInfo);
            if (preConnectedDeviceInfo != null)
            {
                preConnectedDeviceInfo.Channel.Disconnect();
                Thread.Sleep(100);
            }
            //准备新连接
            var deviceDisconnectCancellationToken = clearOldDisconnectNoticeCts(device.Id,true);

            var deviceConnectionInfo = new DeviceConnectionInfo()
            {
                Device = device,
                ClientProgram = request.ClientProgram,
                Channel = channel,
                ConnectTime = DateTime.Now
            };
            channel.Tag = deviceConnectionInfo;
            lock (connectedDeviceDict)
            {
                connectedDeviceDict[device.Id] = deviceConnectionInfo;
                ConnectedDevices = connectedDeviceDict.Values.ToArray();
            }
            EventHandler handler = null;
            handler = (sender, e) =>
            {
                lock (connectedDeviceDict)
                {
                    if (connectedDeviceDict.ContainsKey(device.Id))
                        connectedDeviceDict.Remove(device.Id);
                    ConnectedDevices = connectedDeviceDict.Values.ToArray();
                }
                channel.Disconnected -= handler;
                NoticeTypeManager.Instance.SaveConnectionLog(new()
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    Content = $"已断开，通道：{channel.ChannelName}，连接持续时间：{DateTime.Now - deviceConnectionInfo.ConnectTime}，本次连接流量: 发送[{storageUnitStringConverting.GetString(channel.BytesSent, 2, true)}B],接收[{storageUnitStringConverting.GetString(channel.BytesReceived, 2, true)}B]",
                    Time = DateTime.Now
                });
                if (Agent.Instance.Config.DeviceDisconnectNoticeDurationMinutes > 0)
                {
                    Task.Delay(TimeSpan.FromMinutes(Agent.Instance.Config.DeviceDisconnectNoticeDurationMinutes), deviceDisconnectCancellationToken).ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                            return;
                        //如果配置了设备断开时通知，则进行通知
                        if (Agent.Instance.Config.SmsConfig.Enable && !string.IsNullOrEmpty(Agent.Instance.Config.SmsConfig.DeviceDisconnectNoticeTarget))
                        {
                            var noticeType = NoticeTypeManager.Instance.Get<NoticeTypes.SmsNoticeType.NoticeType>();
                            noticeType.SendNotice(device, new()
                            {
                                NoticeTypeId = noticeType.Id,
                                Target = Agent.Instance.Config.SmsConfig.DeviceDisconnectNoticeTarget,
                                Content = $"{device}已断开，通道：{channel.ChannelName}，连接持续时间：{DateTime.Now - deviceConnectionInfo.ConnectTime}，本次连接流量: 发送[{storageUnitStringConverting.GetString(channel.BytesSent, 1, true)}B],接收[{storageUnitStringConverting.GetString(channel.BytesReceived, 1, true)}B]"
                            });
                        }
                        //清除
                        clearOldDisconnectNoticeCts(device.Id);                            
                    });
                }
                else
                {
                    //清除
                    clearOldDisconnectNoticeCts(device.Id);
                }
            };
            channel.Disconnected += handler;
            channel.AddCommandExecuterManager(commandExecuterManager);
            NoticeTypeManager.Instance.SaveConnectionLog(new()
            {
                DeviceId = device.Id,
                DeviceName = device.Name,
                Content = $"已连接，通道：{channel.ChannelName}，客户端：{request.ClientProgram}",
                Time = DateTime.Now
            });
            return new YlIotProtocol.V1.Commands.Register.Response();
        }

        private CancellationToken clearOldDisconnectNoticeCts(string deviceId, bool addNew = false)
        {
            lock (disconnectNoticeCtsDict)
            {
                if (disconnectNoticeCtsDict.Remove(deviceId, out var cts))
                    cts.Cancel();
                if (addNew)
                {
                    cts = new CancellationTokenSource();
                    disconnectNoticeCtsDict[deviceId] = cts;
                }
                return cts.Token;
            }
        }

        public void OnDeviceDeleted(string deviceId)
        {
            lock (connectedDeviceDict)
            {
                if (connectedDeviceDict.TryGetValue(deviceId, out var connectedDeviceInfo))
                    connectedDeviceInfo.Channel.Disconnect();
            }
        }
    }
}
