using System;
using System.IO;
using Foundation;
using UserNotifications;

namespace mainApp.iOS
{


    public static class localNotifications
    {
        public static NSDateComponents DateTimeToNSDate(DateTime date)
        {
            var temp = new NSDateComponents();
            temp.Month = date.Month;
            temp.Day = date.Day;
            temp.Year = date.Year;
            temp.Hour = date.Hour;
            temp.Minute = date.Minute;
            temp.Second = 0;

            return temp;
        }

        public static void sendNotification(string title, string body){
            var content = new UNMutableNotificationContent();
            content.Title = title;
            content.Body = body;
            content.Badge = 1;

            // New trigger time
            // var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(5, false);
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            // ID of Notification to be updated
            var requestID = "TTCAdminMessage";
            var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

            // Add to system to modify existing Notification
            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
                if (err != null)
                {
                    // Do something with error...
                }
            });
        }

        public static void sendNotifications()
        {
            //Remove
            UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();

            MyEventEntries myEvents = new MyEventEntries();
            myEvents.loadJson(loadMyDatabase());
            DateTime currentTime = DateTime.Now;

            foreach (EventEntry tempEvent in myEvents.Events)
            {
                // Rebuild notification
                var content = new UNMutableNotificationContent();
                content.Title = "Event: " + tempEvent.Title + " starting soon";
                content.Body = "Starts at " + tempEvent.StartTime.ToString("h:mm tt") + "\nLocation: " + tempEvent.Location;
                content.Badge = 1;

                // New trigger time
                // var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(5, false);
                var trigger = UNCalendarNotificationTrigger.CreateTrigger(DateTimeToNSDate(tempEvent.StartTime), false);
                // ID of Notification to be updated
                var requestID = tempEvent.EventID;
                var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

                // Add to system to modify existing Notification
                UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
                    if (err != null)
                    {
                        // Do something with error...
                    }
                });
            }
        }


        public static string loadMyDatabase()
        {
            try
            {
                string filename = "MyEventsDatabase";
                var documentsPath = getEnvironmentPath();
                var filePath = Path.Combine(documentsPath, filename);
                var temp = new WorkingWithFiles.SaveAndLoadDatabase();
                string json = Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().LoadText(filePath);
                return json;
            }
            catch(Exception e){
                var ee = e;
            }
            return string.Empty;
        }

        public static string getEnvironmentPath()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }

    }
}
