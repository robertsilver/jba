using jba.Services;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace jba.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var path = Path.Combine(Server.MapPath("~/UploadedDataFiles"),
                                    "OriginalDataFile.pre");

            JBAReader precipitationFile = new JBAReader(path);
            precipitationFile.RetrieveData();
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            var path = RetrievePath(file);
            if (!string.IsNullOrEmpty(path))
            {
                JBAReader precipitationFile = new JBAReader(path);
                precipitationFile.RetrieveData();
            }

            return View();
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

        private bool IsFileExtensionValid(string filename)
        {
            var dotPosn = filename.LastIndexOf(".");

            return dotPosn >= 0 && filename.Substring(dotPosn + 1) == "pre";
        }

        private string RetrievePath(HttpPostedFileBase file)
        {
            string path = string.Empty;

            if (file != null && file.ContentLength > 0)
            {
                if (IsFileExtensionValid(file.FileName))
                {
                    try
                    {
                        var now = DateTime.Now;

                        path = Path.Combine(Server.MapPath("~/UploadedDataFiles"),
                                                   $"{now.Year}-{now.Month.ToString("00")}-{now.Day.ToString("00")}-" +
                                                   $"{now.Hour.ToString("00")}-{now.Minute.ToString("00")}-{now.Second.ToString("00")}_{Path.GetFileName(file.FileName)}");
                        file.SaveAs(path);
                        ViewBag.Message = "File uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }
                }
                else
                    ViewBag.Message = "ERROR: You have not uploaded a file with the extension of .pre.  Please try again.";
            }
            else
                ViewBag.Message = "You have not specified a file.";

            return path;
        }
    }
}