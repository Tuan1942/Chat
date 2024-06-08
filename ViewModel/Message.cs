using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.ViewModel
{
    internal class Message
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("sendId")]
        public int SendId { get; set; }
        [JsonProperty("receiveId")]
        public int ReceiveId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("sentTime")]
        public string SentTime { get; set; }

        public string Content => Type == "Message" ? Value : string.Empty;
        public string Timestamp => SentTime;
        public bool IsIncoming { get; set; }
        public LayoutOptions Alignment => IsIncoming ? LayoutOptions.End : LayoutOptions.Start;
        public LayoutOptions TextAlignment => IsIncoming ? LayoutOptions.Start : LayoutOptions.End;
        public Color BackgroundColor => IsIncoming ? Color.FromHex("#D3D3D3") : Color.FromHex("#ADD8E6"); // LightGray and LightBlue

        public bool IsTextMessage => Type == "Message";
        public bool IsImageMessage => Type == "Image";
        public ImageSource ImageSource => IsImageMessage ? ImageSource.FromUri(new Uri($"http://192.168.0.116:3000/image/{Value}")) : null;
    }


}
