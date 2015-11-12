using System;

namespace ImgAzyobuziNet.Core
{
    public class ImgAzyobuziNetException : Exception { }

    public class ImageNotFoundException : ImgAzyobuziNetException { }

    public class IsNotPictureException : ImgAzyobuziNetException { }
}
