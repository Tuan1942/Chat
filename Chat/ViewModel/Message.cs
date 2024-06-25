namespace Chat.ViewModel
{
    internal class Message
    {
        public int Id { get; set; }
        public int SendId { get; set; }
        public int ReceiveId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string SentTime { get; set; }
        public bool IsIncoming { get; set; }

        public bool IsTextMessage => Type == "Message";
        public bool IsImageMessage => Type == "Image";
        public bool IsVideoMessage => Type == "Video";
        public bool IsNotTextMessage => Type != "Message";
        public string ImageSource => Type == "Image" ? Connection.Server + "Image/" + Value : null; // Nếu tin nhắn là hình ảnh, Lấy hình ảnh đó đưa vào Image Source 
        public string VideoSource => Type == "Video" ? Connection.Server + "Video/" + Value : null;

        public LayoutOptions Alignment => IsIncoming ? LayoutOptions.End : LayoutOptions.Start;
        public LayoutOptions TextAlignment => IsIncoming ? LayoutOptions.Start : LayoutOptions.End;
        public Color BackgroundColor => IsIncoming ? Color.FromHex("#D3D3D3") : Color.FromHex("#ADD8E6"); // LightGray and LightBlue
    }
}
