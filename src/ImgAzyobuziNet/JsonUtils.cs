using System.IO;
using System.Text;
using Jil;

namespace ImgAzyobuziNet
{
    internal static class JsonUtils
    {
        private static readonly Encoding utf8 = new UTF8Encoding(false);

        // Jil の内部実装に最適化するためにジェネリック引数を取る
        internal static byte[] Serialize<T>(T data)
        {
            using (var ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms, utf8);
                JSON.Serialize(data, sw);
                sw.Flush();
                return ms.ToArray();
            }
        }
    }
}
