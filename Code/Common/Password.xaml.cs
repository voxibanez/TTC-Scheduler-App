using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mainApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Password : ContentPage
    {

        private static Thickness GetPagePadding()
        {
            double topPadding;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    topPadding = 30;
                    break;
                default:
                    topPadding = 20;
                    break;
            }

            return new Thickness(10, topPadding, 10, 5);
        }
        private int count = 0;
        private string prevPass = string.Empty;
        private static ISettings AppSettings =>
CrossSettings.Current;

        public Password(TabbedPage nextPage)
        {
            int retryCount = 5;
            InitializeComponent();
            count = AppSettings.GetValueOrDefault("appPassTries", 0);

            Label infoLabel = new Label
            {
                FontSize = 20,
                Text = "This app is protected by a one time password\nIf you do not know the password, please contact TeradyneMobile@gmail.com"
            };

            Label incorrectLabel = new Label
            {
                FontSize = 15,
                Text = "",
                TextColor = Color.Red
            };

            Entry passwordEntry = new Entry
            {
                Placeholder = "Password",
                IsPassword = true
            };

            Button signInButton = new Button
            {
                Text = "Submit",
                Font = Font.SystemFontOfSize(NamedSize.Large),
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.Start,


            };
            signInButton.Clicked += delegate
            {
             
                if (passwordEntry.Text == "password")
                {
                    
                    AppSettings.AddOrUpdateValue("appFirstTime", false);
                    Start(nextPage);
                    return;
                }
                else if(passwordEntry.Text != null && passwordEntry.Text != string.Empty)
                {
                    if (prevPass != passwordEntry.Text && count < retryCount)
                        count++;
                    if(count >= retryCount)
                        incorrectLabel.Text = "No more attempts left.\nPlease contact TeradyneMobile@gmail.com for support";
                    else
                        incorrectLabel.Text = "Incorrect Password, please try again (" + (retryCount - count) + " more attempts)";
                }
                prevPass = passwordEntry.Text;
            };

            this.Content = new StackLayout
            {
                Padding = GetPagePadding(),
                Children = {
                    infoLabel,
                    incorrectLabel,
                    passwordEntry,
                    signInButton
                }
            };

        }

        internal void Start(TabbedPage nextPage)
        {
            bool firstTime = AppSettings.GetValueOrDefault("appFirstTime", true);
            if (!firstTime || AppResources.passwordDisabled)
            {
                Application.Current.MainPage = new NavigationPage(nextPage);
                return;
            }
            else
            {
                Application.Current.MainPage = this;
            }

        }
    }
}