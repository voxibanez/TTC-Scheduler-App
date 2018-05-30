using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace mainApp
{
    public partial class PinchToZoomContainer : ContentView
    {
        double currentScale = 1;
        double startScale = 1;
        double xOffset = 0;
        double yOffset = 0;
        private double startX;
        private double startY;
        double x = 0, y = 0;

        public PinchToZoomContainer()
        {
            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(pinchGesture);
            /*   var panGesture = new PanGestureRecognizer();	
               panGesture.PanUpdated += OnPanUpdated;	
               GestureRecognizers.Add(panGesture);	
           */
        }

        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    // Store the current scale factor applied to the wrapped user interface element,	
                    // and zero the components for the center point of the translate transform.	
                    startScale = Content.Scale;
                    Content.AnchorX = 0;
                    Content.AnchorY = 0;

                    break;

                case GestureStatus.Running:
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

                    break;

                case GestureStatus.Completed:
                    // Store the translation delta's of the wrapped user interface element.	
                    xOffset = Content.TranslationX;
                    yOffset = Content.TranslationY;

                    break;
            }
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
                    var maxTranslationX = Content.Scale * Content.Width - Content.Width;
                    Content.TranslationX = Math.Min(0, Math.Max(-maxTranslationX, xOffset + e.TotalX - startX));

                    var maxTranslationY = Content.Scale * Content.Height - Content.Height;
                    Content.TranslationY = Math.Min(0, Math.Max(-maxTranslationY, yOffset + e.TotalY - startY));

                    break;

                case GestureStatus.Completed:
                    xOffset = Content.TranslationX;
                    yOffset = Content.TranslationY;

                    break;
            }
        }
    }
}
