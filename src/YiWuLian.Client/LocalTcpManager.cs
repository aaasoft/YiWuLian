using System.Net;
using System.Net.Sockets;
using Quick.Utils;

namespace YiWuLian.Client;

public class LocalTcpManager
{
    public static LocalTcpManager Instance { get; } = new();

    private TcpListener listener;

    private CancellationTokenSource cts;
    public void Start()
    {
        cts?.Cancel();
        cts = new();
        listener = new TcpListener(IPAddress.Loopback, Program.Config.ServiceListenPort);
        listener.Start();
        _ = waitForConnection(listener,cts.Token);
    }

    private async Task waitForConnection(TcpListener listener, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(cancellationToken);
            _ = handleNewChannel(client, cancellationToken);
        }
    }

    private async Task handleNewChannel(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            string target, content;
            using (var reader = new StreamReader(client.GetStream(), leaveOpen:true))
            {
                target = await reader.ReadLineAsync(cancellationToken);
                content = await reader.ReadLineAsync(cancellationToken);
            }
            var msg = "OK";
            try
            {
                await IotManager.Instance.SendNoticeRequestAsync(target, content);
            }
            catch (Exception ex)
            {
                msg = ExceptionUtils.GetExceptionMessage(ex);
            }
            using (var writer = new StreamWriter(client.GetStream(), leaveOpen:true))
            {
                await writer.WriteLineAsync(msg);
                await writer.FlushAsync();
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            client.Dispose();
        }
    }

    public void Stop()
    {
        listener?.Stop();
        listener?.Dispose();
        listener = null;

        cts?.Cancel();
        cts = null;
    }
}
