using System.ComponentModel.DataAnnotations;

namespace DataDashboardApi.Models
{
    public class AddSubmissionBindingModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Promotion Name")]
        public string PromotionName { get; set; }

        [Display(Name = "Is Communications Okay")]
        public bool IsCommsOkay { get; set; } = false;
    }
}