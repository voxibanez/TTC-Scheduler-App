using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using UserNotifications;

namespace mainApp.iOS
{
    public static class PushNotificationsIOS
    {
        public static void SendPush(string title, string message)
        {
            var content = new UNMutableNotificationContent();
            content.Title = title;
            content.Body = message;
            content.Badge = 1;

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var requestID = "666";
            var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
                
            });

        }
    }
}