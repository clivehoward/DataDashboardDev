using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DataDashboardApi.Models
{
    public class CreateSubscriptionBindingModel
    {
        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Display(Name = "Payment Reference")]
        public string PaymentReference { get; set; }

        [Required]
        [Display(Name = "Subscription Reference")]
        public string SubscriptionReference { get; set; }

        [Required]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Is GDPR Opt In")]
        public bool IsGDPROptIn { get; set; } = false;

        [Display(Name = "Is Communications Okay")]
        public bool IsCommsOkay { get; set; } = false;

        [Display(Name = "Enquiry Allocation")]
        public int EnquiryAllocation { get; set; }
    }

    public class ChangeSubscriptionIsActiveModel
    {
        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }

    public class ChangeSubscriptionIsCommsOkayModel
    {
        [Required]
        [Display(Name = "Is Communications Okay")]
        public bool IsCommsOkay { get; set; }
    }

    public class ChangeSubscriptionEnquiryAllocation
    {
        [Required]
        [Display(Name = "Enquiry Allocation")]
        public int EnquiryAllocation { get; set; }
    }
}