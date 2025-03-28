using DataDashboardWebLib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace DataDashboardWeb.Pages
{
    [Produces("application/pdf")]
    [Route("ebookdownload")]
    public class EbookDownloadController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly IDistributedCache _distributedCache;
        private readonly IHostingEnvironment _hostingEnvironment;

        public EbookDownloadController(IOptions<AppSettings> appSettings, IHostingEnvironment hostingEnvironment, IDistributedCache distributedCache)
        {
            _appSettings = appSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public IActionResult Index(string code)
        {
            // If there is no code then send to login
            if (string.IsNullOrWhiteSpace(code) == false)
            {
                string datetime = WebUtility.UrlDecode(code);
                double oaDate = Convert.ToDouble(datetime);

                // This did not work on Azure !!!
                //// The code is an encrypted data which should be checked for a timeout
                //string encrypted = WebUtility.UrlDecode(code).Replace(" ", "+"); // Have to manually convert spaces to +

                //// Must convert the code to a byte array
                //byte[] bytes = Convert.FromBase64String(encrypted);

                //EncryptionHelper encryptionHelper = new EncryptionHelper();
                //byte[] decrypted = encryptionHelper.Decrypt(bytes);

                //double oaDate = Convert.ToDouble(Encoding.UTF8.GetString(decrypted));

                // Check the datetime against now
                DateTime dateTime = DateTime.FromOADate(oaDate);
                DateTime expiryTime = DateTime.UtcNow.AddMinutes(-30);

                //return Content(oaDate.ToString() + " - " + dateTime.ToString() + " - " + DateTime.Now.AddMinutes(-30).ToString());

                // Link must be less then 30 minutes old
                if (dateTime >= expiryTime)
                {
                    // If authenticated then return the PDF
                    string webRootPath = _hostingEnvironment.WebRootPath; // Path to application root
                    string pdfPath = webRootPath + Path.DirectorySeparatorChar.ToString() + "downloads" + Path.DirectorySeparatorChar.ToString() + "discovered-insights-ebook-2018.pdf"; // Path to PDF

                    return PhysicalFile(pdfPath, "application/pdf", "Discovered-Insights-ebook-2018.pdf");
                }
            }

            return Redirect("/Index");
        }
    }
}