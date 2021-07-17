﻿namespace PushNotifications.AspNetCore
{
    public class ApnsServiceOptions
    {
        /// <summary>
        /// Do not perform a server certificate validation when establishing connection with APNs.
        /// Potentially dangerous option that shouldn't be used in production.
        /// </summary>
        public bool DisableServerCertificateValidation { get; set; }
    }
}
