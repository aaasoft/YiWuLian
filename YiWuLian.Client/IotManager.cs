using System.Text;
using Quick.Protocol;
using Quick.Protocol.Utils;
using Quick.Utils;
using Serilog;

namespace YiWuLian.Client;

public class IotManager
{
    public static IotManager Instance { get; } = new();
    private CancellationTokenSource cts;
    private QpClient IotClient { get; set; }
    private QpClientOptions qpClientOptions;

    private StringBuilder sbLogs = new();
    private void pushLog(string log)
    {
        sbLogs.Append(log);
        var message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + log;
        if (Program.Config.SaveLogFile)
            Log.Information(message);
        Console.WriteLine(message);
    }

    public void Start()
    {
        qpClientOptions = QpClientOptions.Parse(new Uri(Program.Config.ConnectUrl));
        qpClientOptions.Password = Program.Config.ConnectPassword;
        qpClientOptions.InstructionSet = [YlIotProtocol.V1.Instruction.Instance];
        if (qpClientOptions.TransportTimeout < 30000)
            qpClientOptions.TransportTimeout = 30000;
        if (Program.Config.SaveLogFile)
        {
            qpClientOptions.Logger = new QpLogger(pushLog)
            {
                LogRaw = true,
                LogConnection = true,
                LogContent = true,
                LogCommand = true,
                LogHeartbeat = true,
                LogPackage = true,
                LogNotice = true
            };
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
        }

        cts?.Cancel();
        cts = new();
        beginConnectToYlIot(cts.Token);
    }

    private void beginConnectToYlIot(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            QpClient qpClient = null;
            try
            {
                sbLogs.Clear();
                pushLog($"[易物联]正在通过[{Program.Config.ConnectUrl}]连接。。。");
                qpClient = IotClient = qpClientOptions.CreateClient();
                await qpClient.ConnectAsync();
                pushLog($"[易物联]连接成功。");
            }
            catch (Exception ex)
            {
                qpClient?.Close();
                pushLog($"[易物联]连接到易物联时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                delayToConnectToYlIot(cancellationToken);
                return;
            }
            try
            {
                pushLog($"[易物联]正在注册。。。");
                var correctAnswer = CryptographyUtils.ComputeMD5Hash(Program.Config.DeviceIMEI + qpClient.AuthenticateQuestion + Program.Config.DeviceICCID);
                await qpClient.SendCommand(new YlIotProtocol.V1.Commands.Register.Request()
                {
                    DeviceId = Program.Config.DeviceIMEI,
                    ClientProgram = $"{Consts.Name} {Consts.Version}",
                    AuthAnswer = correctAnswer
                });
                pushLog($"[易物联]注册成功。");
                EventHandler qpClient_Disconnected = null;
                qpClient_Disconnected = (sender, e) =>
                {
                    qpClient.Disconnected -= qpClient_Disconnected;
                    qpClient.Close();
                    pushLog($"[易物联]连接已经断开，原因：{ExceptionUtils.GetExceptionMessage(qpClient.LastException)}");
                    delayToConnectToYlIot(cancellationToken);
                };
                qpClient.Disconnected += qpClient_Disconnected;
            }
            catch (Exception ex)
            {
                qpClient?.Close();
                pushLog($"[易物联]注册到易物联时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                delayToConnectToYlIot(cancellationToken);
                return;
            }
        });
    }

    private void delayToConnectToYlIot(CancellationToken cancellationToken)
    {
        pushLog($"[易物联]将在5秒后尝试再次连接。");
        Task.Delay(5000, cancellationToken).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;
            beginConnectToYlIot(cancellationToken);
        });
    }

    public void Stop()
    {
        cts?.Cancel();
        cts = null;
        IotClient?.Close();
        IotClient = null;
    }


    private Dictionary<string, string> replaceStringDict = new Dictionary<string, string>()
    {
        [" "] = "_",
        ["　"] = "",
        ["℃"] = "C",
    };

    public async Task SendNoticeRequestAsync(string noticeTarget, string noticeContent)
    {
        var client = IotClient;
        if (client == null || !client.IsConnected)
            throw new IOException($"当前没有连接注册到易物联。日志：" + sbLogs.ToString());

        var content = noticeContent;
        //中国移动云MAS发送的内容中不能有特殊符号
        foreach (var item in replaceStringDict)
            content = content.Replace(item.Key, item.Value);

        await client.SendCommand(new YlIotProtocol.V1.Commands.SendNotice.Request()
        {
            NoticeTypeId = "sms",
            Target = noticeTarget,
            Content = content
        });
    }

    public class ApiResult
    {
        public string code { get; set; }
        public bool success { get; set; }
        public string msg { get; set; }
    }
}
