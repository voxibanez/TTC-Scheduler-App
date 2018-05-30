using FFImageLoading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Daddoon;
using System;

namespace mainApp
{
    
    public partial class App : Application
    {
        EventEntriesMain MainEvents;
        MyEventEntries MyEvents;
        InteractiveSchedulePage InteractivePage;
        MySchedulePage MyPage;
        internal static readonly double ScreenWidth;
        internal static readonly double ScreenHeight;

        public App()
        {
            InitializeComponent();
            MainEvents = new EventEntriesMain();
            MyEvents = new MyEventEntries();



            InteractivePage = new InteractiveSchedulePage(MainEvents, MyEvents, MyPage);
            MyPage = new MySchedulePage(MyEvents, InteractivePage);
            InteractivePage.mySchudule = MyPage;

            Task.Factory.StartNew(() => {
                return MainEvents.refreshData(saveLoad.loadDatabaseFromFile());
            })
                .ContinueWith(task =>
                {
                    InteractivePage.refresh();
                refreshInteractivePage(task.Result);
                }, TaskScheduler.FromCurrentSynchronizationContext());

            Task.Factory.StartNew(() => { updateMyEvents(); }).ContinueWith(task =>
            {
                MyPage.refresh();
            }, TaskScheduler.FromCurrentSynchronizationContext());


            MyPage.Title = "Personal";
            InteractivePage.Title = "Main";
            var nav = new NavigationPage();

            var tab = new TabbedPage
            {
                Padding = 0,
                Children =
                {
                InteractivePage,
                MyPage,
                new MapPage(),
                new InfoPage()
                }
                
            };

            var passwordPage = new Password(tab);
            passwordPage.Start(tab);
            NavigationPage.SetHasNavigationBar(tab, false);
            NavigationPage.SetHasBackButton(tab, false);

            //Preload about image
            ImageService.Instance.LoadUrl(AppResources.aboutPicture).Preload();
            //Preload the default image
            ImageService.Instance.LoadUrl(AppResources.defaultPicture).Preload();
        }

        private void refreshInteractivePage(bool foundFile)
        {
            Task.Factory.StartNew(() => {
                updateMainEvents(foundFile);
            })
               .ContinueWith(task =>
               {
                   InteractivePage.refresh();
               }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void updateMainEvents(bool gotFile = true)
        {
            MainEvents.refreshData(gotFile);
        }

        private void updateMyEvents()
        {
            MyEvents.loadJson(saveLoad.loadMyDatabase());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
