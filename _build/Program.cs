using Quick.Build;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

//版本号
var version = "1.0." + DateTime.Now.ToString("yyyy.Mdd");

//准备目录变量
var appFolder = QbFolder.GetAppFolder();
if (appFolder == Environment.CurrentDirectory)
    Environment.CurrentDirectory = Path.GetFullPath("../../../../");
var baseFolder = Environment.CurrentDirectory;

var productDict = new Dictionary<string, string>();

foreach (var fi in new DirectoryInfo(baseFolder).GetDirectories())
{
    var yiqidongImageFile = Path.Combine(fi.FullName, "YiQiDong.Image.json");
    if (!File.Exists(yiqidongImageFile))
        continue;
    productDict[fi.Name] = QbJson.ReadString(yiqidongImageFile, "Name");
}

Console.WriteLine("请选择编译项目(一个都不勾选代表全选)：");
var productDirs = QbSelect.MultiSelect(productDict.ToArray(), selectedForegroundColor: ConsoleColor.Green);
if (productDirs == null || productDirs.Length == 0)
    productDirs = productDict.Keys.ToArray();

Console.WriteLine("请选择编译架构(一个都不勾选代表全选)：");
var allArchs = new[] { "any", "win-x64", "linux-x64", "linux-arm64", "linux-arm", "osx-x64" };
var selectArchs = QbSelect.MultiSelect(allArchs.ToDictionary(t => t, t => t).ToArray(), selectedForegroundColor: ConsoleColor.Green);
if (selectArchs == null || selectArchs.Length == 0)
    selectArchs = allArchs;

foreach (var productDir in productDirs)
{
    foreach (var arch in selectArchs)
    {
        var configuration = arch == "any" ? "Release" : "ReleaseSelfContained";

        var publishFolder = $"{productDir}/bin/{configuration}/publish";
        var productName = QbJson.ReadString(Path.Combine($"{productDir}/YiQiDong.Image.json"), "Name");
        var outFolder = Path.GetFullPath("bin");
        if (!Directory.Exists(outFolder))
            Directory.CreateDirectory(outFolder);
        var outFile = Path.Combine(outFolder, $"{productName}-{version}-{arch}.ymg");

        //开始
        Console.WriteLine("----------------------------------");
        Console.WriteLine($"  欢迎使用[{productName}]发布脚本");
        Console.WriteLine("----------------------------------");
        Console.WriteLine($"正在删除{configuration}目录...");
        //先删除之前的编译目录
        QbFolder.DeleteFolders(productDir, configuration, SearchOption.AllDirectories);
        //再删除ymg文件
        QbFile.DeleteFiles("bin", $"{productName}-{version}.ymg");

        Console.WriteLine($"正在发布{productName}项目...");
        if (arch == "any")
        {
            QbCommand.Run("dotnet", $"publish {productDir} -c {configuration} -o {publishFolder} --no-self-contained");
            QbDotNet.KeepPublishRuntimes(publishFolder, "win-x64", "linux-x64", "linux-arm");
        }
        else
        {
            QbCommand.Run("dotnet", $"publish {productDir} -c {configuration} -o {publishFolder} -r {arch} --self-contained");
        }
        //复制文件
        QbFile.CopyFiles($"{productDir}", publishFolder, "YiQiDong.Image.*", true);

        //修改容器信息文件中的版本号
        QbJson.WriteString(Path.Combine(publishFolder, "YiQiDong.Image.json"), "Version", version);
        QbJson.Write(Path.Combine(publishFolder, "YiQiDong.Image.json"), "Platform", new string[] { arch });
        if (arch == "any")
        {
            QbJson.Write(Path.Combine(publishFolder, "YiQiDong.Image.json"), "AgentExecute", "dotnet");
            QbJson.Write(Path.Combine(publishFolder, "YiQiDong.Image.json"), "AgentStartup", $"{productDir}.dll");
            QbJson.Write(Path.Combine(publishFolder, "YiQiDong.Image.json"), "Runtime", new[] { "dotnet-10.0" });
        }
        else
        {
            var agentStartup = productDir;
            if (arch.StartsWith("win-"))
                agentStartup += ".exe";
            QbJson.Write(Path.Combine(publishFolder, "YiQiDong.Image.json"), "AgentExecute", agentStartup);
        }
        Console.WriteLine("正在制作易启动镜像...");
        using (var archive = ZipArchive.CreateArchive())
        {
            archive.AddAllFromDirectory(publishFolder);
            archive.SaveTo(outFile, CompressionType.LZMA);
        }
    }
}
Console.WriteLine("完成");
QbGui.OpenFolder("bin");

