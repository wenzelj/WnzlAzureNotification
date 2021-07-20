using System;
namespace WnzlAzureNotification.Models
{

    public class FNCNotification {
        public Notification notification { get; set; }
    }

    public class Notification {
        public string title { get; set; }
        public string body { get; set; }
    }

}
