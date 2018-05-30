using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mainApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MySchedulePage : ContentPage
    {
        private static Thickness GetPagePadding()
        {
            double topPadding;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    topPadding = 20;
                    break;
                default:
                    topPadding = 20;
                    break;
            }

            return new Thickness(10, topPadding, 10, 5);
        }

        InteractiveSchedulePage interactiveSchedule;
        public MyEventEntries MyEvents { get; private set; }
        SchedListView lvForEvents;
        public void refresh()
        {
            if (MyEvents.Events == null)
                return;
            foreach (EventEntry ev in MyEvents.Events)
                ev.inMySched = true;
            lvForEvents.events = MyEvents.Events;
            if(MyEvents.Events.Count < 1)
                this.Content = new Label
                {
                    Text = "Nothing Here Yet\nTry adding an event from the Main Schedule",
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            else
            {
                this.Content = lvForEvents;
                lvForEvents.refresh();
            }
        }

        protected override void OnAppearing()
        {
            if(lvForEvents == null)
            {
                this.Content = new Label
                {
                    Text = "Loading...",
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            }
            else{
                lvForEvents.events = MyEvents.Events;
            }
            if (lvForEvents != null && MyEvents.Events.Count < 1)
                this.Content = new Label
                {
                    Text = "Nothing Here Yet\nTry adding an event from the Main Schedule",
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            else
            {
                this.Content = lvForEvents;
                lvForEvents.refresh();
            }
            
        }

        public MySchedulePage(MyEventEntries _MyEvents, InteractiveSchedulePage _interactiveSchedule = null)
        {
            InitializeComponent();

            interactiveSchedule = _interactiveSchedule;

            MyEvents = _MyEvents;

            this.Padding = GetPagePadding();

                lvForEvents = new SchedListView(MyEvents.Events);
                lvForEvents.ItemSelected += PopUpWithData;

                refresh();
           
        }
        async void PopUpWithData(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            EventEntry ee = e.SelectedItem as EventEntry;

            string addOrRemove = String.Empty;
            if (!MyEvents.Events.Contains(ee))
            {
                addOrRemove = "Add to My Schedule";
            }
            else
            {
                addOrRemove = "Remove from My Schedule";
            }

            string body = string.Empty;
            if (ee.StartTime.ToString() != string.Empty)
                body += "Time: " + ee.StartTime.ToString() + "\n\n";
            if (ee.category.Trim() != string.Empty)
                body += "Category: " + ee.category + "\n\n";
            if (ee.speaker1.Trim() != string.Empty && ee.speaker2.Trim() != string.Empty)
                body += "Speakers: " + ee.speaker1 + ", " + ee.speaker2 + "\n\n";
            else if (ee.speaker1.Trim() != string.Empty)
                body += "Speaker: " + ee.speaker1 + "\n\n";
            if (ee.contactInfo.Trim() != string.Empty)
                body += "Contact: " + ee.contactInfo + "\n\n";
            if (ee.Location.Trim() != string.Empty)
                body += "Location: " + ee.Location + "\n\n";
            if (ee.Description.Trim() != string.Empty)
                body += "Description: " + ee.Description;
            var userResult = await DisplayAlert(ee.Title, body, addOrRemove, "Back");
            if (userResult)
            {
                    MyEvents.removeEvent(ee.EventID);
                    await DisplayAlert(ee.Title, "Event has been removed from your schedule.", "Ok");
                ee.inMySched = false;
                if (interactiveSchedule != null)
                {
                    interactiveSchedule.refresh();
                    interactiveSchedule.MainEvents.syncWithMySched(this.MyEvents.Events);
                }
                    
                this.refresh();
            }
    ((ListView)sender).SelectedItem = null;
        }

    }
}