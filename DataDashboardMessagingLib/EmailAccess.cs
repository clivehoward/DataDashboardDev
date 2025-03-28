using DataDashboardMessagingLib.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace DataDashboardMessagingLib
{
    public class EmailAccess : IEmailAccess
    {
        private string StorageConnectionString;

        public EmailAccess(string storageConnectionString)
        {
            StorageConnectionString = storageConnectionString;
        }

        public OutgoingEmail CreateOutgoingEmail(string to, string from, string subject, string body)
        {
            // Create an OutgoingEmail object ready for sending (to our queue)
            OutgoingEmail email = new OutgoingEmail
            {
                Subject = subject,
                To = to,
                From = from,
                Body = body
            };

            return email;
        }

        public string CreateMessageBody(string templatePath, params string[] args)
        {
            string body = string.Empty;

            // Read in the template
            using (StreamReader SourceReader = File.OpenText(templatePath))
            {
                body = SourceReader.ReadToEnd();
            }

            // Now add the data
            // Originally we did this using string.Format(body, args);
            // But, that did not work once the template became HTML and had CSS using {} in it
            // So, this is the alternate solution
            int i = 0;
            foreach (string arg in args)
            {
                body = body.Replace("{" + i.ToString() + "}", arg);
                i++;
            }

            return body;
        }

        public async Task Send(OutgoingEmail email)
        {
            // Send an email by adding to an Azure Storage Queue
            // Must first serialize the email object (can only store strings in queues)
            // It's okay because the Azure Function SendGrid Binding auto-deserialises it !!
            string serializedMessage = JsonConvert.SerializeObject(email);

            // Push the message to the queue
            // Using StorageConnectionString as a param in case we choose to move the queuing functions to another Library later
            await QueuePutMessage(StorageConnectionString, "emailqueue", serializedMessage);
        }

        private static async Task QueuePutMessage(string connectionString, string queueName, string message)
        {
            CloudQueue queue = await GetQueue(connectionString, queueName);

            // Create a message and add it to the queue
            CloudQueueMessage queueMessage = new CloudQueueMessage(message);

            // Now add it to the queue
            await queue.AddMessageAsync(queueMessage);
        }

        private static async Task<CloudQueue> GetQueue(string connectionString, string queueName)
        {
            // Connect the Azure Storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create instance of the queue client
            CloudQueueClient cloudQueueClient = storageAccount.CreateCloudQueueClient();

            // Connect to a specific queue
            CloudQueue queue = cloudQueueClient.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync();

            return queue;
        }
    }
}
