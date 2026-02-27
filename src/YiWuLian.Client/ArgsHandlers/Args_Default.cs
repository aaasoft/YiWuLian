using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;

namespace YiWuLian.Client.ArgsHandlers;

public class Args_Default
{
    internal static void Invoke(string[] args)
    {
        var target = args[0];
        var content = args[1];

        var client = new TcpClient();
        client.Connect(IPAddress.Loopback, Program.Config.ServiceListenPort);
        client.SendTimeout = 5000;
        client.ReceiveTimeout = 5000;

        using (var writer = new StreamWriter(client.GetStream(), leaveOpen: true))
        {
            writer.WriteLine(target);
            writer.WriteLine(content);
            writer.Flush();
        }
        string ret;
        using (var reader = new StreamReader(client.GetStream(), leaveOpen: true))
        {
            ret = reader.ReadLine();
        }
        client.Dispose();
        if (ret == "OK")
        {
            Console.WriteLine("发送成功");
            Environment.Exit(0);
            return;
        }
        Console.WriteLine("发送失败: " + ret);
        Environment.Exit(-1);
    }
}
