using System;
namespace WnzlAzureNotification.Configuration
{
    public class NotificationHubOptions
    {
        public const string NotificationHub = "NotificationHub";
        public string ConnectionString { get; set; }
        public string HubName { get; set; }
    }
}
