using Quick.Utils;

namespace YiWuLian.Client;

class Program
{
    public static ConfigModel Config { get; private set; }

    internal static void LoadConfig()
    {
        //注册编码提供程序(支持GB2312等编码)
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        Quick.Protocol.QpAllClients.RegisterUriSchema();
        Quick.Protocol.SerialPort.QpSerialPortClientOptions.RegisterUriSchema();
        Environment.CurrentDirectory = AppContext.BaseDirectory;
        Config = ConfigModel.Load();
    }

    public static void Main(string[] args)
    {
        ArgsHandlers.ArgsHandler.Invoke(args);
    }

    private static Task waitForExitTask;


    public static Task Start()
    {
        try
        {
            Console.WriteLine($@"---------------------
  {Consts.Name} [{Consts.Version}]
---------------------");
            IotManager.Instance.Start();
            LocalTcpManager.Instance.Start();
            waitForExitTask = new Task(() => Console.WriteLine("[停止完成]"));
            return waitForExitTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine("启动时出错。原因：" + ExceptionUtils.GetExceptionMessage(ex));
            throw;
        }
    }

    public static void Stop()
    {
        LocalTcpManager.Instance.Stop();
        IotManager.Instance.Stop();
        waitForExitTask.Start();
    }
}