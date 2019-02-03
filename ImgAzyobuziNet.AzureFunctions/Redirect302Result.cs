using Microsoft.AspNetCore.Mvc;

namespace ImgAzyobuziNet.AzureFunctions
{
    // RedirectResult を使うと、 Functions の実行環境ではいろいろ足りてないので、ぬるりして死ぬ
    public class Redirect302Result : ActionResult
    {
        public Redirect302Result(string location)
        {
            this.Location = location;
        }

        public string Location { get; }

        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.Redirect(this.Location);
        }
    }
}
