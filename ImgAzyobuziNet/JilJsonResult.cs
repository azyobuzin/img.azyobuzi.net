using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ImgAzyobuziNet
{
    public class JilJsonResult<T> : ActionResult
    {
        public T Data { get; set; }
        public int? StatusCode { get; set; }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var res = context.HttpContext.Response;
            if (this.StatusCode.HasValue)
                res.StatusCode = this.StatusCode.Value;
            res.ContentType = "application/json; charset=utf-8";
            var body = JsonUtils.Serialize(this.Data);
            res.ContentLength = body.Length;
            return res.Body.WriteAsync(body, 0, body.Length);
        }
    }

    public static class JilJsonResult
    {
        public static JilJsonResult<T> Create<T>(T data)
        {
            return new JilJsonResult<T> { Data = data };
        }

        public static JilJsonResult<T> CreateWithStatusCode<T>(T data, int statusCode)
        {
            return new JilJsonResult<T> { Data = data, StatusCode = statusCode };
        }
    }
}
