using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BlobStore.Models.OwnClass
{
    public class ImageService : IImageService
    {
        private readonly string _imageRootPath;
        private readonly string _containerName;
        private readonly string _tableName;
        private readonly string _blobStorageConnectionString;

        public ImageService()
        {
            _imageRootPath = ConfigurationManager.AppSettings["ImageRootPath"];
            _containerName = ConfigurationManager.AppSettings["ImagesContainer"];
            _tableName = ConfigurationManager.AppSettings["ImagesTable"];
            _blobStorageConnectionString = ConfigurationManager.ConnectionStrings["BlobStorageConnectionString"].ConnectionString;
        }

        public List<UploadedImage> GetAllImagesByTable()
        {
            List<UploadedImage> upImages = new List<UploadedImage>();
            CloudTable table = GetImagesTable();
            TableQuery<UploadedImage> query = new TableQuery<UploadedImage>(); 
            foreach (UploadedImage entity in table.ExecuteQuery(query))
            {
                upImages.Add(entity);
            }
            return upImages;
        }

        public async Task<UploadedImage> CreateUploadedImage(HttpPostedFileBase file)
        {
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                byte[] fileBytes = new byte[file.ContentLength];
                await file.InputStream.ReadAsync(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                return new UploadedImage
                {
                    ContentType = file.ContentType,
                    Data = fileBytes,
                    CropData = null,
                    Name = file.FileName,
                    URLcore = string.Format("{0}/{1}", _imageRootPath, file.FileName),
                    URLcrop = ""
                };
            }
            return null;
        }

        public async Task AddImageToBlobStorageAsync(UploadedImage image)
        {
            //  get the container reference
            var container = GetImagesBlobContainer();
            // using the container reference, get a block blob reference and set its type
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(image.Name);
            blockBlob.Properties.ContentType = image.ContentType;
            // finally, upload the image into blob storage using the block blob reference
            var fileBytes = image.Data;
            await blockBlob.UploadFromByteArrayAsync(fileBytes, 0, fileBytes.Length);
        }
        public async Task AddImageToTableStorageAsync(UploadedImage upImage)
        {
            upImage.Data = null;           
            CloudTable table = GetImagesTable();
            TableOperation insertOperation = TableOperation.Insert(upImage);
            await table.ExecuteAsync(insertOperation);
            AddToQueue(upImage.RowKey);
        }

        //Get BlobContainer
        private CloudBlobContainer GetImagesBlobContainer()
        {
            // use the connection string to get the storage account
            var storageAccount = CloudStorageAccount.Parse(_blobStorageConnectionString);
            // using the storage account, create the blob client
            var blobClient = storageAccount.CreateCloudBlobClient();
            // finally, using the blob client, get a reference to our container
            var container = blobClient.GetContainerReference(_containerName);
            // if we had not created the container in the portal, this would automatically create it for us at run time
            container.CreateIfNotExists();
            // by default, blobs are private and would require your access key to download.
            //   You can allow public access to the blobs by making the container public.   
            container.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
            return container;
        }

        //Get Tables
        private CloudTable GetImagesTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_blobStorageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(_tableName);
            table.CreateIfNotExists();  
            return table;
        }

        public void AddToQueue(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "task_queue",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }
    }
}