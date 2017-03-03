using BlobStore.Models.OwnClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlobStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IImageService _imageService = new ImageService();
        public ActionResult Index()
        {
            List<UploadedImage> upImages = _imageService.GetAllImagesByTable();
            return View(upImages);
        }

        [HttpGet]
        public ActionResult Upload()
        {
            var model = new UploadedImage();
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Upload(FormCollection formCollection)
        {
            var uploadedImage = new UploadedImage();
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["uploadedFile"];
                uploadedImage = await _imageService.CreateUploadedImage(file);
                await _imageService.AddImageToBlobStorageAsync(uploadedImage);
                await _imageService.AddImageToTableStorageAsync(uploadedImage);
            }
            return View("Upload", uploadedImage);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}