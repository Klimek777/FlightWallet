using FlightWallet.ViewModels;

namespace FlightWallet.Views;

public partial class AddPage : ContentPage
{
    private readonly FlightsViewModel _viewModel;
    public AddPage(FlightsViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel; // Utw�rz instancj� widoku modelu
        BindingContext = _viewModel;
    }
}