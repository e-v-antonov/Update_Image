using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UpdateImage.Controllers
{
    public class ActionPictureController : Controller
    {
        private readonly IHostingEnvironment he;
        public static string pathImage = string.Empty;
        public static string NameImageUpdate = string.Empty;
        public List<string> listImage = new List<string>();

        public ActionPictureController(IHostingEnvironment e)
        {
            he = e;
        }

        public IActionResult Index()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(he.WebRootPath);            
            foreach (var file in directoryInfo.GetFiles())
            {
                var typefile = Path.GetExtension(file.FullName);
                if (Path.GetExtension(file.FullName) == ".jpg" || Path.GetExtension(file.FullName) == ".png" || Path.GetExtension(file.FullName) == ".jpeg")
                    listImage.Add(new string(file.Name));
            }

            ViewBag.ListImage = listImage;

            return View("~/Views/Home/ActionPicture.cshtml");
        }

        [HttpPost]
        public IActionResult UploadImage(string fullName, IFormFile pic)
        {
            fullName = fullName + Path.GetExtension(pic.FileName);
            ViewData["fname"] = fullName;

            if (pic != null)
            {
                var fileName = Path.Combine(he.WebRootPath, fullName);
                FileStream fs = new FileStream(fileName, FileMode.Create);
                pic.CopyTo(fs);
                fs.Close();
                fs.Dispose();
                ViewData["fileLocation"] = "/" + fullName;
            }

            return View("~/Views/Home/UploadImage.cshtml");
        }

        [HttpGet]
        public IActionResult DownloadImage(string pathImage)
        {
            string pathFile = Path.Combine(he.WebRootPath, pathImage);
            string fileType = "image/jpg";
            string fileName = pathFile;
            return PhysicalFile(pathFile, fileType, fileName);
        }

        [HttpGet]
        public IActionResult UpdateImage(string nameImage)
        {
            NameImageUpdate = nameImage;
            pathImage = Path.Combine(he.WebRootPath, nameImage);
            ViewData["imageLocation"] = "/" + nameImage;

            return View("~/Views/Home/UpdateImage.cshtml");
        }
    }
}