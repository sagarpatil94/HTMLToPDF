﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PushNotifications.Apple;
using PushNotifications.Google;
using PushNotifications.Logging;

namespace PushNotifications
{
    /// <summary>
    /// Cross-platform implementation of a push notification client.
    /// </summary>
    public class PushNotificationClient : IPushNotificationClient
    {
        private readonly ILogger logger;
        private readonly IFcmClient fcmClient;
        private readonly IApnsClient apnsClient;

        public PushNotificationClient(IFcmClient fcmClient, IApnsClient apnsClient)
            : this(Logger.Current, fcmClient, apnsClient)
        {
        }

        public PushNotificationClient(ILogger logger, IFcmClient fcmClient, IApnsClient apnsClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.fcmClient = fcmClient ?? throw new ArgumentNullException(nameof(fcmClient));
            this.apnsClient = apnsClient ?? throw new ArgumentNullException(nameof(apnsClient));
        }

        public async Task<PushResponse> SendAsync(PushRequest pushRequest, CancellationToken ct = default)
        {
            var apnsResponses = new List<ApnsResponse>();
            var fcmResponses = new List<FcmResponse>();

            var apnsPushDevices = pushRequest.Devices.Where(d => d.Platform == RuntimePlatform.iOS).ToList();
            var fcmPushDevices = pushRequest.Devices.Where(d => d.Platform == RuntimePlatform.Android).ToList();
            this.logger.Log(LogLevel.Info, $"SendAsync sends PushRequest to {apnsPushDevices.Count + fcmPushDevices.Count} devices ({apnsPushDevices.Count} iOS, {fcmPushDevices.Count} Android)");

            // Handle APNS push notifications
            if (apnsPushDevices.Any())
            {
                foreach (var pushDevice in apnsPushDevices)
                {
                    var apnsRequest = new ApnsRequest(ApplePushType.Alert)
                        .AddToken(pushDevice.DeviceToken)
                        .AddAlert(pushRequest.Content.Title, pushRequest.Content.Body);

                    foreach (var item in pushRequest.Content.CustomData)
                    {
                        apnsRequest.AddCustomProperty(item.Key, item.Value);
                    }

                    var apnsResponse = await this.apnsClient.SendAsync(apnsRequest, ct);
                    apnsResponses.Add(apnsResponse);
                }
            }

            // Handle FCM push notifications
            if (fcmPushDevices.Any())
            {
                var fcmRequest = new FcmRequest()
                {
                    RegistrationIds = fcmPushDevices.Select(d => d.DeviceToken).ToList(),
                    Notification = new FcmNotification
                    {
                        Title = pushRequest.Content.Title,
                        Body = pushRequest.Content.Body,
                    },
                    Data = pushRequest.Content.CustomData
                };

                var fcmResponse = await this.fcmClient.SendAsync(fcmRequest, ct);
                fcmResponses.Add(fcmResponse);
            }

            // Map platform-specific responses to platform-agnostic response
            var apnsPushResults = apnsResponses.Select(r => new PushResponseResult(r, r.Token, r.IsSuccessful));

            var fcmPushResults = fcmResponses.SelectMany(r => r.Results.Select(x => new PushResponseResult(r, x.RegistrationId, isSuccessful: x.Error == null)));

            return new PushResponse(apnsPushResults.Union(fcmPushResults).ToList());
        }
    }
}
