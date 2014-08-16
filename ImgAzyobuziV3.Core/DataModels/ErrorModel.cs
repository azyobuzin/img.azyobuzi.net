using System;
using Newtonsoft.Json;

namespace ImgAzyobuziV3.Core.DataModels
{
    [JsonObject]
    public class ErrorModel
    {
        public ErrorModel() { }

        public ErrorModel(Exception ex)
        {
            var aex = ex as AggregateException;
            if (aex != null) ex = aex.Flatten().InnerException;
            var iex = ex as ImgAzyobuziException;
            if (iex != null)
            {
                this.Code = iex.ErrorCode;
                this.Message = iex.Message;
            }
            else
            {
                this.Code = Errors.UnknownError;
                this.Message = Errors.ErrorTable[Errors.UnknownError].Message;
            }
            this.Exception = ex.ToString();
        }

        public ErrorModel(int errorCode)
        {
            this.Code = errorCode;
            this.Message = Errors.ErrorTable[errorCode].Message;
        }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("exception")]
        public string Exception { get; set; }
    }
}
