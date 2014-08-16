using System;
using System.Threading.Tasks;
using ImgAzyobuziV3.Core.DataModels;

namespace ImgAzyobuziV3.Core
{
    public static class Logging
    {
        public static void WriteLog(this ImgAzyobuziContext context, string service, string id, string api, string userAgent, string referer)
        {
            if (context.InfluxDbClient != null)
                Task.Run(() =>
                {
                    try
                    {
                        context.InfluxDbClient.WritePoints(
                            "log",
                            new[] { "service", "id", "version", "api", "user_agent", "referer" },
                            new object[] { service, id, 3, api, userAgent, referer }
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Couldn't write log: ");
                        Console.WriteLine(ex.ToString());
                    }
                });
        }

        public static void WriteLog(this ImgAzyobuziContext context, SizesModel model, string userAgent, string referer)
        {
            WriteLog(context, model.ServiceId, model.Id, "/sizes.json", userAgent, referer);
        }

        public static void WriteLog(this ImgAzyobuziContext context, RedirectResult model, string userAgent, string referer)
        {
            WriteLog(context, model.ServiceId, model.Id, "/redirect.json", userAgent, referer);
        }
    }
}
