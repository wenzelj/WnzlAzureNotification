using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using System.Web;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WnzlAzureNotification.Configuration;

namespace WnzlAzureNotification.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class RegisterController : ControllerBase
    {
        private NotificationHubClient hub;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration configuration;

        public RegisterController(IConfiguration configuration)
        {
            this.configuration = configuration;
            NotificationHubOptions notificationsOption = new NotificationHubOptions();
            configuration.GetSection(NotificationHubOptions.NotificationHub).Bind(notificationsOption);
           
            hub = NotificationHubClient.CreateClientFromConnectionString(notificationsOption.ConnectionString, notificationsOption.HubName);
        }

        public class DeviceRegistration
        {
            public string Platform { get; set; }
            public string Handle { get; set; }
            public string[] Tags { get; set; }
        }


        [HttpGet]
        public string Get()
        {
            return "working";
        }

        //[HttpPut]
        //public string Put(string clientId, string token)
        //{
        //    return clientId;
        //}



        // Custom API on the backend
        [HttpPut]
        public async Task<HttpResponseMessage> Put(string clientId, string token)
        {

            //this is notification hub client  
            var registrationId = await hub.CreateRegistrationIdAsync();

            //Installation installation = new Installation();
            //installation.InstallationId = deviceUpdate.InstallationId;
            //installation.PushChannel = deviceUpdate.Handle;
            //installation.Tags = deviceUpdate.Tags;

            //switch (platform)
            //{
            //    case NotificationPlatform.Mpns:
            //        //installation.Platform = NotificationPlatform.Mpns;

            //        break;
            //    case NotificationPlatform.Wns:
            //        //installation.Platform = NotificationPlatform.Wns;
            //        break;
            //    case NotificationPlatform.Apns:
            //        //installation.Platform = NotificationPlatform.Apns;
            //        break;
            //    case NotificationPlatform.Fcm:
            //        registration = new GcmRegistrationDescription(token);
            //        //installation.Platform = NotificationPlatform.Fcm;
            //        break;
            //    default:
            //        break;
            //        //throw new HttpResponseException(HttpStatusCode.BadRequest);
            //}

            var registration = new GcmRegistrationDescription(token)
            {
                //one we got in previous call.
                RegistrationId = registrationId,
                Tags = new HashSet<string> {
                clientId }
            };



            // In the backend we can control if a user is allowed to add tags
            //installation.Tags = new List<string>(deviceUpdate.Tags);
            //installation.Tags.Add("username:" + username);

            await hub.CreateOrUpdateRegistrationAsync(registration);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        //// Custom API on the backend
        //public async Task<HttpResponseMessage> Put(DeviceInstallation deviceUpdate)
        //{

        //    Installation installation = new Installation();
        //    installation.InstallationId = deviceUpdate.InstallationId;
        //    installation.PushChannel = deviceUpdate.Handle;
        //    installation.Tags = deviceUpdate.Tags;

        //    switch (deviceUpdate.Platform)
        //    {
        //        case "mpns":
        //            installation.Platform = NotificationPlatform.Mpns;
        //            break;
        //        case "wns":
        //            installation.Platform = NotificationPlatform.Wns;
        //            break;
        //        case "apns":
        //            installation.Platform = NotificationPlatform.Apns;
        //            break;
        //        case "fcm":
        //            installation.Platform = NotificationPlatform.Fcm;
        //            break;
        //        default:
        //            throw new HttpResponseException(HttpStatusCode.BadRequest);
        //    }


        //    // In the backend we can control if a user is allowed to add tags
        //    //installation.Tags = new List<string>(deviceUpdate.Tags);
        //    //installation.Tags.Add("username:" + username);

        //    await hub.CreateOrUpdateInstallationAsync(installation);

        //    return Request.CreateResponse(HttpStatusCode.OK);
        //}


        // POST api/register
        // This creates a registration id
        [HttpPost]
        public async Task<string> Post(string handle = null)
        {
            string newRegistrationId = null;

            // make sure there are no existing registrations for this push handle (used for iOS and Android)
            if (handle != null)
            {
                var registrations = await hub.GetRegistrationsByChannelAsync(handle, 100);

                foreach (RegistrationDescription registration in registrations)
                {
                    if (newRegistrationId == null)
                    {
                        newRegistrationId = registration.RegistrationId;
                    }
                    else
                    {
                        await hub.DeleteRegistrationAsync(registration);
                    }
                }
            }

            if (newRegistrationId == null)
                newRegistrationId = await hub.CreateRegistrationIdAsync();

            return newRegistrationId;
        }


       

        // DELETE api/register/5
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete(string clientId)
        {
            await hub.DeleteRegistrationAsync(clientId);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        {
            var webex = e.InnerException as WebException;
            if (webex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)webex.Response;
                if (response.StatusCode == HttpStatusCode.Gone)
                    throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }
        }



    }
}