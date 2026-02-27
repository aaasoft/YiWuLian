namespace YiWuLian.Client.Utils
{
    internal class DebugUtils
    {
        public static bool IsDebug()
        {
#if (DEBUG)
            return true;
#else
            return false;
#endif
        }
    }
}
