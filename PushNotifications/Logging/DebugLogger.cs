﻿using System;
using System.Diagnostics;

namespace PushNotifications.Logging
{
    public class DebugLogger : ILogger
    {
        public void Log(LogLevel level, string message)
        {
            Debug.WriteLine($"{DateTime.UtcNow}|Paging.NET|{level}|{message}[EOL]");
        }
    }
}