using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DataDashboardWeb.Pages
{
    public class IndexModel : PageModel
    {
        public string HeroImage;

        public void OnGet()
        {
            // Random hero image
            Random random = new Random();
            int randomNumber = 1;

            randomNumber = random.Next(1, 9);

            string imageName = "hero-" + randomNumber.ToString() + ".png";

            HeroImage = imageName;
        }
    }
}
