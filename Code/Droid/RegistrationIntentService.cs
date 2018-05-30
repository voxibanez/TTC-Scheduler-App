using System;
using Android.App;
using Firebase.Iid;
using Android.Util;
using Firebase.Messaging;
using mainApp.Droid;
using Android.Content;
using Plugin.Settings.Abstractions;
using Plugin.Settings;

namespace mainApp.Droid
{
   
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        const string TAG = "MyFirebaseIIDService";
        public override void OnTokenRefresh()
        {
            Firebase.Messaging.FirebaseMessaging.Instance.SubscribeToTopic("TTCAdmin");
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "Refreshed token: " + refreshedToken);
            SendRegistrationToServer(refreshedToken);
        }
        void SendRegistrationToServer(string token)
        {
            // Add custom implementation, as needed.
        }
    }

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";
        public override void OnCreate()
        {
            
            base.OnCreate();
        }
        public override void OnMessageReceived(RemoteMessage message)
        {
            
            ISettings AppSettings = CrossSettings.Current;

            bool adminPushStatus = AppSettings.GetValueOrDefault("pushAdminCheckBox", true);

            if (adminPushStatus)
            {
                PushNotificationsAndroid pa = new PushNotificationsAndroid();
                pa.SendPush(message.GetNotification().Title, message.GetNotification().Body, this);
            }

        }
    }


}