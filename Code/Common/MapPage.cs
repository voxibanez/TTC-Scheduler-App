using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace hello
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();



            Content = new Grid
            {
                Padding = new Thickness(20),
                Children =
                        {
                            new PinchToZoomContainer
                            {

                                Content = new Image
                                {
                                    Source = ImageSource.FromFile ("final_picture_map.PNG")

                                }
                            }
                        }
            };
        }


        public class PinchToZoomContainer : ContentView
        {
            private double startScale;
            private double currentScale;
            private double xOffset;
            private double yOffset;
            public PinchToZoomContainer()
            {
                var pinchGesture = new PinchGestureRecognizer();
                pinchGesture.PinchUpdated += OnPinchUpdated;
                GestureRecognizers.Add(pinchGesture);
            }
            void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
            {
                if (e.Status == GestureStatus.Started)
                {
                    // Store the current scale factor applied to the wrapped user interface element,
                    // and zero the components for the center point of the translate transform.
                    //changed content to e 
                    startScale = e.Scale;
                    Content.AnchorX = 0;
                    Content.AnchorY = 0;
                }  // Handle the pinch
                if (e.Status == GestureStatus.Running)
                {
                    // Calculate the scale factor to be applied.
                    currentScale += (e.Scale - 1) * startScale;
                    currentScale = Math.Max(1, currentScale);

                    // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                    // so get the X pixel coordinate.
                    double renderedX = Content.X + xOffset;
                    double deltaX = renderedX / Width;
                    double deltaWidth = Width / (Content.Width * startScale);
                    double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                    // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                    // so get the Y pixel coordinate.
                    double renderedY = Content.Y + yOffset;
                    double deltaY = renderedY / Height;
                    double deltaHeight = Height / (Content.Height * startScale);
                    double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                    // Calculate the transformed element pixel coordinates.
                    double targetX = xOffset - (originX * Content.Width) * (currentScale - startScale);
                    double targetY = yOffset - (originY * Content.Height) * (currentScale - startScale);

                    // Apply translation based on the change in origin.
                    Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (currentScale - 1)));
                    Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (currentScale - 1)));

                    // Apply scale factor.
                    Content.Scale = currentScale;
                }
                if (e.Status == GestureStatus.Completed)
                {
                    // Store the translation delta's of the wrapped user interface element.
                    xOffset = Content.TranslationX;
                    yOffset = Content.TranslationY;
                }
            }
        }
        public class PanContainer : ContentView
        {
            private double startX;
            private double startY;
            double x, y;
            public PanContainer()
            {
                // Set PanGestureRecognizer.TouchPoints to control the
                // number of touch points needed to pan
                var panGesture = new PanGestureRecognizer();
                panGesture.PanUpdated += OnPanUpdated;
                GestureRecognizers.Add(panGesture);
            }
            void OnPanUpdated(object sender, PanUpdatedEventArgs e)
            {
                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                        startX = e.TotalX;
                        startY = e.TotalY;
                        Content.AnchorX = 0;
                        Content.AnchorY = 0;

                        break;
                    case GestureStatus.Running:
                        // Translate and ensure we don't pan beyond the wrapped user interface element bounds.
                        Content.TranslationX = Math.Max(Math.Min(0, x + e.TotalX), -Math.Abs(Content.Width - App.ScreenWidth));
                        Content.TranslationY = Math.Max(Math.Min(0, y + e.TotalY), -Math.Abs(Content.Height - App.ScreenHeight));
                        break;

                    case GestureStatus.Completed:
                        // Store the translation applied during the pan
                        x = Content.TranslationX;
                        y = Content.TranslationY;
                        break;
                }
            }
        }
    }
}
    


