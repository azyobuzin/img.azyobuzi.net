using System;

namespace ImgAzyobuziNet.Core
{
    public class ImgAzyobuziNetException : Exception
    {
        public ImgAzyobuziNetException() : base() { }
        public ImgAzyobuziNetException(string message) : base(message) { }
    }

    public class ImageNotFoundException : ImgAzyobuziNetException { }

    public class NotPictureException : ImgAzyobuziNetException { }

    public class NotConfiguredException : ImgAzyobuziNetException
    {
        public NotConfiguredException(string configurationKey)
            : base($"このサービスを使用するのに必要なオプション {configurationKey} が設定されていません。")
        { }
    }
}
