using Quick.Protocol;
using YiWuLian.Server.Core.Interfaces.Core;
using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol.Utils;
using YiWuLian.Server.Models;
using YiWuLian.Server.Core.NoticeTypes;
using Quick.Utils;
using YiQiDong.Agent;

namespace YiWuLian.Server.Core.Interfaces.Device
{
    public class Manager
    {
        public static Manager Instance { get; } = new Manager();

        private AllInterface allInterface;
        private AllInterfaceConfig deviceServiceConfig;
        private UnitStringConverting storageUnitStringConverting = UnitStringConverting.StorageUnitStringConverting;
        private int disconnectionDuartionMinutes;

        public void Init(IApplicationBuilder app, ConfigModel configModel)
        {
            disconnectionDuartionMinutes = Agent.Instance.Config.NoticeConnectionChangedDurationMinutes;
            deviceServiceConfig = Agent.Instance.Config.DeviceServiceConfig;
            deviceServiceConfig.InterfaceName = "设备接口";
            deviceServiceConfig.InstructionSet = [YlIotProtocol.V1.Instruction.Instance];
            deviceServiceConfig.WebSocketPath = "/ws/device";
            allInterface = new AllInterface(deviceServiceConfig, app);
        }

        public void Start()
        {
            allInterface.Start(deviceServiceConfig, commandExecuterManagerForRegister, noticeHandlerManager);
        }

        public void Stop()
        {
            allInterface.Stop();
            var devices = ConfigDbContext.CacheContext.Query<YIS_Device>();
            foreach (var device in devices)
            {
                var connectionInfo = device.ConnectionInfo;
                if (connectionInfo.IsConnected)
                    connectionInfo.Channel?.Disconnect();
            }
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
                Data = NoticeTypeManager.Instance.GetAll()
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
            var noticeType = NoticeTypeManager.Instance.Get(request.NoticeTypeId);
            var device = channel.Tag as YIS_Device;
            noticeType.SendNotice(device, request);
            return new();
        }

        private YlIotProtocol.V1.Commands.Register.Response ExecuteRegister(QpChannel channel, YlIotProtocol.V1.Commands.Register.Request request)
        {
            var deviceId = request.DeviceId;
            YIS_Device device;            
            device = ConfigDbContext.CacheContext.Find(new YIS_Device(deviceId));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{deviceId}]的设备");
            var correctAnswer = CryptographyUtils.ComputeMD5Hash(deviceId + channel.AuthenticateQuestion + device.ICCID);
            if (correctAnswer != request.AuthAnswer)
                throw new ApplicationException($"认证失败");
            var deviceConnectionInfo = device.ConnectionInfo;
            //重新生成连接取消令牌
            deviceConnectionInfo.RenewConnectCancellationToken();
            var deviceConnectCancellationToken = deviceConnectionInfo.ConnectCancellationToken;

            //如果此设备已经有连接，则断开之前的连接
            if (deviceConnectionInfo.IsConnected)
            {
                deviceConnectionInfo.Channel?.Disconnect();
                Thread.Sleep(100);
            }
            //更新连接信息
            deviceConnectionInfo.ClientProgram = request.ClientProgram;
            deviceConnectionInfo.Channel = channel;
            deviceConnectionInfo.SetConnected(true);
            channel.Tag = device;
            EventHandler channelDisconnectHandler = null;
            channelDisconnectHandler = (sender, e) =>
            {
                channel.Disconnected -= channelDisconnectHandler;
                channel.Disconnect();
                deviceConnectionInfo.SetConnected(false);
                //流量信息
                var dataUsageFullString = $"，使用流量: 发送[{storageUnitStringConverting.GetString(channel.BytesSent, 2, true)}B],接收[{storageUnitStringConverting.GetString(channel.BytesReceived, 2, true)}B]";
                //连接持续时间
                string connectDuartionFullString = null;
                if(deviceConnectionInfo.DisconnectTime.HasValue && deviceConnectionInfo.ConnectTime.HasValue)
                {
                    var timespan = deviceConnectionInfo.DisconnectTime.Value - deviceConnectionInfo.ConnectTime.Value;
                    connectDuartionFullString = $"，连接持续时间：{timespan:[-][d.]hh:mm:ss}";
                }
                AgentContext.LogDebug($"{device}已断开，通道：{channel.ChannelName}{connectDuartionFullString}{dataUsageFullString}");
                NoticeTypeManager.Instance.SaveConnectionLog(new()
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    Content = $"已断开，通道：{channel.ChannelName}{connectDuartionFullString}{dataUsageFullString}",
                    Time = DateTime.Now
                });
                //断开通知
                if (disconnectionDuartionMinutes > 0)
                {
                    Task.Delay(TimeSpan.FromMinutes(disconnectionDuartionMinutes), deviceConnectCancellationToken).ContinueWith(t =>
                    {
                        AgentContext.LogInfo($"{device}断开延时检测：deviceConnectionInfo.ConnectTime:{deviceConnectionInfo.ConnectTime},t.IsCanceled->{t.IsCanceled},deviceConnectCancellationToken.IsCancellationRequested:{deviceConnectCancellationToken.IsCancellationRequested}");
                        if (t.IsCanceled || deviceConnectCancellationToken.IsCancellationRequested)
                            return;
                        //短信通知
                        if (Agent.Instance.Config.SmsConfig.Enable && !string.IsNullOrEmpty(Agent.Instance.Config.SmsConfig.AdminNoticeTarget))
                        {
                            var noticeType = NoticeTypeManager.Instance.Get<NoticeTypes.SmsNoticeType.NoticeType>();
                            noticeType.SendNotice(new YIS_Device()
                            {
                                Id = "system",
                                Name = "系统"
                            },
                            new()
                            {
                                NoticeTypeId = noticeType.Id,
                                Target = Agent.Instance.Config.SmsConfig.AdminNoticeTarget,
                                Content = $"{deviceConnectionInfo.DisconnectTime.Value:yyyy-MM-dd HH:mm:ss}，{device}已断开{connectDuartionFullString}"
                            });
                        }
                    });
                }
            };
            channel.Disconnected += channelDisconnectHandler;
            channel.AddCommandExecuterManager(commandExecuterManager);
            //客户端
            string clientProgramFullString = null;
            if (!string.IsNullOrEmpty(request.ClientProgram))
                clientProgramFullString = $"，客户端：{request.ClientProgram}";
            //断开持续时间
            TimeSpan? disconnectDuartion = null;
            string disconnectDuartionFullString = null;
            if (deviceConnectionInfo.ConnectTime.HasValue && deviceConnectionInfo.DisconnectTime.HasValue)
            {
                disconnectDuartion = deviceConnectionInfo.ConnectTime.Value - deviceConnectionInfo.DisconnectTime.Value;
                disconnectDuartionFullString = $"，断开持续时间：{disconnectDuartion.Value:[-][d.]hh:mm:ss}";
            }

            AgentContext.LogDebug($"{device}已连接，通道：{channel.ChannelName}{clientProgramFullString}{disconnectDuartionFullString}");
            NoticeTypeManager.Instance.SaveConnectionLog(new()
            {
                DeviceId = device.Id,
                DeviceName = device.Name,
                Content = $"已连接，通道：{channel.ChannelName}{clientProgramFullString}{disconnectDuartionFullString}",
                Time = DateTime.Now
            });
            //连接通知
            if (disconnectionDuartionMinutes > 0 && disconnectDuartion.HasValue)
            {
                if (disconnectDuartion.Value.TotalMinutes > disconnectionDuartionMinutes)
                {
                    //短信通知
                    if (Agent.Instance.Config.SmsConfig.Enable && !string.IsNullOrEmpty(Agent.Instance.Config.SmsConfig.AdminNoticeTarget))
                    {
                        Task.Run(() =>
                        {
                            var noticeType = NoticeTypeManager.Instance.Get<NoticeTypes.SmsNoticeType.NoticeType>();
                            noticeType.SendNotice(new YIS_Device()
                            {
                                Id = "system",
                                Name = "系统"
                            },
                            new()
                            {
                                NoticeTypeId = noticeType.Id,
                                Target = Agent.Instance.Config.SmsConfig.AdminNoticeTarget,
                                Content = $"{deviceConnectionInfo.ConnectTime.Value:yyyy-MM-dd HH:mm:ss}，{device}已连接{disconnectDuartionFullString}"
                            });
                        });
                    }
                }
            }
            return new YlIotProtocol.V1.Commands.Register.Response();
        }

        public void OnDeviceDeleted(YIS_Device device)
        {
            device.ConnectionInfo.Channel?.Disconnect();
        }
    }
}
