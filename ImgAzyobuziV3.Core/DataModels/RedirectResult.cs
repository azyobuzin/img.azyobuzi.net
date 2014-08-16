namespace ImgAzyobuziV3.Core.DataModels
{
    public class RedirectResult
    {
        public RedirectResult(string location, string serviceId, string id)
        {
            this.Location = location;
            this.ServiceId = serviceId;
            this.Id = id;
        }

        public string Location { get; set; }
        public string ServiceId { get; set; }
        public string Id { get; set; }
    }
}
