﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PushNotifications.Apple;
using PushNotifications.AspNetCore;
using PushNotifications.Google;

namespace PushNotifications.AspNetCoreSample.Controllers
{
    [ApiController]
    [Route("pushnotification")]
    public class PushNotificationController : ControllerBase
    {
        private static readonly PushDevice[] pushDevices = new[]
        {
            PushDevice.Android("dBpr37I3WlI:APA91bHqhmzZVoUd2hE9Yw-s3wDOtzexg0LkDew59q0Q1hjc2a3KN0kZu0fSZpqSIej346F69q0eKm3u0WJEgG3_AOM44E3DH-AvnHM6vjIMRora-eXKyJ7kDZ5F1lpZXfNb1B0hxmeS"),
            PushDevice.Android("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"),
            PushDevice.iOS("85bea18076def67319aa2345e30ca5fbce20296e2af05640cd6036c9543dbbb3"), // Token expired
            PushDevice.iOS("235857441ce4ad2fa491c48738dafb1e456cf5d76252967bd4ceb5a4ccb11777"), // Valid Token
            PushDevice.iOS("IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII"),
        };

        private readonly ILogger<PushNotificationController> logger;
        private readonly IApnsClient apnsClient;
        private readonly IFcmClient fcmClient;
        private readonly IPushNotificationClient pushNotificationClient;

        public PushNotificationController(
            ILogger<PushNotificationController> logger,
            IApnsClient apnsClient,
            IFcmClient fcmClient,
            IPushNotificationClient pushNotificationClient)
        {
            this.logger = logger;
            this.apnsClient = apnsClient;
            this.fcmClient = fcmClient;
            this.pushNotificationClient = pushNotificationClient;
        }

        [HttpGet("send/apns")]
        public async Task<IEnumerable<ApnsResponse>> SendApnsPushNotifications()
        {
            this.logger.LogInformation("Sending APNS push notifications...");

            var responses = new List<ApnsResponse>(pushDevices.Length);

            foreach (var pushDevice in pushDevices)
            {
                var token = pushDevice.DeviceToken;

                var apnsRequest = new ApnsRequest(ApplePushType.Alert)
                     .AddToken(token)
                     .AddAlert("Test Message", $"Message from PushNotifications.AspNetCoreSample @ {DateTime.Now}")
                     .AddCustomProperty("key", "value");

                var apnsResponse = await this.apnsClient.SendAsync(apnsRequest);
                responses.Add(apnsResponse);

                if (apnsResponse.IsSuccessful)
                {
                    this.logger.LogInformation($"Successfully sent push notification to device {token}");
                }
                else
                {
                    this.logger.LogInformation($"Failed to send push notification to device {token}: {apnsResponse.Reason}");
                }
            }

            return responses;
        }

        [HttpGet("send/fcm")]
        public async Task<IEnumerable<FcmResponse>> SendFcmPushNotifications()
        {
            this.logger.LogInformation("Sending FCM push notifications...");

            var responses = new List<FcmResponse>(pushDevices.Length);

            foreach (var pushDevice in pushDevices)
            {
                var token = pushDevice.DeviceToken;

                var fcmRequest = new FcmRequest()
                {
                    To = token,
                    //RegistrationIds = pushDevices.ToList(),
                    Notification = new FcmNotification
                    {
                        Title = "Test Message",
                        Body = $"Message from PushNotifications.AspNetCoreSample @ {DateTime.Now}",
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "key", "value" }
                    },
                };

                var fcmResponse = await this.fcmClient.SendAsync(fcmRequest);
                responses.Add(fcmResponse);

                if (fcmResponse.IsSuccessful)
                {
                    this.logger.LogInformation($"Successfully sent push notification to device {token}");
                }
                else
                {
                    this.logger.LogInformation($"Failed to send push notification to device {token}: {fcmResponse.Results[0].Error}");
                }
            }

            return responses;
        }

        [HttpGet("send/x")]
        public async Task<PushResponse> SendXPushNotifications()
        {
            this.logger.LogInformation("Sending push notifications...");

            var pushRequest = new PushRequest
            {
                Content = new PushContent
                {
                    Title = "Test Message",
                    Body = $"Message from PushNotifications.AspNetCoreSample @ {DateTime.Now}",
                    CustomData = new Dictionary<string, string>
                    {
                        { "key", "value" }
                    }
                },
                Devices = pushDevices
            };

            var pushResponse = await this.pushNotificationClient.SendAsync(pushRequest);

            if (pushResponse.IsSuccessful)
            {
                this.logger.LogInformation($"Successfully sent push notification to {pushResponse.Results.Count} devices");
            }
            else
            {
                foreach (var result in pushResponse.Results)
                {
                    if (result.IsSuccessful)
                    {
                        this.logger.LogInformation($"Successfully sent push notification to DeviceToken={result.DeviceToken}");
                    }
                    else
                    {
                        this.logger.LogError($"Failed to send push notification to DeviceToken={result.DeviceToken}"); // TODO: Log reason for error here!
                    }
                }
            }

            return pushResponse;
        }
    }
}
