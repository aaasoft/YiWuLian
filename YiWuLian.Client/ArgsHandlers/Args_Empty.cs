using Quick.Build;
using Quick.Shell.PowerShell;
using Quick.Utils;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using YiWuLian.Client.Utils;

namespace YiWuLian.Client.ArgsHandlers
{
    public partial class Args_Empty
    {
        private class ServiceStatus
        {
            public bool Installed { get; set; } = false;
            public bool Enabled { get; set; } = false;
            public bool Started { get; set; } = false;
        }

        internal static void Invoke(string[] args)
        {
            while (true)
            {
                Console.WriteLine($"-------欢迎使用{Consts.Name}--------");
                Console.WriteLine($"版本：{Consts.Version}");
                Console.WriteLine("-----------------------------");

                var select1Dict = new Dictionary<string, string>()
                {
                    ["Debug"] = "调试运行",
                    ["ServiceManage"] = "服务管理",
                    ["EditConfig"] = "编辑配置"
                };
                if (OperatingSystem.IsWindows())
                {
                    select1Dict["Shotcut"] = "快捷方式";
                    //select1Dict["Test"] = "测试";
                }
                select1Dict["Exit"] = "退出";
                var select1 = QbSelect.ArrowSelect(select1Dict.ToArray(), selectedForegroundColor: ConsoleColor.Green);
                var selectName = select1Dict[select1];
                Console.WriteLine($"----------{selectName}-----------");
                try
                {
                    switch (select1)
                    {
                        case "Debug":
                            Invoke_Debug();
                            break;
                        case "EditConfig":
                            Invoke_EditConfig();
                            break;
                        case "ServiceManage":
                            Invoke_ServiceManage();
                            break;
                        case "Shotcut":
                            if (OperatingSystem.IsWindows())
                                Invoke_Shotcut();
                            break;
                        case "Test":
                            Invoke_Test();
                            break;
                        case "Exit":
                            return;
                    }
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    ConsoleUtils.ConsoleWriteLine($"执行[{selectName}]时出错", ConsoleColor.Red);
                    ConsoleUtils.ConsoleWriteLine(ExceptionUtils.GetExceptionString(ex), ConsoleColor.Red);
                    Console.WriteLine("按回车键回到主菜单...");
                    Console.ReadLine();
                }
            }
        }

        private static void Invoke_Test()
        {
            
        }

        private static void Invoke_Debug()
        {
            Program.Start().Wait();
            Program.Stop();
        }

        private static void Invoke_EditConfig()
        {
            var changed = false;
            string line = null;

            ConsoleUtils.ConsoleWriteLine("服务通讯端口", ConsoleColor.Green);
            Console.WriteLine("说明：一般不用修改，除非端口被占用");
            Console.WriteLine($"当前值：{Program.Config.ServiceListenPort}");
            ConsoleUtils.ConsoleWrite(">", ConsoleColor.Green);
            line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                Program.Config.ServiceListenPort = int.Parse(line);
                changed = true;
            }

            ConsoleUtils.ConsoleWriteLine("易物联连接地址", ConsoleColor.Green);
            Console.WriteLine("说明：串口连接示例[qp.serial://./COM1?BaudRate=9600?TransportTimeout=30000]，TCP连接示例[qp.tcp://192.168.1.3:10067?TransportTimeout=30000]");
            Console.WriteLine($"当前值：{Program.Config.ConnectUrl}");
            ConsoleUtils.ConsoleWrite(">", ConsoleColor.Green);
            line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                Program.Config.ConnectUrl = line;
                changed = true;
            }

            ConsoleUtils.ConsoleWriteLine("易物联连接密码", ConsoleColor.Green);
            Console.WriteLine($"当前值：{Program.Config.ConnectPassword}");
            ConsoleUtils.ConsoleWrite(">", ConsoleColor.Green);
            line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                Program.Config.ConnectPassword = line;
                changed = true;
            }

            ConsoleUtils.ConsoleWriteLine("设备IMEI", ConsoleColor.Green);
            Console.WriteLine($"当前值：{Program.Config.DeviceIMEI}");            
            ConsoleUtils.ConsoleWrite(">", ConsoleColor.Green);
            line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                Program.Config.DeviceIMEI = line;
                changed = true;
            }

            ConsoleUtils.ConsoleWriteLine("设备ICCID", ConsoleColor.Green);
            Console.WriteLine($"当前值：{Program.Config.DeviceICCID}");
            ConsoleUtils.ConsoleWrite(">", ConsoleColor.Green);
            line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                Program.Config.DeviceICCID = line;
                changed = true;
            }
            
            var boolDict=  new Dictionary<string,string>()
            {
                ["true"] = "是",
                ["false"] = "否"
            };
            ConsoleUtils.ConsoleWriteLine("保存日志文件", ConsoleColor.Green);
            Console.WriteLine($"当前值：{boolDict[Program.Config.SaveLogFile.ToString().ToLower()]}");
            line = QbSelect.ArrowSelect(boolDict.ToArray(), selectedForegroundColor: ConsoleColor.Green);
            if (!string.IsNullOrEmpty(line))
            {
                Program.Config.SaveLogFile = bool.Parse(line);
                changed = true;
            }

            if (changed)
            {
                Program.Config.Save();
                ConsoleUtils.ConsoleWriteLine("[已保存修改后的配置]", ConsoleColor.Green);
            }
            else
            {
                Console.WriteLine("[配置未修改]");
            }
        }


        [SupportedOSPlatform("windows")]
        private static void Invoke_Shotcut()
        {
            var lnkFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{Consts.Name}.lnk");
            if (File.Exists(lnkFile))
            {
                ConsoleUtils.ExecuteAction("正在删除旧的桌面快捷方式", () => { File.Delete(lnkFile); });
            }
            var executeFileName = Process.GetCurrentProcess().MainModule.FileName;
            var psFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ps1");
            var psFileEncoding = Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage);
            var psFileContent = @$"$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut(""{lnkFile}"")
$Shortcut.TargetPath =""{executeFileName}""
$Shortcut.WorkingDirectory = ""{Path.GetDirectoryName(executeFileName)}"";
$Shortcut.Save()
Remove-Item ""{psFileName}""
";
            File.WriteAllText(psFileName, psFileContent, psFileEncoding);
            ConsoleUtils.ExecuteFunc("正在创建桌面快捷方式",
                () => PowerShellProcessContext.ExecutePs1File(psFileName));
            Console.WriteLine("[创建桌面快捷方式成功]");
        }
    }
}
