using DataDashboardSubscriptionsLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataDashboardSubscriptionsLib
{
    public class SubscriptionAccess : ISubscriptionAccess
    {
        private string DbConnectionString;

        public SubscriptionAccess(string dbConnectionString)
        {
            // We pass in the connection string when an instance is instantiated
            DbConnectionString = dbConnectionString;
        }

        public int CreateSubscription(string userId, string paymentReference, decimal amountPaid, string subscriptionReference, 
                                        bool isGDPROptIn, bool isCommsOkay, int enquiryAllocation)
        {
            // Create a new subscription and return the ID
            int subscriptionId = 0;

            if (string.IsNullOrWhiteSpace(userId) == false && string.IsNullOrWhiteSpace(paymentReference) == false && amountPaid > 0)
            {
                // Create the subscription
                using (var db = new SubscriptionContext(DbConnectionString))
                {
                    var subscription = new Subscription
                    {
                        UserId = userId,
                        PaymentReference = paymentReference,
                        AmountPaid = amountPaid,
                        SubscriptionReference = subscriptionReference,
                        IsGDPROptIn = isGDPROptIn,
                        IsCommsOkay = isCommsOkay,
                        EnquiryAllocation = enquiryAllocation
                    };

                    db.Subscriptions.Add(subscription);
                    db.SaveChanges();

                    // Get the new ID
                    subscriptionId = subscription.SubscriptionId;
                }
            }

            return subscriptionId;
        }

        public List<Subscription> GetSubscriptionsForUser(string userId)
        {
            // Get a list of subscriptions for the passed in user
            List<Subscription> subscriptions = new List<Subscription>();

            // Make sure that UserID is a GUID
            if (IsValidGUID(userId))
            {
                using (var db = new SubscriptionContext(DbConnectionString))
                {
                    subscriptions = (from s in db.Subscriptions
                                     where s.UserId == userId
                                     select s).ToList();
                }
            }

            return subscriptions;
        }

        public void SetSubscriptionIsActive(int subscriptionId, bool isActive)
        {
            // Change the IsActive property of a subscription
            using (var db = new SubscriptionContext(DbConnectionString))
            {
                Subscription subscription = (from s in db.Subscriptions
                                             where s.SubscriptionId == subscriptionId
                                             orderby s.SubscriptionId descending
                                             select s).SingleOrDefault();

                // If we find a subscription then change the property
                if (subscription != null)
                {
                    subscription.IsActive = isActive;
                    db.SaveChanges();
                }
            }
        }

        public void SetSubscriptionIsCommsOkay(int subscriptionId, bool isCommsOkay)
        {
            // Change the isCommsOkay (and by association IsGDPROptIn) property of a subscription
            using (var db = new SubscriptionContext(DbConnectionString))
            {
                Subscription subscription = (from s in db.Subscriptions
                                             where s.SubscriptionId == subscriptionId
                                             orderby s.SubscriptionId descending
                                             select s).SingleOrDefault();

                // If we find a subscription then change the property
                if (subscription != null)
                {
                    subscription.IsGDPROptIn = true; // User may opt in/out of comms but they have complied with GDPR requirement
                    subscription.IsCommsOkay = isCommsOkay;
                    db.SaveChanges();
                }
            }
        }

        public void SetSubscriptionEnquiryAllocation(int subscriptionId, int enquiryAllocation)
        {
            // Change the number of allocated enquiries
            using (var db = new SubscriptionContext(DbConnectionString))
            {
                Subscription subscription = (from s in db.Subscriptions
                                             where s.SubscriptionId == subscriptionId
                                             orderby s.SubscriptionId descending
                                             select s).SingleOrDefault();

                // If we find a subscription then change the property
                if (subscription != null)
                {
                    subscription.EnquiryAllocation = enquiryAllocation;
                    db.SaveChanges();
                }
            }
        }

        public bool IsValidSubscription(string userId)
        {
            // Is there a valid subscription for this user?
            bool isValid = false;

            // Make sure that UserID is a GUID
            if (IsValidGUID(userId))
            {
                using (var db = new SubscriptionContext(DbConnectionString))
                {
                    var subscription = (from s in db.Subscriptions where s.UserId == userId select s).FirstOrDefault();

                    // If there is a subscription
                    if (subscription != null)
                    {
                        DateTime endDate = subscription.EndDate;
                        if (endDate > DateTime.Now)
                        {
                            isValid = subscription.IsActive;
                        }
                    }
                }
            }

            return isValid;
        }

        public bool SubscriptionIsGDPROptIn(string userId)
        {
            // Has this user taken action with respect to GDPR compliance?
            bool isGDPROptIn = false;

            // Make sure that UserID is a GUID
            if (IsValidGUID(userId))
            {
                using (var db = new SubscriptionContext(DbConnectionString))
                {
                    var subscription = (from s in db.Subscriptions where s.UserId == userId select s).FirstOrDefault();

                    // If there is a subscription
                    if (subscription != null)
                    {
                        isGDPROptIn = subscription.IsGDPROptIn;
                    }
                }
            }

            return isGDPROptIn;
        }

        private static bool IsValidGUID(string guid)
        {
            // Want to check if we have a valid GUID
            string pattern = @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$";
            return Regex.IsMatch(guid, pattern, RegexOptions.Compiled);
        }
    }
}
