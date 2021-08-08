﻿using System.Collections.Generic;

namespace PushNotifications.Server.ConsoleSample.Demodata
{
    public static class PushDevices
    {
        public static IEnumerable<PushDevice> Get()
        {
            yield return PushDevice.Android("ciXaQ5lURTO-IZlv35l7Yg:APA91bFN8KyBjn0PiacvbLv0vuYWGaAz42AKuP-neb5Jpzj0Rqwv9SJndi2zR-AUR00oClJmGBJc4ZjrBM4qGbEkG09ID-8-_lf8HBrTTwMPtI_EddfZi-M4fmyWFzfRdxce-2Zo1z2Q");
            yield return PushDevice.Android("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"); // Wrong token
            yield return PushDevice.iOS("85bea18076def67319aa2345e30ca5fbce20296e2af05640cd6036c9543dbbb3"); // Token expired
            yield return PushDevice.iOS("235857441ce4ad2fa491c48738dafb1e456cf5d76252967bd4ceb5a4ccb11777"); // Valid Token
            yield return PushDevice.iOS("IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII"); // Wrong token
        }
    }
}
