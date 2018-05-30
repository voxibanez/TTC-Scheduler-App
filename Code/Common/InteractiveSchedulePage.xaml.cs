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
    public partial class InteractiveSchedulePage : ContentPage
    {
        public MySchedulePage mySchudule;
        SchedListView lvForEvents;
        Label LoadingText;
        private int prevCategory = 0;

        private static Thickness GetPagePadding()
        {
            double topPadding;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    topPadding = 20;
                    break;
                default:
                    topPadding = 1;
                    break;
            }

            return new Thickness(0, topPadding, 0, 0);
        }
       public EventEntriesMain MainEvents { get; private set; }
        MyEventEntries MyEvents;

        public void refresh()
        {
            if (MainEvents.Events == null || MainEvents.Events.Count < 1)
            {
                this.Content = new StackLayout
                {
                    Children =
                    {
                        new Label { Text = "Error, database could not be loaded",
                            FontSize = 30,
                            HorizontalTextAlignment = TextAlignment.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center },
                        new Button
                        {
                            Text = "Try Again",
                            Command = new Command(() => {
                                this.Content = null;
                                this.Content = LoadingText;
                                 Task.Factory.StartNew(() => {MainEvents.refreshData(false); })
                                .ContinueWith(task =>
            {
                this.refresh();
            }, TaskScheduler.FromCurrentSynchronizationContext());
                            })
                        }


            }
                };
            }
            else
            {
                Label NameCategory = new Label
                {
                    Text = " Category:",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 17,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center

                };
                Picker categories = new Picker
                {
                    Title = "Filter by category",
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center

                };
                categories.Items.Add("All");

                foreach  (EventEntry cat in MainEvents.Events)
                {
                    if (!categories.Items.Contains(cat.category)&& cat.category!= " ")
                        categories.Items.Add(cat.category);
                }



                categories.SelectedIndexChanged += (sender, args) =>
                {
                    prevCategory = categories.SelectedIndex;
                    if (categories.SelectedIndex == 0)
                        lvForEvents.events = MainEvents.Events;
                    else
                    {
                        lvForEvents.events = MainEvents.Events.Where(x => x.category == categories.Items[categories.SelectedIndex]).ToList();
                    }
                    lvForEvents.refresh();
                };
                var tempGrid = new Grid();
                tempGrid.RowSpacing = 0;
                
                tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Auto) });
                tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Auto) });
                tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20, GridUnitType.Auto) });
                tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50, GridUnitType.Star) });
                tempGrid.Children.Add(NameCategory, 0, 0);
                var aaaa = new StackLayout { Children = { categories } };
                tempGrid.Children.Add(aaaa, 1, 0);
                tempGrid.Children.Add(lvForEvents, 0, 2, 1, 3);


                this.Content = tempGrid;

                try
                {
                    categories.SelectedIndex = prevCategory;
                    if (categories.SelectedIndex == 0)
                        lvForEvents.events = MainEvents.Events;
                    else
                    {
                        lvForEvents.events = MainEvents.Events.Where(x => x.category == categories.Items[categories.SelectedIndex]).ToList();
                    }
                    lvForEvents.refresh();
                }
                catch
                {
                    categories.SelectedIndex = 0;
                }


            }                          
        }

        protected override void OnAppearing()
        {

                Task.Factory.StartNew(() => { return MainEvents.refreshData(); })
                               .ContinueWith(task =>
                               {
                                   if (task.Result)
                                   {
                                       lvForEvents.refresh();
                                       this.refresh();
                                   }

                               }, TaskScheduler.FromCurrentSynchronizationContext());
            
        }

    

        public InteractiveSchedulePage(EventEntriesMain _MainEvents, MyEventEntries _MyEvents, MySchedulePage _mySchedule = null)
        {
            InitializeComponent();
            mySchudule = _mySchedule;
            this.Padding = GetPagePadding();
            MyEvents = _MyEvents;
            MainEvents = _MainEvents;

   
                LoadingText = new Label();
                LoadingText.FontSize = 20;
                LoadingText.HorizontalOptions = LayoutOptions.Center;
                LoadingText.VerticalOptions = LayoutOptions.Center;
                LoadingText.HorizontalTextAlignment = TextAlignment.Center;
                LoadingText.FontAttributes = FontAttributes.Italic;
                LoadingText.Text = "Loading Database...";
                this.Content = LoadingText;

            lvForEvents = new SchedListView(MainEvents.Events);
            lvForEvents.ItemSelected += PopUpWithData;
        }
        async void PopUpWithData(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            EventEntry ee = e.SelectedItem as EventEntry;
            bool inMy = false;
            foreach (EventEntry _event in MyEvents.Events)
            {
                if (_event.EventID == ee.EventID)
                    inMy = true;

            }
            string addOrRemove = String.Empty;
            if (!inMy)
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
                if (inMy)
                {
                    MyEvents.removeEvent(ee.EventID);
                    if (mySchudule != null)
                    {
                        mySchudule.refresh();
                        MainEvents.syncWithMySched(mySchudule.MyEvents.Events);
                    }
                    this.refresh();
                    await DisplayAlert(ee.Title, "Event has been removed from your schedule.", "Ok");
                    ee.inMySched = false;

                }
                else
                {
                    MyEvents.addEvent(ee);
                    if (mySchudule != null)
                    {
                        mySchudule.refresh();
                        MainEvents.syncWithMySched(mySchudule.MyEvents.Events);
                    }
                    this.refresh();
                    await DisplayAlert(ee.Title, "Event has been added to your schedule.\nIt starts at " + ee.StartTime.ToString(), "Ok");
                    ee.inMySched = true;
                }

                   

            }
                ((ListView)sender).SelectedItem = null;
        }
    }
}