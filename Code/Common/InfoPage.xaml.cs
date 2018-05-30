using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XLabs.Forms.Controls;

namespace mainApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class InfoPage : ContentPage
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
        private static ISettings AppSettings =>
    CrossSettings.Current;

        public InfoPage ()
		{
            InitializeComponent();
            this.Padding = GetPagePadding();
            CheckBox pushRemindersCheckBox = new CheckBox
            {
                DefaultText = "Enable MySchedule reminders",
                TextColor = Xamarin.Forms.Color.Black,
                Checked = true
            };
            pushRemindersCheckBox.Checked = AppSettings.GetValueOrDefault("pushRemindersCheckBox", true);
            if (pushRemindersCheckBox.Checked)
            {
                Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().startNotifications(false);
            }
            else
            {
                Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().stopNotifications();
            }

            CheckBox pushAdminCheckBox = new CheckBox
            {
                DefaultText = "Enable conference notifications",
                TextColor = Xamarin.Forms.Color.Black,
                Checked = true
            };
            pushAdminCheckBox.Checked = AppSettings.GetValueOrDefault("pushAdminCheckBox", true);


            Button aboutButton = new Button
            {
                Text = "About The App",
                Font = Font.SystemFontOfSize(NamedSize.Large),

                HorizontalOptions = LayoutOptions.Fill,

            };
            
            Button surveyButton = new Button
            {
                Text = "TTC Survey",
                Font = Font.SystemFontOfSize(NamedSize.Large),

                HorizontalOptions = LayoutOptions.Fill,

            };

            Button appFeedbackButton = new Button
            {
                Text = "App Feedback",
                Font = Font.SystemFontOfSize(NamedSize.Large),

                HorizontalOptions = LayoutOptions.Fill,
            };

            appFeedbackButton.Clicked += delegate
            {
               // Device.OpenUri(new Uri(AppResources.appSurveyLink));
            };

            surveyButton.Clicked += delegate
            {
               // Device.OpenUri(new Uri(AppResources.surveyLink));
            };


            pushRemindersCheckBox.CheckedChanged += delegate
            {
                AppSettings.AddOrUpdateValue("pushRemindersCheckBox", pushRemindersCheckBox.Checked);
                if (pushRemindersCheckBox.Checked)
                {
                    Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().startNotifications(false);
                }
                else
                {
                    Xamarin.Forms.DependencyService.Get<CrossPlatformUtility>().stopNotifications();
                }
            };
            pushAdminCheckBox.CheckedChanged += async delegate
            {

                if (!pushAdminCheckBox.Checked)
                {
                    var userResult = await DisplayAlert("Warning", "If you uncheck this, you will not receive any notifications from the event organizers\nThis includes schedule changes, location changes, and general information not found in the schedule\nAre you sure you want to turn this off?","No","Yes");
                    if (userResult)
                        pushAdminCheckBox.Checked = true;
                }
                AppSettings.AddOrUpdateValue("pushAdminCheckBox", pushAdminCheckBox.Checked);
            };

            aboutButton.Clicked += delegate
                {
                    Navigation.PushAsync(new About());
                };

         


            this.Content = new StackLayout
            {
                Children =
                {
                aboutButton,
                surveyButton,
                appFeedbackButton,
                pushRemindersCheckBox,
                pushAdminCheckBox
                }
                };

            
			
		}
	}
}