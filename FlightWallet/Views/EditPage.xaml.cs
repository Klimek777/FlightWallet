using FlightWallet.ViewModels;
using FlightWallet.Models;
using FlightWallet.Data;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace FlightWallet.Views;

public partial class EditPage : ContentPage
{
    private readonly DatebaseContext _context = new DatebaseContext();
    private readonly FlightsViewModel _viewModel;
    public EditPage(Flight flight)
	{
		InitializeComponent();
        _viewModel = new FlightsViewModel(_context, flight);
        _viewModel.SetNavigation(Navigation);
        BindingContext = _viewModel;
        NavigationPage.SetHasBackButton(this, true);
    }

    private async void SelectPhoto_Click(object sender, EventArgs e)
    {
        try
        {
            var action = await DisplayActionSheet("Add Photo", "Cancel", null, "Choose from Gallery", "Take a Photo");
            if (action == "Choose from Gallery")
            {
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo != null)
                {
                    _viewModel.OperatingFlight.ImagePath = photo.FullPath;
                    await Navigation.PushAsync(new EditPage(_viewModel.OperatingFlight));
                }
            }
            else if (action == "Take a Photo")
            {
                await CapturePhotoAsync();
            }
        }
        catch (Exception ex)
        {
            // Obsłużanie błędów w przypadku problemów z wyborem zdjęcia
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task CapturePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                // Save the file into local storage
                var newFile = Path.Combine(FileSystem.CacheDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                    await stream.CopyToAsync(newStream);

                _viewModel.OperatingFlight.ImagePath = newFile;
                await Navigation.PushAsync(new EditPage(_viewModel.OperatingFlight));
            }
        }
        catch (Exception ex)
        {
            // Obsłużanie błędów w przypadku problemów z robieniem zdjęcia
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void RotatePhoto_Click(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_viewModel.OperatingFlight.ImagePath))
            {
                var rotatedFilePath = await RotateImage90DegreesAsync(_viewModel.OperatingFlight.ImagePath);
                _viewModel.OperatingFlight.ImagePath = rotatedFilePath;
                await Navigation.PushAsync(new EditPage(_viewModel.OperatingFlight));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task<string> RotateImage90DegreesAsync(string filePath)
    {
        using (var originalImage = SKBitmap.Decode(filePath))
        {
            var rotatedImage = RotateImage(originalImage, 90);

            var rotatedFilePath = Path.Combine(FileSystem.CacheDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}_rotated.jpg");
            using (var image = SKImage.FromBitmap(rotatedImage))
            using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
            using (var stream = File.OpenWrite(rotatedFilePath))
            {
                await data.AsStream().CopyToAsync(stream);
            }

            return rotatedFilePath;
        }
    }

    private SKBitmap RotateImage(SKBitmap originalImage, int degrees)
    {
        var rotatedImage = new SKBitmap(originalImage.Height, originalImage.Width);
        using (var canvas = new SKCanvas(rotatedImage))
        {
            canvas.Translate(rotatedImage.Width / 2, rotatedImage.Height / 2);
            canvas.RotateDegrees(degrees);
            canvas.Translate(-originalImage.Width / 2, -originalImage.Height / 2);
            canvas.DrawBitmap(originalImage, 0, 0);
        }
        return rotatedImage;
    }

    private async void RemovePhoto_Click(object sender, EventArgs e)
    {
        try
        {
            _viewModel.OperatingFlight.ImagePath = null;
            await Navigation.PushAsync(new EditPage(_viewModel.OperatingFlight));
        }
        catch (Exception ex)
        {
            // Obsłużanie błędów w przypadku problemów z wyborem zdjęcia
            await Shell.Current.DisplayAlert(" error", ex.Message, "OK");
        }
    }
}