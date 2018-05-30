using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FFImageLoading.Forms.Droid;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Android.Gms.Common;
using Android.Util;
using Firebase.Messaging;
using Firebase.Iid;
using JobSchedulerType = Android.App.Job.JobScheduler;
using Plugin.Settings.Abstractions;
using Plugin.Settings;
using Firebase.Analytics;

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
            while(wantToWrite || readlock || writeLock)
            {
                System.Threading.Thread.Sleep(10);
            }
            readlock = true;
            try
            {
                if (File.Exists(filePath))
                    text = File.ReadAllText(filePath);
            }
            catch {
                text = string.Empty;
            }
            readlock = false;
            return text;
        }
        public string getEnvironmentPath()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }
        public void notifyService()
        {
          //todo: notify service to change of myschedule
        }
        public void stopNotifications()
        {
            if (Convert.ToInt32(Android.OS.Build.VERSION.Sdk) < 21)
            {
                Android.App.Application.Context.StopService(new Intent(Android.App.Application.Context, typeof(mainApp.Droid.LocalNotificationService)));
            }
            else
            {
                var tm = (JobSchedulerType)Android.App.Application.Context.GetSystemService(Context.JobSchedulerService);
                tm.CancelAll();
            }

        }
        public void startNotifications(bool IOS)
        {
            if (IOS)
                return;
            if (Convert.ToInt32(Android.OS.Build.VERSION.Sdk) < 21)
            {

                mainApp.Droid.PushNotificationsAndroid push = new mainApp.Droid.PushNotificationsAndroid();
                push.SendPush("Warning: Battery Drain", "It looks like you are on android 4.4 or lower.\nNotifications on these version of android has a significant impact on battery life\nIt is reccomended that you go into settings and disable notifications", Android.App.Application.Context);
                Android.App.Application.Context.StartService(new Intent(Android.App.Application.Context, typeof(mainApp.Droid.LocalNotificationService)));
            }
            else
            {
                mainApp.Droid.jobScheduler job = new mainApp.Droid.jobScheduler(Android.App.Application.Context);
                job.ScheduleJob();
            }

        }
    }
}

namespace mainApp.Droid
{

    [Activity(Label = "TTC Schedule", Theme = "@style/MyTheme.Splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
   public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        FirebaseAnalytics firebaseAnalytics;
        const string TAG = "MainActivity";

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.Window.RequestFeature(WindowFeatures.ActionBar);
           
            //Init CachedImageRenderer
            CachedImageRenderer.Init();
            FFImageLoading.Config.Configuration ffConfig = new FFImageLoading.Config.Configuration
            {
                BitmapOptimizations = true,
                SchedulerMaxParallelTasks = 4,
                VerboseLogging = false,
                VerbosePerformanceLogging = false,
                VerboseMemoryCacheLogging = false,
                VerboseLoadingCancelledLogging = false,
                HttpClient = new System.Net.Http.HttpClient(new Xamarin.Android.Net.AndroidClientHandler())
            };
            FFImageLoading.ImageService.Instance.Initialize(ffConfig);


            //Start service on app launch


            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            this.SetTheme(Resource.Style.MyTheme);
            base.SetTheme(Resource.Style.MyTheme);
            base.OnCreate(bundle);
            
            global::Xamarin.Forms.Forms.Init(this, bundle);
            firebaseAnalytics = FirebaseAnalytics.GetInstance(this);
            LoadApplication(new App());
        }

        [BroadcastReceiver(Enabled = true, Exported = true, Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
        [IntentFilter(new string[] { "android.intent.action.BOOT_COMPLETED" })]
        public class ReceiveBoot : BroadcastReceiver
        {
            
            public override void OnReceive(Context context, Intent intent)
            {
                //When boot is complete
                ISettings AppSettings = CrossSettings.Current;

               bool pushStatus = AppSettings.GetValueOrDefault("pushRemindersCheckBox", true);
                WorkingWithFiles.SaveAndLoadDatabase temp = new WorkingWithFiles.SaveAndLoadDatabase();
                if (pushStatus)
                    temp.startNotifications(false);

            }
        }
    }
}


