using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFImageLoading;
using FFImageLoading.Forms;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Net.Http;
using Xamarin.Forms;


namespace Foundation
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    public sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers;

        public bool Conditional;
    }
}
namespace mainApp
{
    [Foundation.Preserve(AllMembers = true)]
    public interface CrossPlatformUtility
    {
        //Saves text given the filename and content (text)
        void SaveText(string filename, string text);

        //Loads text given the filename
        string LoadText(string filename);

        //Gets the environment path (used to make sure we have a consistent path for local storage)
        string getEnvironmentPath();

        //Stops all notifications on Android and IOS
        void stopNotifications();

        //Starts all notifications on Android and IOS
        //The IOS bool was used to signal IOS to a change in the personal schedule that requires the notification service to be restarted
        void startNotifications(bool IOS);
    }

    [Foundation.Preserve(AllMembers = true)]
    public static class saveLoad{
        public static string loadDatabaseFromFile()
        {
            string url = string.Empty;
            string filename = "MainEventsDatabase";
            string versionName = "MainEventsDatabaseVersion";
            string currVersionStr = string.Empty;
            string onlineVersionStr = string.Empty;
            var documentsPath = Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().getEnvironmentPath();
            var filePath = Path.Combine(documentsPath, filename);
            var versionPath = Path.Combine(documentsPath, versionName);

            

            return Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().LoadText(filePath);
        }

        public static string loadMainDatabase(bool localCopy = false)
        {
            string url = string.Empty;
            string filename = "MainEventsDatabase";
            string versionName = "MainEventsDatabaseVersion";
            string currVersionStr = string.Empty;
            string onlineVersionStr = string.Empty;
            bool update = false;
            var documentsPath = Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().getEnvironmentPath();
            var filePath = Path.Combine(documentsPath, filename);
            var versionPath = Path.Combine(documentsPath, versionName);

            for(int i = 0; i < 4; i++)
            {
                switch(i){
                    case 0:
                        url = AppResources.serverIP4;
                        break;
                    case 1:
                        url = AppResources.serverIP6;
                        break;
                    case 2:
                        url = AppResources.backupServerIP4;
                        break;
                    case 3:
                        url = AppResources.backupServerIP6;
                        break;                       
                }
            
            if (url == string.Empty)
                continue;

            update = false;
            currVersionStr = Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().LoadText(versionPath);
            if (currVersionStr == string.Empty)
                update = true;
            //Check for new version of database
            using (var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 2) })
            {
                try
                {
                    onlineVersionStr = client.GetStringAsync(url + "/get_version").Result;
                }
                catch
                {
                       continue;
                }
                if (onlineVersionStr != string.Empty && currVersionStr != onlineVersionStr)
                    update = true;
                if (onlineVersionStr.ToUpper() == "NO FILE UPLOADED")
                    update = false;
                if (onlineVersionStr == currVersionStr && !localCopy)
                    update = true;
            }

                //Try loading locally if not updating database

            if(!update)
            {
                    return string.Empty;
            }

                //Update the json local database
                using (var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 6) })
                {
                    string result = string.Empty;
                    try
                    {
                        result = client.GetStringAsync(url + "/get_all").Result;
                    }
                    catch
                    {
                        continue;
                    }
                    //If the string was empty (error on the servers part), then load the local database
                    if (result == string.Empty)
                    {
                        continue;
                    }
                    //If we got results, save it
                    try
                    {
                        Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().SaveText(filename, result);
                    }
                    catch
                    {

                    }

                    //Save the version to file if we loaded it succesfully
                    try
                    {
                        Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().SaveText(versionPath, onlineVersionStr);
                    }
                    catch
                    {

                    }

                    return result;
                }
            }

            //Last ditch effor to load old Json file stored outside of the server
            using (var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 6) })
            {
                string result = string.Empty;
                try
                {
                    result = client.GetStringAsync(AppResources.lastDitchEffort).Result;
                }
                catch
                {
                    return string.Empty;
                }
                //If the string was empty (error on the servers part), then load the local database
                if (result == string.Empty)
                {
                    return string.Empty;
                }
                //If we got results, save it
                try
                {
                    Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().SaveText(filename, result);
                }
                catch
                {

                }

                return result;
            }

        }

        public static string loadMyDatabase()
        {
            string filename = "MyEventsDatabase";
            var documentsPath = Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().getEnvironmentPath();
            var filePath = Path.Combine(documentsPath, filename);
            
            string _json = Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().LoadText(filePath);
            if(_json != string.Empty)
                return _json;
            return string.Empty;
        }

        public static void saveMyDatabase(string text)
        {
            string filename = "MyEventsDatabase";
            var documentsPath = Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().getEnvironmentPath();
            var filePath = Path.Combine(documentsPath, filename);
            Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().SaveText(filename, text);
        }
    }

    [Foundation.Preserve(AllMembers = true)]
    public class EventEntries
    {
        public List<EventEntry> Events { get; protected set; }
        protected void checkNull()
        {
            if (Events == null)
                Events = new List<EventEntry>();
            checkDup();
        }
        protected void checkDup()
        {
            if (Events == null || Events.Count == 0)
                return;
            Events = Events.GroupBy(x => x.EventID).Select(y => y.First()).ToList();
        }
        public EventEntries()
        {
            Events = new List<EventEntry>();
        }
    }

    [Foundation.Preserve(AllMembers = true)]
    public class MyEventEntries : EventEntries
    {
        public MyEventEntries()
        {
            checkNull();
        }

        public string getJson()
        {
            return JsonConvert.SerializeObject(Events);
        }

        public void loadJson(string _json)
        {
            Events = JsonConvert.DeserializeObject<List<EventEntry>>(_json);
            checkNull();
            Events.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
        }

        public void removeEvent(EventEntry myEvent)
        {
            checkNull();
            
            if (!Events.Contains(myEvent))
                return;

            myEvent.inMySched = false;
            Events.Remove(myEvent);
            Events.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));

            saveDatabase();
        }

        //Override if you want to remove by eventID string
        public void removeEvent(string myEventID)
        {
            foreach(EventEntry myEvent in Events)
            {
                if (myEvent.EventID == myEventID)
                {
                    myEvent.inMySched = false;
                    Events.Remove(myEvent);
                    break;
                }
                    
            }
            Events.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            saveDatabase();
        }

        private void saveDatabase()
        {
            checkNull();
            string json = getJson();
            //Save database to file
            //This may not work
            Task.Factory.StartNew(() => { saveLoad.saveMyDatabase(json);});
            
        }

        public void addEvent(EventEntry myEvent)
        {
            checkNull();
            myEvent.inMySched = true;
            Events.Add(myEvent);
            Events.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            saveDatabase();
            Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().startNotifications(true);
        }

        public void refreshData(string _json)
        {
            try
            {
                checkNull();
                List<EventEntry> Temp = JsonConvert.DeserializeObject<List<EventEntry>>(_json);
                //Quick and dirty way to compare and update objects in the list
                for(int i = 0; i < Events.Count; i++)
                {
                    for(int j = 0; j < Temp.Count; j++)
                    {
                        if(Events[i].EventID == Temp[j].EventID)
                        {
                            Events[i] = Temp[j];
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var ee = e;
            }
            Events.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            saveDatabase();
        }
    }

    [Foundation.Preserve(AllMembers = true)]
    public class EventEntriesMain : EventEntries
    {
       public System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        public int secondsToWait = 5;
        public EventEntriesMain()
        {
            watch.Start();
            checkNull();

            //For github version only
            //Add some blank entries
            this.Events.Add(new EventEntry("Test1", "This is a test", "Test Location", "12:15pm", "", "red", "1"));
            this.Events.Add(new EventEntry("Test2", "This is a test", "Test Location", "7:15pm", "", "green", "2"));
            this.Events.Add(new EventEntry("Test3", "This is a test", "Test Location", "6:20pm", "", "blue", "3"));
            this.Events.Add(new EventEntry("Test4", "This is a test", "Test Location", "8:30pm", "", "purple", "4"));
            this.Events.Add(new EventEntry("Test5", "This is a test", "Test Location", "5:30pm", "", "orange", "5"));
            this.Events.Add(new EventEntry("Test6", "This is a test", "Test Location", "1:30pm", "", "blue", "6"));
            this.Events.Add(new EventEntry("Test7", "This is a test", "Test Location", "3:30pm", "", "green", "7"));
            this.Events.Add(new EventEntry("Test8", "This is a test", "Test Location", "7:45pm", "", "blue", "8"));
            this.Events.Add(new EventEntry("Test9", "This is a test", "Test Location", "1:30pm", "", "orange", "9"));
            this.Events.Add(new EventEntry("Test10", "This is a test", "Test Location", "2:30pm", "", "red", "10"));
        }

        public bool refreshData(bool gotFile = true)
        {
            try
            {
                if (!watch.IsRunning)
                    watch.Start();
                if (watch.ElapsedMilliseconds > secondsToWait * 1000 || Events == null || Events.Count == 0)
                {
                    string oldJson =  JsonConvert.SerializeObject(Events);
                    watch.Restart();
                    string _json = saveLoad.loadMainDatabase(gotFile);
                    if (_json == string.Empty)
                        return false;
                    if (_json == oldJson)
                        return false;
                    Events = JsonConvert.DeserializeObject<List<EventEntry>>(_json);
                    checkNull();
                    Events.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
                    this.syncWithMySched(Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<EventEntry>>(saveLoad.loadMyDatabase()));
                    return true;
                }
                return false;
            }

           
            catch(Exception e)
            {
                var ee = e;
                return false;
            }
        }
        public void syncWithMySched(List<EventEntry> events)
        {
            List<string> ids = new List<string>();
            foreach (EventEntry ev in events)
            {
                ids.Add(ev.EventID);
            }
            foreach (EventEntry ev in Events)
            {
                if (ids.Contains(ev.EventID))
                    ev.inMySched = true;
                else
                    ev.inMySched = false;
            }
        }
        public bool refreshData(string json)
        {
            try
            {
                if (json == string.Empty)
                    return false;

                 string oldJson = JsonConvert.SerializeObject(Events);
                 if (json == oldJson)
                    return false;
                Events = JsonConvert.DeserializeObject<List<EventEntry>>(json);
                checkNull();
                Events.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
                this.syncWithMySched(Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<EventEntry>>(saveLoad.loadMyDatabase()));
                return true;
            }
            catch (Exception e)
            {
                var ee = e;
                return false;
            }
        }
    }

    [Foundation.Preserve(AllMembers = true)]
    public static class MyDateTimeUtil
    {
        public static DateTime CreateDateFromTime(int year, int month, int day, DateTime time)
        {
            return new DateTime(year, month, day, time.Hour, time.Minute, 0);
        }
         public static DateTime CreateTimeFromDate(int hour, int minute, DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, hour, minute, 0);
        }
    }

    [Foundation.Preserve(AllMembers = true)]
    public class EventEntry
    {
        public bool inMySched { get; set; }
        public bool past = false;
        [JsonProperty("lead")]
        public string speaker1 { get; private set; }
        [JsonProperty("lead2")]
        public string speaker2 { get; private set; }
        [JsonProperty("contact")]
        public string contactInfo { get; private set; }
        [JsonProperty("category")]
        public string category { get; private set; }
        [JsonProperty("event_id")]
        public string EventID { get; private set; }
        [JsonProperty("title")]
        public string Title { get; private set; }
        [JsonProperty("description")]
        public string Description { get; private set; }
        [JsonProperty("location")]
        public string Location { get; private set; }
        [JsonProperty("date")]
        public string DateStr {get { return _datestr; } set
            {
                _datestr = value;
                DateTime temp = new DateTime();
                
                //Try to parse date/time from string
                if (DateTime.TryParse(value, out temp))
                    StartTime = MyDateTimeUtil.CreateDateFromTime(temp.Year, temp.Month, temp.Day, StartTime);
                else
                    Description += "\nDate: " + value;
            }
        }
        private string _datestr;
        [JsonProperty("time")]
        public string TimeStr { get { return _timestr; } set
            {
                _timestr = value;
                DateTime temp = new DateTime();
                
                //Try to parse date/time from string
                if (DateTime.TryParse(value, out temp))
                    StartTime = MyDateTimeUtil.CreateTimeFromDate(temp.Hour, temp.Minute, StartTime);
                else
                    Description += "\nTime: " + value;
            } }
        private string _timestr;
        public DateTime StartTime { get; private set; }
        [JsonProperty("duration")]
        public string DurStr
        {
            get { return _durStr; }
            set
            {
                _durStr = value;
                TimeSpan temp = new TimeSpan();
                int hour = 0;
                int minute = 0;
                if (value.ToUpper().Contains("HR"))
                {
                    hour = Convert.ToInt32(value.Remove(value.ToUpper().IndexOf("HR")));
                    value = value.Substring(value.ToUpper().IndexOf("HR") + 2);
                }
                if (value.ToUpper().Contains("HOUR"))
                {
                    hour = Convert.ToInt32(value.Remove(value.ToUpper().IndexOf("HOUR")));
                    value = value.Substring(value.ToUpper().IndexOf("HOUR") + 4);
                }  
                if (value.ToUpper().Contains("MIN"))
                    minute = Convert.ToInt32(value.Remove(value.ToUpper().IndexOf("MIN")));
                if (value.ToUpper().Contains("MINUTE"))
                    minute = Convert.ToInt32(value.Remove(value.ToUpper().IndexOf("MINUTE")));

                if (hour != 0 || minute != 0)
                {
                    EndTime = StartTime.AddHours(hour).AddMinutes(minute);
                    StartEndTime = StartTime.ToString("h:mm tt") + " - " + EndTime.ToString("h:mm tt");
                }
                    
                //Try to parse date/time from string
                else if (TimeSpan.TryParse(value, out temp))
                    EndTime = StartTime.AddHours(temp.Hours).AddMinutes(temp.Minutes);
                else
                    Description += "\nDuration: " + value;
               


            }
        }
        private string _durStr;
        public DateTime EndTime { get; private set; }
        [JsonProperty("picture_url")]
        public string ThumbnailStr { get { return thumbnailstr; } set
            {
                if (value != null && value != string.Empty && value != " ")
                    thumbnailstr = value;
            }
        }
        private string thumbnailstr;
        [JsonProperty("tag")]
        public string ColorStr {get { return _colorstr; }
            set
            {
                _colorstr = value;
                //Try to parse color from string (may need to exception handle this when the string is invalid)
                Xamarin.Forms.ColorTypeConverter converter = new Xamarin.Forms.ColorTypeConverter();
                try
                {
                    Color = (Xamarin.Forms.Color)converter.ConvertFromInvariantString(value);
                    return;
                }
                catch
                {
                    try
                    {
                        Color = (Xamarin.Forms.Color)converter.ConvertFromInvariantString(char.ToUpper(value[0]) + value.Substring(1));
                        return;
                    }
                    catch
                    {
                        Color = Xamarin.Forms.Color.White;
                    }
                    
                }
                finally
                {
                    try
                    {
                        Color = Color.WithLuminosity(0.9);
                    }
                    catch { }
                }
            } }
        private string _colorstr;
        public Xamarin.Forms.Color Color { get; private set; }
       public string StartEndTime { get; private set; }
        public EventEntry() {
            inMySched = false;
            Color = Xamarin.Forms.Color.White;
            StartTime = new DateTime();
            EndTime = new DateTime();
            Description = string.Empty;
            ThumbnailStr = AppResources.defaultPicture;
            speaker1 = string.Empty;
            speaker2 = string.Empty;
            contactInfo = string.Empty;
            category = string.Empty;
            EventID = string.Empty;
            Title = string.Empty;
            Location = string.Empty;
            StartEndTime = string.Empty;
        }

        //This is a debugging constructor only, you should only be using this class when loading from JSON
        public EventEntry(string _title, string _description, string _location, String _time, string _pictureURL, string _color, string _uid)
        {
            inMySched = false;
            Color = Xamarin.Forms.Color.White;
            StartTime = new DateTime();
            EndTime = new DateTime();
            Description = string.Empty;
            ThumbnailStr = AppResources.defaultPicture;
            speaker1 = string.Empty;
            speaker2 = string.Empty;
            contactInfo = string.Empty;
            category = string.Empty;
            EventID = string.Empty;
            Title = string.Empty;
            Location = string.Empty;
            StartEndTime = string.Empty;
            ColorStr = _color;
            DateTime temp;
            EventID = _uid;
            this.

            ThumbnailStr = "https://www.vccircle.com/wp-content/uploads/2017/03/default-profile.png";
            //Try to parse date/time from string
            bool success = DateTime.TryParse(_time, out temp);

            //If time cannot be parsed from string, attach string to begining of description
            if (!success)
            {
                Description = "Date/Time: " + _time + "\n" + _description;
                StartTime = new DateTime();
            }
            else
            {
                Description = _description;
                StartTime = temp;
                TimeStr = StartTime.ToString("H:mm:ss");
                DateStr = StartTime.ToString("MMMM dd, yyyy");
            }

            //Set the rest of the attributes
            Title = _title;
            Location = _location;
        }
    }


}

 
