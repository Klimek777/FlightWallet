using FlightWallet.ViewModels;
using FlightWallet.Views;
using FlightWallet.Models;

namespace FlightWallet
{
    public partial class MainPage : ContentPage
    {
        private readonly FlightsViewModel _viewModel;

        public MainPage( FlightsViewModel viewModel)
        {
            InitializeComponent();
            Application.Current.UserAppTheme=AppTheme.Light;
            _viewModel = viewModel;
            BindingContext = viewModel;
        }
        
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadFlightsAsync();
        }
        private async void NavigateToAddPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddPage(_viewModel));
        }

        private async void NavigateToEditPage(object sender, EventArgs e)
        {
            var selectedFlight = (sender as Button)?.BindingContext as Flight;
            if (selectedFlight != null)
            {
                await Navigation.PushAsync(new EditPage(selectedFlight));
            }
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            var image = (Image)sender;

            await Navigation.PushModalAsync(new ImageModal(image.Source));
        }
    }

}
