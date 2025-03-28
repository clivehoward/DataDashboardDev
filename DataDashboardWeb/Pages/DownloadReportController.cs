using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.IO;

namespace DataDashboardWeb.Pages
{
    [Produces("application/pdf")]
    [Route("download/Discovered-Insights-2018-Companion-Report-Understanding-Digital-Transformation")]
    public class DownloadReportController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly IDistributedCache _distributedCache;
        private readonly IHostingEnvironment _hostingEnvironment;

        public DownloadReportController(IOptions<AppSettings> appSettings, IHostingEnvironment hostingEnvironment, IDistributedCache distributedCache)
        {
            _appSettings = appSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // NOTE: ////////////////////////////////////////////////////////////////////////////////////////////////
            // This would probably be better if the file and it's download name were stored in a DB or Azure Storage
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Get our authentication token from session state
            var token = HttpContext.Session.GetString("Token");

            // If there is no token then send to login
            if (token == null)
            {
                return Redirect("/Login");
            }

            // If authenticated then return the PDF
            string webRootPath = _hostingEnvironment.WebRootPath; // Path to application root
            string pdfPath = webRootPath + Path.DirectorySeparatorChar.ToString() + "reports" + Path.DirectorySeparatorChar.ToString() + "2018.pdf"; // Path to PDF

            return PhysicalFile(pdfPath, "application/pdf", "Discovered-Insights-2018-Companion-Report-Understanding-Digital-Transformation.pdf");
        }
    }
}