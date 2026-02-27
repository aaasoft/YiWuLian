
namespace YiWuLian.Client.ArgsHandlers
{
    public class ArgsHandler
    {
        internal static void Invoke(string[] args)
        {
            var firstArg = args?.FirstOrDefault() ?? string.Empty;
            Program.LoadConfig();
            switch (firstArg)
            {
                case "":
                    Args_Empty.Invoke(args);
                    break;
                case "-debug":
                    Program.Start().Wait();
                    Program.Stop();
                    break;
                case "-service":
                    Args_Service.Invoke(args);
                    break;
                default:
                    Args_Default.Invoke(args);
                    break;
            }
        }
    }
}
