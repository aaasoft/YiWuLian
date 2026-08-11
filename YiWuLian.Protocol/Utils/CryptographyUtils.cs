using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace YiWuLian.Protocol.Utils
{
    public class CryptographyUtils
    {
        public static string ComputeMD5Hash(string data)
        {
            int byteCount = Encoding.UTF8.GetByteCount(data);
            byte[] dataBuf = ArrayPool<byte>.Shared.Rent(byteCount);
            var dataSpan = dataBuf.AsSpan(0, byteCount);

            var hashBuffer = ArrayPool<byte>.Shared.Rent(MD5.HashSizeInBytes);
            var hashSpan = hashBuffer.AsSpan(0, MD5.HashSizeInBytes);
            try
            {
                Encoding.UTF8.GetBytes(data, dataSpan);
                MD5.HashData(dataSpan, hashSpan);
                return Convert.ToHexString(hashSpan).ToLower();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(dataBuf);
                ArrayPool<byte>.Shared.Return(hashBuffer);
            }
        }
    }
}
