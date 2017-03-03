using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlobStore.Models.OwnClass
{
    public class UploadedImage : TableEntity
    {
        public UploadedImage()
        {
            this.PartitionKey = DateTime.Now.ToString();
            this.RowKey = Guid.NewGuid().ToString();
        }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public byte[] CropData { get; set; }
        public string URLcore { get; set; }
        public string URLcrop { get; set; }
    }
}