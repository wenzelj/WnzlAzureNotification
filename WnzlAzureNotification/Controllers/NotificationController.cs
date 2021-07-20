using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WnzlAzureNotification.Configuration;
using WnzlAzureNotification.Models;

namespace WnzlAzureNotification.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private NotificationHubClient hub;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration configuration;

        public NotificationController(IConfiguration configuration) {
            this.configuration = configuration;
            NotificationHubOptions notificationsOption = new NotificationHubOptions();
            configuration.GetSection(NotificationHubOptions.NotificationHub).Bind(notificationsOption);

            hub = NotificationHubClient.CreateClientFromConnectionString(notificationsOption.ConnectionString, notificationsOption.HubName);
        }


        [HttpGet]
        public string Get()
        {
            return "working";
        }

        public async Task<HttpResponseMessage> Post(string clientId, [FromBody] FNCNotification notification)
        {
   
            Microsoft.Azure.NotificationHubs.NotificationOutcome outcome = null;
            HttpStatusCode ret = HttpStatusCode.InternalServerError;

            string message = Newtonsoft.Json.JsonConvert.SerializeObject(notification);

            // Android
            outcome = await hub.SendFcmNativeNotificationAsync(message, clientId);
            
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