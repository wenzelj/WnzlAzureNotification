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
using WnzlAzureNotification.Models;

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


        //// Custom API on the backend
        //[HttpPut]
        //public async Task<HttpResponseMessage> Put(string clientId, string token)
        //{

        //    //this is notification hub client  
        //    var registrationId = await hub.CreateRegistrationIdAsync();
           
        //    var registration = new GcmRegistrationDescription(token)
        //    {
        //        //one we got in previous call.
        //        RegistrationId = registrationId,
        //        Tags = new HashSet<string> {
        //        clientId }
        //    };


        //    await hub.CreateOrUpdateRegistrationAsync(registration);

        //    return new HttpResponseMessage(HttpStatusCode.OK);
        //}

        [HttpPut]
        public async Task<HttpResponseMessage> Put(RegistrationRequest registrationRequest)
        {
            var registrations = getRegistrationsByTag(registrationRequest.clientId);

            // need to get all client registrations by tag
            // loop the registrations and check if token already exist for device.
            // Add token if different device and token not exist 
            
            string registrationId = getRegistrationIdByTag(registrationRequest.clientId);
            if (string.IsNullOrEmpty(registrationId)) {
                // No registration for tag. Get a new one
                registrationId = await hub.CreateRegistrationIdAsync();
            }

            //TODO we have to handle different types of registrations Android and IOS 
            var registration = new FcmRegistrationDescription(registrationRequest.token)
            {
                //one we got in previous call.
                RegistrationId = registrationId,
                Tags = new HashSet<string> {
                registrationRequest.clientId }
            };

            await hub.CreateOrUpdateRegistrationAsync(registration);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        //// POST api/register
        //// This creates a registration id
        //[HttpPost]
        //public async Task<string> Post(string handle = null)
        //{
        //    string newRegistrationId = null;

        //    // make sure there are no existing registrations for this push handle (used for iOS and Android)
        //    if (handle != null)
        //    {
        //        var registrations = await hub.GetRegistrationsByChannelAsync(handle, 100);

        //        foreach (RegistrationDescription registration in registrations)
        //        {
        //            if (newRegistrationId == null)
        //            {
        //                newRegistrationId = registration.RegistrationId;
        //            }
        //            else
        //            {
        //                await hub.DeleteRegistrationAsync(registration);
        //            }
        //        }
        //    }

        //    if (newRegistrationId == null)
        //        newRegistrationId = await hub.CreateRegistrationIdAsync();

        //    return newRegistrationId;
        //}

        private string getRegistrationIdByTag(string tag) {
            var registrants = hub.GetRegistrationsByTagAsync(tag, 1);
            var client = registrants.Result.FirstOrDefault();
            if (client != null)
            {
                return client.RegistrationId;
            }

            return null;
        }

        private List<RegistrationDescription> getRegistrationsByTag(string tag) {
            var registrations = hub.GetRegistrationsByTagAsync(tag, 1);
            return registrations.Result.ToList();
        }


        // DELETE api/register/5
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete(string clientId)
        {
            //var registrants = await hub.GetRegistrationsByTagAsync(clientId, 1);
            //var client = registrants.FirstOrDefault();
            string id = getRegistrationIdByTag(clientId);
            if (!string.IsNullOrEmpty(id)) {
                await hub.DeleteRegistrationAsync(id);
            }
            
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