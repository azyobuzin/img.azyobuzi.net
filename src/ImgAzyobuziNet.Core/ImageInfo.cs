namespace ImgAzyobuziNet.Core
{
    public class ImageInfo
    {
        public ImageInfo() { }

        public ImageInfo(string full, string large, string thumb, string video = null)
        {
            this.Full = full;
            this.Large = large;
            this.Thumb = thumb;
            this.Video = video;
        }

        public string Full { get; set; }
        public string Large { get; set; }
        public string Thumb { get; set; }
        public string Video { get; set; }
    }
}
