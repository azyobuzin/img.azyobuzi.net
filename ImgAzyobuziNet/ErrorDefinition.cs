namespace ImgAzyobuziNet
{
    public struct ErrorDefinition
    {
        public ErrorDefinition(int statusCode, string message)
        {
            this.StatusCode = statusCode;
            this.Message = message;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
