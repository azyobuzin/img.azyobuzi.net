namespace ImgAzyobuziNet.Core
{
    public class ImageInfo
    {
        public string Full { get; set; }
        public string Large { get; set; }
        public string Thumb { get; set; }
        public string VideoFull { get; set; }
        public string VideoLarge { get; set; }
        public string VideoMobile { get; set; }

        public ImageInfo() { }

        public ImageInfo(string full, string large, string thumb)
        {
            this.Full = full;
            this.Large = large;
            this.Thumb = thumb;
        }

        public ImageInfo(string full, string large, string thumb, string videoFull, string videoLarge, string videoMobile)
            : this(full, large, thumb)
        {
            this.VideoFull = videoFull;
            this.VideoLarge = videoLarge;
            this.VideoMobile = videoMobile;
        }
    }
}
