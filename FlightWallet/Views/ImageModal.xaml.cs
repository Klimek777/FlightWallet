using Microsoft.Maui.Controls;

namespace FlightWallet.Views
{
    public partial class ImageModal : ContentPage
    {
        public ImageModal(ImageSource imageSource)
        {
            InitializeComponent();
            BindingContext = new ImageModalViewModel(imageSource);
        }

        private double startScale, currentScale;
        private double xOffset, yOffset;

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                startScale = Content.Scale;
                Content.Scale = 1;
                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }

            if (e.Status == GestureStatus.Running)
            {
                currentScale += (e.Scale - 1) * startScale;
                currentScale = Math.Max(1, currentScale);
                Content.Scale = currentScale;

                double deltaX = xOffset + (e.ScaleOrigin.X - 0.5) * zoomableImage.Width * (1 - e.Scale);
                double deltaY = yOffset + (e.ScaleOrigin.Y - 0.5) * zoomableImage.Height * (1 - e.Scale);
                Content.TranslationX = Math.Clamp(deltaX, -zoomableImage.Width * (currentScale - 1), 0);
                Content.TranslationY = Math.Clamp(deltaY, -zoomableImage.Height * (currentScale - 1), 0);
            }
        }

        void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Content.Scale != 1 && e.StatusType == GestureStatus.Running)
            {
                // Calculate the new translation values
                double newTranslationX = xOffset + e.TotalX;
                double newTranslationY = yOffset + e.TotalY;

                // Clamp the values to ensure the image doesn't move out of bounds
                double maxTranslationX = (zoomableImage.Width * Content.Scale - absoluteLayout.Width) / 2;
                double minTranslationX = -maxTranslationX;
                double maxTranslationY = (zoomableImage.Height * Content.Scale - absoluteLayout.Height) / 2;
                double minTranslationY = -maxTranslationY;

                newTranslationX = Math.Max(minTranslationX, Math.Min(maxTranslationX, newTranslationX));
                newTranslationY = Math.Max(minTranslationY, Math.Min(maxTranslationY, newTranslationY));

                // Apply the translation
                Content.TranslationX = newTranslationX;
                Content.TranslationY = newTranslationY;
            }

            if (e.StatusType == GestureStatus.Completed)
            {
                // Store the translation delta's applied during the pan
                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }
        }

        async void OnDoubleTapped(object sender, EventArgs e)
        {
            if (Content.Scale > 1)
            {
                // Reset the scale and translation to original state with animation
                await Content.ScaleTo(1, 400, Easing.CubicInOut);
                await Content.TranslateTo(0, 0, 400, Easing.CubicInOut);

                currentScale = 1;
                xOffset = 0;
                yOffset = 0;
            }
            else
            {
                // Zoom in to a predefined scale factor, e.g., 2x with animation
                currentScale = 2;
                await Content.ScaleTo(currentScale, 400, Easing.CubicInOut);
                // Center the image with animation
                await Content.TranslateTo(0, 0, 400, Easing.CubicInOut);
            }
        }

        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }

    public class ImageModalViewModel
    {
        public ImageSource ImageSource { get; set; }

        public ImageModalViewModel(ImageSource imageSource)
        {
            ImageSource = imageSource;
        }
    }
}
