using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Platform = Xamarin.Forms.PlatformConfiguration;

namespace mainApp
{

    //Derived from ListView, it is a custom class used to display an EventEntry list
    class SchedListView : ListView
    {
        public List<EventEntry> events;
        private EventEntriesMain mainEvents;

        //Called when the EventEntry list is refreshed
        //This triggers the listview to refresh
        public void refresh()
        {
            if (events == null || events.Count < 1)
                return;
            string lastID = string.Empty;
            if (this.SelectedItem != null)
                lastID = ((EventEntry)this.SelectedItem).EventID;
            ItemsSource = null;
            ItemsSource = events;
            if (lastID != string.Empty) {
                foreach (EventEntry ev in events)
                {
                    if (ev.EventID == lastID)
                    {
                        this.ScrollTo(ev, ScrollToPosition.Center, false);
                        break;
                    }
                }
            }

            foreach (EventEntry _event in events)
            {
                if (DateTime.Compare(_event.EndTime, DateTime.Now) < 0)
                {
                    _event.ColorStr = "White";
                    _event.ThumbnailStr = "";
                    _event.past = true;
                }
                else
                    break;
                    
            }
            //scrollToCurrent();
        }

        //Constructor that sets up the item template for the EventEntry list
        public SchedListView(List<EventEntry> _events, MyEventEntries myEvents = null, EventEntriesMain _mainEvents = null) : base(ListViewCachingStrategy.RecycleElement)
        {
            this.IsPullToRefreshEnabled = false;
            mainEvents = _mainEvents;
            events = _events;

            Task.Factory.StartNew(() => {
                refresh();
                ItemTemplate = new DataTemplate(() =>
                {
                    RowHeight = 95;
                    Label Title = new Label();
                    Title.FontAttributes = FontAttributes.Bold;
                    Title.SetBinding(Label.TextProperty, "Title");
                    Title.MinimumHeightRequest = 50;
                    Title.FontSize = Xamarin.Forms.AbsoluteLayout.AutoSize;

                    Label date = new Label();
                    date.FontAttributes = FontAttributes.Italic;
                    date.SetBinding(Label.TextProperty, "StartTime", BindingMode.Default, null, ("{0:MMMM dd}"));
                    date.MinimumHeightRequest = 50;
                    date.FontSize = Xamarin.Forms.AbsoluteLayout.AutoSize;

                    Label speaker = new Label();
                    speaker.FontAttributes = FontAttributes.Italic;
                    speaker.SetBinding(Label.TextProperty, "speaker1");
                    speaker.MinimumHeightRequest = 50;
                    speaker.FontSize = Xamarin.Forms.AbsoluteLayout.AutoSize;

                    Label startEndTime = new Label();
                    startEndTime.FontAttributes = FontAttributes.Italic;
                    startEndTime.SetBinding(Label.TextProperty, "StartEndTime");
                    startEndTime.MinimumHeightRequest = 50;
                    startEndTime.FontSize = Xamarin.Forms.AbsoluteLayout.AutoSize;

                    Label location = new Label();
                    location.SetBinding(Label.TextProperty, "Location");
                    location.MinimumHeightRequest = 50;
                    location.FontSize = Xamarin.Forms.AbsoluteLayout.AutoSize;

                    FFImageLoading.Forms.CachedImage im = new FFImageLoading.Forms.CachedImage()
                    {
                        RetryCount = 3, //Number of trys until it gives up
                        RetryDelay = 200, //Delay between tries (in ms)
                        HeightRequest = 50,
                        WidthRequest = 50,
                        // DownsampleWidth = 100, //Down sample to predetermined width (maintains aspect ratio)
                        Aspect = Xamarin.Forms.Aspect.AspectFit, //Sets aspect ratio 
                        DownsampleToViewSize = true,
                        CacheType = FFImageLoading.Cache.CacheType.All,
                        IsOpaque = true,
                        LoadingPriority = FFImageLoading.Work.LoadingPriority.High,
                        LoadingPlaceholder = AppResources.defaultPicture
                    };

                    var transform = new FFImageLoading.Transformations.CircleTransformation(10, "#000000");
                    im.Transformations.Add(transform);
                    im.SetBinding(CachedImage.SourceProperty, "ThumbnailStr");
                    im.SetBinding(CachedImage.BackgroundColorProperty, "Color");

                    FFImageLoading.Forms.CachedImage calenderImage = new FFImageLoading.Forms.CachedImage()
                    {
                        IsVisible = false,
                        HeightRequest = 50,
                        WidthRequest = 50,
                        // DownsampleWidth = 100, //Down sample to predetermined width (maintains aspect ratio)
                        Aspect = Xamarin.Forms.Aspect.AspectFit, //Sets aspect ratio 
                        DownsampleToViewSize = true,
                        CacheType = FFImageLoading.Cache.CacheType.Memory,
                        IsOpaque = true,
                        LoadingPriority = FFImageLoading.Work.LoadingPriority.High,
                        Source = "checklist.png"
                    };
                    calenderImage.SetBinding(CachedImage.IsVisibleProperty, "inMySched");



                    Button addRemove;
                    addRemove = new Button
                    {
                        Text = "Add/Remove",
                    };

                    BoxView boxView = new BoxView
                    {
                        Color = Color.Gray,
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.Fill
                    };


                    var tempGrid = new Grid();
                    tempGrid.RowSpacing = 2;
                    tempGrid.ColumnSpacing = 5;

                    tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Star) });
                    tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Auto) });
                    tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25, GridUnitType.Star) });
                    tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25, GridUnitType.Star) });
                    tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Star) });

                    tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30, GridUnitType.Star) });
                    tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50, GridUnitType.Star) });
                    tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                    tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50, GridUnitType.Star) });
                    tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                    //  tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50, GridUnitType.Star) });
                    tempGrid.Children.Add(Title, 1, 4, 1, 3);
                    tempGrid.Children.Add(date, 1, 3);
                    tempGrid.Children.Add(startEndTime, 3, 3);
                    tempGrid.Children.Add(location, 1, 3,4,5);
                    tempGrid.Children.Add(speaker, 3, 4);
                    tempGrid.Children.Add(im, 0, 1, 1, 5);
                    tempGrid.Children.Add(calenderImage,4, 5, 1, 5);
                    //tempGrid.Children.Add(addRemove, 2, 3, 1, 4);
                    tempGrid.Children.Add(boxView, 0, 5, 5, 6);
                    tempGrid.SetBinding(Grid.BackgroundColorProperty, "Color");

                    return new ViewCell
                    {
                        View = tempGrid
                    };

                });
            }).ContinueWith(task =>
            {
               // scrollToCurrent();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            

            
            }

        //Scroll to the latest ongoing event
        public void scrollToCurrent()
        {
            foreach (EventEntry _event in events)
            {
                //Compare time of event to time 1 hour before current (with 0 minutes)
                // if ((DateTime.Compare(_event.Time, DateTime.Now.AddHours(-1).AddMinutes(-DateTime.Now.Minute)) >= 0))
                if ((DateTime.Compare(_event.EndTime, DateTime.Now) >= 0))
                {
                    this.ScrollTo(_event, ScrollToPosition.Center, true);
                    break;
                }
            }
        }
        }


        }
    
