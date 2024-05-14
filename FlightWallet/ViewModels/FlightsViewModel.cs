using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightWallet.Data;
using FlightWallet.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightWallet.ViewModels
{
    public partial class FlightsViewModel : ObservableObject
       
    {
        private readonly DatebaseContext _context;
        public FlightsViewModel(DatebaseContext context)
        {
            _context = context;   
        }
        [ObservableProperty]
        private ObservableCollection<Flight> _flights;
        [ObservableProperty]
        private Flight _operatingFlight = new();
        [ObservableProperty]
        private bool _isBusy;
        [ObservableProperty]
        private string _busyText;

        public async Task LoadFlightsAsync()
        {
            try
            {
                await ExecuteAsync(async () =>
                {
                    var flights = await _context.GetAllAsync<Flight>();
                    if (flights is not null && flights.Any())
                    {
                        if (Flights is null)
                        {
                            Flights ??= new ObservableCollection<Flight>();
                            foreach (var flight in flights)
                            {
                                Flights.Add(flight);
                            }
                        }
                    }
                }, "Fetching flights");
            }catch(Exception ex)
            {
                await Shell.Current.DisplayAlert(" error", ex.ToString(), "OK");

            }


        }
        [RelayCommand]
        private void SetOperatingFlight(Flight? flight)=>OperatingFlight = flight ?? new();
        
        [RelayCommand]
        private async Task SaveFlightAsync()
        {
            try
            {
                if (OperatingFlight is null)
                    return;
                var busytext = OperatingFlight.Id == 0 ? "Creating flight..." : "Updateing flight";
                await ExecuteAsync(async () =>
                {
                    if (OperatingFlight.Id == 0)
                    {
                        //create Flight
                        await _context.AddItemAsync<Flight>(OperatingFlight);
                        try
                        {
                            if (Flights is null)
                            {
                                // Jeśli OperatingFlight jest null, utwórz nowy obiekt Flight
                                Flights = new();
                            }
                            Flights.Add(OperatingFlight);

                        }
                        catch (Exception ex)
                        {
                            await Shell.Current.DisplayAlert(" error", ex.ToString(), "OK");

                        }

                    }
                    else
                    {
                        // Update Flight
                        await _context.UpdateItemAsync<Flight>(OperatingFlight);
                        var flightCopy = OperatingFlight;

                        var index = Flights.IndexOf(OperatingFlight);
                        Flights.RemoveAt(index);

                        Flights.Insert(index, flightCopy);

                    }


                    SetOperatingFlightCommand.Execute(new());


                }, busytext);
            }catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("error", ex.ToString(), "OK");
            }

        }
        [RelayCommand]
        private async Task DeleteFlightAsync(int id)
        {
            await ExecuteAsync(async () => {
                if (await _context.DeleteItemByKeyAsync<Flight>(id))
                {
                    var flight = Flights.FirstOrDefault(f => f.Id == id);
                    Flights.Remove(flight);
                    //moze tu dodac wibracje ? albo modal z potwierdzeniem który wywołuje wibracje?
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete error", "Product was not deleted", "OK");
                }
            }, "Deleting product");
           
        }
       

        private async Task ExecuteAsync(Func<Task> operation, string? busyText = null)
        {
            IsBusy = true;
            BusyText = busyText ?? "Processing"; 
            try
            {
                await operation?.Invoke();
            }
            finally
            {
                IsBusy = false;
                BusyText = "Processing...";
            }
        }
    }
}
