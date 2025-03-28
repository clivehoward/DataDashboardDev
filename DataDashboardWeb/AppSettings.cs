using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataDashboardWeb
{
    public class AppSettings
    {
        public string ApiUrl { get; set; }
        public string RedisConfiguration { get; set; }
        public string RedisInstanceName { get; set; }
        public string AzureStorageConnection { get; set; }
        public string reCaptchaSecretKey { get; set; }
        public string StripePublicKey { get; set; }
        public string StripePrivateKey { get; set; }
        public string StripeWebhookSecret { get; set; }
    }
}
