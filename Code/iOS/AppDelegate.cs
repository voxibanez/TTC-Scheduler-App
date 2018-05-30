using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.IO;
using UserNotifications;
using FFImageLoading.Forms.Touch;
using Firebase.Analytics;
using Firebase.CloudMessaging;
using Firebase.Core;
using Plugin.Settings.Abstractions;
using Plugin.Settings;

[assembly: Xamarin.Forms.Dependency(typeof(WorkingWithFiles.SaveAndLoadDatabase))]
namespace WorkingWithFiles
{

    public class SaveAndLoadDatabase : mainApp.CrossPlatformUtility
    {
        private bool wantToWrite = false;
        private bool writeLock = false;
        private bool readlock = false;
        public void SaveText(string filename, string text)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(documentsPath, filename);
            wantToWrite = true;
            while (writeLock || readlock)
            {
                System.Threading.Thread.Sleep(10);
            }
            writeLock = true;
            try
            {
                System.IO.File.WriteAllText(filePath, text);
            }
            catch { }
            writeLock = false;
            wantToWrite = false;
        }
        public string LoadText(string filename)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(documentsPath, filename);
            string text = string.Empty;
            while (wantToWrite || readlock || writeLock)
            {
                System.Threading.Thread.Sleep(10);
            }
            readlock = true;
            try
            {
                if (File.Exists(filePath))
                    text = File.ReadAllText(filePath);
            }
            catch
            {
                text = string.Empty;
            }
            readlock = false;
            return text;
        }
        public string getEnvironmentPath()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }
        public void stopNotifications()
        {
            UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
        }
        public void startNotifications(bool IOS)
        {
            mainApp.iOS.localNotifications.sendNotifications();
        }
    }
}
namespace mainApp.iOS
{


    [Register("AppDelegate")]
    //public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    public partial class AppDelegate : XLabs.Forms.XFormsApplicationDelegate, IUNUserNotificationCenterDelegate
    {
        private static ISettings AppSettings =>
CrossSettings.Current;


        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.Daddoon.iOS.IconTabbedPageRenderer.Initialize();
            //App.Configure();
            // Code for startilng up the Xamarin Test Cloud Agent
#if DEBUG
			Xamarin.Calabash.Start();
#endif
         




            CachedImageRenderer.Init();
            //Requests permission for push notifications
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) => {});

            LoadApplication(new App());

            // get permission for notification
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // iOS 10
                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
                {
                    Console.WriteLine(granted);
                });

                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;
            }
            else
            {
                // iOS 9 <=
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }

            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            // Firebase component initialize
            Firebase.Core.App.Configure();

            Firebase.InstanceID.InstanceId.Notifications.ObserveTokenRefresh((sender, e) => {
                var newToken = Firebase.InstanceID.InstanceId.SharedInstance.Token;
                // if you want to send notification per user, use this token
                System.Diagnostics.Debug.WriteLine(newToken);

                connectFCM();
            });
            return base.FinishedLaunching(app, options);
        }

        // iOS 9 <=, fire when recieve notification foreground
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            if (!AppSettings.GetValueOrDefault("pushAdminCheckBox", true))
                return;
            Messaging.SharedInstance.AppDidReceiveMessage(userInfo);

            // Generate custom event
            NSString[] keys = { new NSString("Event_type") };
            NSObject[] values = { new NSString("Recieve_Notification") };
            var parameters = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(keys, values, keys.Length);

            // Send custom event
            Firebase.Analytics.Analytics.LogEvent("CustomEvent", parameters);

            if (application.ApplicationState == UIApplicationState.Active)
            {
                System.Diagnostics.Debug.WriteLine(userInfo);
                var aps_d = userInfo["aps"] as NSDictionary;
                var alert_d = aps_d["alert"] as NSDictionary;
                var body = alert_d["body"] as NSString;
                var title = alert_d["title"] as NSString;
                debugAlert(title, body);
            }
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
#if DEBUG
            Firebase.InstanceID.InstanceId.SharedInstance.SetApnsToken(deviceToken, Firebase.InstanceID.ApnsTokenType.Sandbox);
#endif
#if RELEASE
            Firebase.InstanceID.InstanceId.SharedInstance.SetApnsToken(deviceToken, Firebase.InstanceID.ApnsTokenType.Prod);
#endif
        }

        // iOS 10, fire when recieve notification foreground
        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            completionHandler(UNNotificationPresentationOptions.Alert);
            //var title = notification.Request.Content.Title;
            //var body = notification.Request.Content.Body;
            //debugAlert(title, body);
            //localNotifications.sendNotification(notification.Request.Content.Title,notification.Request.Content.Body);
        }

        private void connectFCM()
        {
            if (!AppSettings.GetValueOrDefault("pushAdminCheckBox", true))
                return;
            Messaging.SharedInstance.ShouldEstablishDirectChannel = true;
            Messaging.SharedInstance.Subscribe("/topics/TTCAdmin");
        }

        public override void DidEnterBackground(UIApplication uiApplication)
        {
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
          //  Messaging.SharedInstance.Disconnect();
           // Messaging.SharedInstance.ShouldEstablishDirectChannel = false;
        }

        public override void OnActivated(UIApplication uiApplication)

        {
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            connectFCM();
            base.OnActivated(uiApplication);
        }


        void TokenRefreshNotification(object sender, NSNotificationEventArgs e)
        {
            // This method will be fired everytime a new token is generated, including the first
            // time. So if you need to retrieve the token as soon as it is available this is where that
            // should be done.
            //var refreshedToken = InstanceId.SharedInstance.Token;

            connectFCM();

            // TODO: If necessary send token to application server.
        }

        public static void ShowMessage(string title, string message, UIViewController fromViewController, Action actionForOk = null)
        {
            if (!AppSettings.GetValueOrDefault("pushAdminCheckBox", true))
                return;
            localNotifications.sendNotification(title, message);
        }

        private void debugAlert(string title, string message)
        {
            if (!AppSettings.GetValueOrDefault("pushAdminCheckBox", true))
                return;
            localNotifications.sendNotification(title, message);
        }
    }


}