using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.ViewModel
{
    internal class ImageItem
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string CompressedPath { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
