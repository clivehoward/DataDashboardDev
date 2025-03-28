using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using DataDashboardWebLib;
using System.Text.Encodings.Web;
using System.Net;

namespace DataDashboardWeb.Pages
{
    public class ResetPasswordModel : PageModel
    {
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        [BindProperty]
        public ResetPasswordFormModel ResetPasswordForm { get; set; }

        private readonly AppSettings _appSettings;

        public ResetPasswordModel(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public void OnGet(string Code)
        {
            // Get the querystring parameter value and assign so that our hidden field can pick it up
            ResetPasswordForm = new ResetPasswordFormModel
            {
                Code = Code
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string apiUrl = _appSettings.ApiUrl;

            string email = ResetPasswordForm.Email;
            string code = ResetPasswordForm.Code;
            string password = ResetPasswordForm.Password;

            // Reset the user's password
            UserAccess userAccess = new UserAccess(apiUrl);
            bool isResult = await userAccess.ResetPassword(email, code, password);

            if (isResult)
            {
                SuccessMessage = "Password reset Successful, <a href='/Login' style='color: White;'>Log in</a>";
            }
            else
            {
                ErrorMessage = "Sorry, we were unable to reset your password";
            }

            return Page();
        }
    }

    public class ResetPasswordFormModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,15}$", ErrorMessage = "Must contain 1 upper case, 1 lower case and 1 number")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}