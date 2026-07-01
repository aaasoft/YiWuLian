using Quick.Shell.Utils;
using System.Runtime.InteropServices;

namespace YiWuLian.Client.Utils.Unix
{
    public partial class UnixUtils
    {
        internal const int PR_SET_NAME = 15;

        private static Dictionary<string, string> fileNameReplaceDict = new Dictionary<string, string>()
        {
            [" "] = "\\ ",
            ["\""] = "\\\"",
            ["'"] = "\\'",
            ["`"] = "\\`"
        };

        /// <summary>
        /// 为文件添加可执行权限
        /// </summary>
        /// <param name="fileName"></param>
        public static void AddExecutePermissionToFile(string fileName)
        {
            foreach (var key in fileNameReplaceDict.Keys)
            {
                if (fileName.Contains(key))
                    fileName = fileName.Replace(key, fileNameReplaceDict[key]);
            }
            ProcessUtils.ExecuteShell($"chmod +x {fileName}");
        }

        /// <summary>
        /// 当前是否以root账号运行
        /// </summary>
        /// <returns></returns>
        public static bool IsRuningWithRoot()
        {
            return Environment.UserName == "root";
        }

        /// <summary>
        /// 是否是在chroot环境运行
        /// </summary>
        /// <returns></returns>
        public static bool IsRuningInChroot() => "1" == Environment.GetEnvironmentVariable("IS_CHROOT");
        /// <summary>
        /// 是否是在docker环境运行
        /// </summary>
        /// <returns></returns>
        public static bool IsRuningInDocker() => File.Exists("/.dockerenv");
    }
}
