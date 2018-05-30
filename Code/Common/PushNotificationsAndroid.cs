using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Content.PM;


namespace hello.Droid
{
    class PushNotificationsAndroid
    {
        public void SendPush(string title, string message, Context context)
        {
            Notification.Builder builder = new Notification.Builder(context)
    .SetContentTitle("Sample Notification")
    .SetContentText("Hello World! This is my first notification!")
    .SetSmallIcon(Resource.Drawable.icon);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                context.GetSystemService(Context.NotificationService) as NotificationManager;
            
            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }
    }
}