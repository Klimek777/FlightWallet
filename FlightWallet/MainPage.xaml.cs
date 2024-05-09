using FlightWallet.ViewModels;
using FlightWallet.Views;

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


    }

}
