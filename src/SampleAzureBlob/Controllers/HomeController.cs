using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace SampleAzureBlob.Controllers
{
    public class HomeController : Controller
    {
        private StorageCredentials storageCredentials;
        private CloudStorageAccount cloudStorage;
        private CloudBlobClient cloudBlob;
        private CloudBlobContainer cloudblobcontainer;
        private IHostingEnvironment hostingEnv;
        public HomeController(IHostingEnvironment env)
        {
            storageCredentials = new StorageCredentials("account name", "key value");
            cloudStorage = new CloudStorageAccount(storageCredentials, true);
            cloudBlob = cloudStorage.CreateCloudBlobClient();
            cloudblobcontainer = cloudBlob.GetContainerReference("container name");
            this.hostingEnv = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(IList<IFormFile> files)
        {
            long size = 0;
            try
            {
                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName
                        .Trim('"');
                    CloudBlockBlob blockBlob = cloudblobcontainer.GetBlockBlobReference(filename);
                    var stream = file.OpenReadStream();
                    size = file.Length;
                    await blockBlob.UploadFromStreamAsync(stream, size);
                    ViewBag.Message = $"{files.Count} file(s) / {size} bytes uploaded successfully!";
                    GetBlobs();
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }

            return View("Index");
        }

        public IActionResult Index()
        {
            ViewBag.ContainerName = "Name: " + cloudblobcontainer.Name;
            GetBlobs();
            return View();
        }

        private void GetBlobs()
        {
            ViewBag.Blobs = cloudblobcontainer.ListBlobs().ToList();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
