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
        public NotConfiguredException()
            : base("このサービスを使用するのに必要なオプションが設定されていません。")
        { }
    }
}
