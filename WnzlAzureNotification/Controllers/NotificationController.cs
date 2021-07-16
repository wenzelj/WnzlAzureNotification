using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WnzlAzureNotification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        public async Task<HttpResponseMessage> Post(string pns, [FromBody] string message, string to_tag)
        {
            var user = "wenzel";
            string[] userTag = new string[2];
            userTag[0] = "username:" + to_tag;
            userTag[1] = "from:" + user;

            Microsoft.Azure.NotificationHubs.NotificationOutcome outcome = null;
            HttpStatusCode ret = HttpStatusCode.InternalServerError;

            // Android
            var notif = "{ \"data\" : {\"message\":\"" + "From " + user + ": " + message + "\"}}";
            outcome = await Notifications.Instance.Hub.SendFcmNativeNotificationAsync(notif, userTag);
            
            //switch (pns.ToLower())
            //{
            //    case "wns":
            //        // Windows 8.1 / Windows Phone 8.1
            //        var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">" +
            //                    "From " + user + ": " + message + "</text></binding></visual></toast>";
            //        outcome = await Notifications.Instance.Hub.SendWindowsNativeNotificationAsync(toast, userTag);
            //        break;
            //    case "apns":
            //        // iOS
            //        var alert = "{\"aps\":{\"alert\":\"" + "From " + user + ": " + message + "\"}}";
            //        outcome = await Notifications.Instance.Hub.SendAppleNativeNotificationAsync(alert, userTag);
            //        break;
            //    case "fcm":
            //        // Android
            //        var notif = "{ \"data\" : {\"message\":\"" + "From " + user + ": " + message + "\"}}";
            //        outcome = await Notifications.Instance.Hub.SendFcmNativeNotificationAsync(notif, userTag);
            //        break;
            //}

            if (outcome != null)
            {
                if (!((outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Abandoned) ||
                    (outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Unknown)))
                {
                    ret = HttpStatusCode.OK;
                }
            }

            return new HttpResponseMessage(ret);
        }

    }
}