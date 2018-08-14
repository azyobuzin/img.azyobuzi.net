namespace ImgAzyobuziNet
{
    internal class V2ErrorResponse
    {
        public V2ErrorObject error;
    }

    internal class V2ErrorObject
    {
        public int code;
        public string message;
        public string exception;
    }
}
