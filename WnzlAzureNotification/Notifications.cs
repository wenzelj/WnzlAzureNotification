using System;
using Microsoft.Azure.NotificationHubs;

namespace WnzlAzureNotification
{
    public class Notifications
    {
        public static Notifications Instance = new Notifications();

        public NotificationHubClient Hub { get; set; }

        private Notifications()
        {
            Hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://myappnotificationsns.servicebus.windows.net//subscriptions/c41d3dd7-c580-436a-9d2b-8314f31633a9/resourceGroups/notificationresource/providers/Microsoft.NotificationHubs/namespaces/wnzlnotification", "wnzlnotification");
        }
    }
}

