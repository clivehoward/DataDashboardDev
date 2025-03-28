using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using DataDashboardWebLib;

namespace DataDashboardWeb.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        [BindProperty]
        public ForgotPassswordFormModel ForgotPasswordForm { get; set; }

        private readonly AppSettings _appSettings;

        public ForgotPasswordModel(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            string apiUrl = _appSettings.ApiUrl;

            UserAccess userAccess = new UserAccess(apiUrl);
            bool isSuccess = await userAccess.ForgotPassword(ForgotPasswordForm.Email);

            if (isSuccess)
            {
                SuccessMessage = "You should receive an email with further instructions.";
            }
            else
            {
                ErrorMessage = "Sorry, we were unable to reset your password.";
            }

            return Page();
        }
    }

    public class ForgotPassswordFormModel
    {
        [Required]
        public string Email { get; set; }
    }
}