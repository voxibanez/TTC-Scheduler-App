using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Threading;
using Android.Util;
using Java.Interop;
using Android.App.Job;
using JobSchedulerType = Android.App.Job.JobScheduler;
using System.IO;

namespace mainApp.Droid
{
    #region PushNotifications
    public class PushNotificationsAndroid
    {
        //SendPush
        //Given a title, message, context, and icon (optional), a push notification is sent locally to the android device
        public void SendPush(string title, string message, Context context)
        {
            // Set up an intent so that tapping the notifications returns to this app:
            // Assuming that context will always be the MainActivity context
            Intent intent = new Intent(context, typeof(MainActivity));

            const int pendingIntentId = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            Notification.Builder builder = new Notification.Builder(context)
                .SetContentIntent(pendingIntent)
                .SetTicker(title)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.TTC_App_Logo)
                .SetAutoCancel(true);
                //.AddAction(Resource.Drawable.abc_edit_text_material, "Snooze 5 Min", PendingIntent.GetBroadcast());

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
    #endregion

    #region Legacy
    [Service]
    public class LocalNotificationService : Service
    {
        static readonly string TAG = "X:" + typeof(LocalNotificationService).Name;
        public int TimerWait = 5 * 60 * 1000;
        Timer timer;
        DateTime startTime;
        bool isStarted = false;
        PushNotificationsAndroid push = new PushNotificationsAndroid();
        private MyEventEntries myEvents = new MyEventEntries();
        private List<EventEntries> notifiedEntries = new List<EventEntries>();
        //private List<EventEntry> myEvents = new List<EventEntry>();
        //sContext mainActivityContext = new MainActivity();

        private List<EventEntry> events = new List<EventEntry>();
        public override void OnCreate()
        {
            DateTime current = DateTime.Now;
            //this.addEvent(new EventEntry("test", "test1", "Teradyne", new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0),"","green"));
            base.OnCreate();
        }

        public override IBinder OnBind(Intent intent)
        {
            // This is a started service, not a bound service, so we just return null.
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(TAG, $"OnStartCommand called at {startTime}, flags={flags}, startid={startId}");
            if (isStarted)
            {
                myEvents.loadJson(saveLoad.loadMyDatabase());
                TimeSpan runtime = DateTime.UtcNow.Subtract(startTime);
                Log.Debug(TAG, $"This service was already started, it's been running for {runtime:c}.");
            }
            else
            {
                startTime = DateTime.UtcNow;
                Log.Debug(TAG, $"Starting the service, at {startTime}.");
                timer = new Timer(HandleTimerCallback, startTime, 0, TimerWait);
               
                isStarted = true;
            }
            return StartCommandResult.Sticky;
        }

        void HandleTimerCallback(object state)
        {
            TimeSpan runTime = DateTime.UtcNow.Subtract(startTime);
            Log.Debug(TAG, $"This service has been running for {runTime:c} (since ${state}).");
            checkEvents();
        }

        public override void OnDestroy()
        {
            timer.Dispose();
            timer = null;
            isStarted = false;

            TimeSpan runtime = DateTime.UtcNow.Subtract(startTime);
            Log.Debug(TAG, $"Simple Service destroyed at {DateTime.UtcNow} after running for {runtime:c}.");
            base.OnDestroy();
        }

        public void addEvent(EventEntry _event)
        {
            events.Add(_event);
        }

        public void removeEvent(EventEntry _event)
        {
            //This may not work in every scenario
            events.Remove(_event);
        }

        private void checkEvents()
        {
            myEvents.loadJson(saveLoad.loadMyDatabase());
            DateTime currentTime = DateTime.Now;
            List<EventEntry> temp = new List<EventEntry>();

            foreach (EventEntry tempEvent in myEvents.Events)
            {
                int timeInterval = 15;
                int day = tempEvent.StartTime.Day;
                int month = tempEvent.StartTime.Month;
                int year = tempEvent.StartTime.Year;

                if (day == currentTime.Day && month == currentTime.Month && year == currentTime.Year)
                {
                    int hour = tempEvent.StartTime.Hour;
                    int minute = tempEvent.StartTime.Minute + hour * 60;
                    int minuteNow = currentTime.Minute + currentTime.Hour * 60;
                    if (minute - minuteNow < timeInterval && minute - minuteNow > 0)
                        temp.Add(tempEvent);
                }
            }
            if(temp.Count > 1)
            {
                string tempString = string.Empty;
                foreach(EventEntry tempEvent in temp)
                {
                    tempString += tempEvent.Title + "\n";
                }
                push.SendPush(temp.Count + " Events coming up in the next 15 minutes", tempString, this);
            }
                
            else if(temp.Count == 1)
                push.SendPush("Event: " + temp[0].Title + " starting soon", "Starts at " + temp[0].StartTime.ToString("h:mm tt") + "\nLocation: " + temp[0].Location, this);
        }

    }
#endregion

    #region JobScheduler
    public class jobScheduler
    {
        LocalNotificationJobService notificationService;
        ComponentName serviceComponent;
        Context mainContext;
        public int kJobId = 0;
        double periodInMins = 5;

        public jobScheduler(Context cont)
        {
            mainContext = cont;
            serviceComponent = new ComponentName(mainContext, Java.Lang.Class.FromType(typeof(LocalNotificationJobService)));
        }

        /* UI onclick listener to schedule a job.What this job is is defined in
          * TestJobService#scheduleJob().
          */

        public void ScheduleJob()
        {
            var builder = new JobInfo.Builder(kJobId++, serviceComponent);


            //builder.SetMinimumLatency(periodInMins * 60 * 1000);
            builder.SetPeriodic((long)periodInMins * 60 * 1000);
            //builder.SetPersisted(true);
            //The service can wait up to an aditional 30 seconds before it must execute
     

            builder.SetRequiresDeviceIdle(false);
            builder.SetRequiresCharging(false);
            notificationService = new LocalNotificationJobService(mainContext);
            notificationService.ScheduleJob(builder.Build());
        }

        [Export("cancelAllJobs")]
        public void CancelAllJobs(View v)
        {
            var tm = (JobSchedulerType)mainContext.GetSystemService(Context.JobSchedulerService);
            tm.CancelAll();
        }

        /**
	     * UI onclick listener to call jobFinished() in our service.
	     */
        [Export("finishJob")]
        public void FinishJob(View v)
        {
            notificationService.CallJobFinished();
        }
   
}

    [Service(Exported = true, Permission = "android.permission.BIND_JOB_SERVICE")]
    public class LocalNotificationJobService : JobService
    {
        Context mainContext;
        private static string Tag = "SyncService";
        MainActivity owner;
        private readonly List<JobParameters> jobParamsMap = new List<JobParameters>();
        
        public LocalNotificationJobService()
        {
            mainContext = Android.App.Application.Context;
        }

        public LocalNotificationJobService(Context main)
        {
            mainContext = main;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(Tag, "Service created");
        }

        public override void OnDestroy()
        {
            Log.Info(Tag, "Service destroyed");
            base.OnDestroy();
        }

        public override StartCommandResult OnStartCommand(Intent intent, Android.App.StartCommandFlags flags, int startId)
        {
            return StartCommandResult.NotSticky;
        }

        public override bool OnStartJob(JobParameters args)
        {
            // We don't do any real 'work' in this sample app. All we'll
            // do is track which jobs have landed on our service, and
            // update the UI accordingly.
            PushNotificationsAndroid push = new PushNotificationsAndroid();
            jobParamsMap.Add(args);
            try
            {
                checkEvents();
            }
            catch
            {
                push.SendPush("Error", "Sorry, there was an issue with the background notification service, please notify the TTC app admin", this);
            }
            
            this.JobFinished(args, false);
            return true;
        }

        public override bool OnStopJob(JobParameters args)
        {
            // Stop tracking these job parameters, as we've 'finished' executing.
            jobParamsMap.Remove(args);

            Log.Info(Tag, "on stop job: " + args.JobId);
            return true;
        }

        public void setUiCallback(MainActivity activity)
        {
            owner = activity;
        }

        /** Send job to the JobScheduler. */
        public void ScheduleJob(JobInfo t)
        {
            var tm = (JobSchedulerType)mainContext.GetSystemService(Context.JobSchedulerService);
            var status = tm.Schedule(t);
            Log.Info(Tag, "Scheduling job: " + (status == JobSchedulerType.ResultSuccess ? "Success" : "Failure"));
        }

        /**
	     * Called when Task Finished button is pressed. 
	     */
        public bool CallJobFinished()
        {
            if (jobParamsMap.Count == 0)
            {
                return false;
            }
            else
            {
                var args = jobParamsMap[0];
                jobParamsMap.Remove(args);
                JobFinished(args, false);
                return true;
            }
        }

        private void checkEvents()
        {
            
            MyEventEntries myEvents = new MyEventEntries();
            PushNotificationsAndroid push = new PushNotificationsAndroid();
            myEvents.loadJson(loadMyDatabase());
            DateTime currentTime = DateTime.Now;
            List<EventEntry> temp = new List<EventEntry>();

            foreach (EventEntry tempEvent in myEvents.Events)
            {
                int timeInterval = 15;
                int day = tempEvent.StartTime.Day;
                int month = tempEvent.StartTime.Month;
                int year = tempEvent.StartTime.Year;

                if (day == currentTime.Day && month == currentTime.Month && year == currentTime.Year)
                {
                    int hour = tempEvent.StartTime.Hour;
                    int minute = tempEvent.StartTime.Minute + hour * 60;
                    int minuteNow = currentTime.Minute + currentTime.Hour * 60;
                    if (minute - minuteNow < timeInterval && minute - minuteNow > 0)
                        temp.Add(tempEvent);
                }
            }
            if (temp.Count > 1)
            {
                string tempString = string.Empty;
                foreach (EventEntry tempEvent in temp)
                {
                    tempString += tempEvent.Title + "\n";
                }
                push.SendPush(temp.Count + " Events coming up in the next 15 minutes", tempString, this);
            }

            else if (temp.Count == 1)
                push.SendPush("Event: " + temp[0].Title + " starting soon", "Starts at " + temp[0].StartTime.ToString("h:mm tt") + "\nLocation: " + temp[0].Location, this);
        }

        public string loadMyDatabase()
        {
            string filename = "MyEventsDatabase";
            var documentsPath = getEnvironmentPath();
            var filePath = Path.Combine(documentsPath, filename);

            string _json = LoadText(filePath);
            if (_json != string.Empty)
                return _json;
            return string.Empty;
        }
        public string getEnvironmentPath()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }
        public void SaveText(string filename, string text)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(documentsPath, filename);
            try
            {
                System.IO.File.WriteAllText(filePath, text);
            }
            catch { }
        }
        public string LoadText(string filename)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(documentsPath, filename);
            string text = string.Empty;
            try
            {
                if (File.Exists(filePath))
                    text = File.ReadAllText(filePath);
            }
            catch
            {
                text = string.Empty;
            }
            return text;
        }

    }
#endregion
}
