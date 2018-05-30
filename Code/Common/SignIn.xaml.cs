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
    public partial class SignIn : ContentPage
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

        public SignIn()
        {
            InitializeComponent();




            Entry usernameEntry = new Entry { Placeholder = "Username" };
            Entry passwordEntry = new Entry
            {
                Placeholder = "Password",
                IsPassword = true
            };

            Button signInButton = new Button
            {
                Text = "Sign In",
                Font = Font.SystemFontOfSize(NamedSize.Large),

                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.Start,

            };

            Button signUpButton = new Button
            {
                Text = "Sign Up",
                Font = Font.SystemFontOfSize(NamedSize.Large),

                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.Start,

            };

            this.Content = new StackLayout
            {
                Children =
                {
                    usernameEntry,
                    passwordEntry,
                    signInButton,
                    signUpButton
                }
            };

        }
    }
}