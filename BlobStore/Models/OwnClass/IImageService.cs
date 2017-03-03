using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BlobStore.Models.OwnClass
{
    public interface IImageService
    {
        Task<UploadedImage> CreateUploadedImage(HttpPostedFileBase file);
        Task AddImageToBlobStorageAsync(UploadedImage image);
        Task AddImageToTableStorageAsync(UploadedImage _uplImage);
        List<UploadedImage> GetAllImagesByTable();
    }
}
