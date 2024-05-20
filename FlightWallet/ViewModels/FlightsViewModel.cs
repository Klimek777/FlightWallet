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
using EventKit;
using Foundation;
using FlightWallet.Interfaces;

namespace FlightWallet.ViewModels
{
    public partial class FlightsViewModel : ObservableObject
    {
        private readonly DatebaseContext _context;
        private INavigation _navigation;
        public FlightsViewModel(DatebaseContext context)
        {
            _context = context;
        }
        public FlightsViewModel(DatebaseContext context, Flight flight)
        {
            _context = context;
            _operatingFlight = flight;
        }
        public void SetNavigation(INavigation navigation)
        {
            _navigation = navigation;
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
                            Flights = new ObservableCollection<Flight>();
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
                var busytext = OperatingFlight.Id == 0 ? "Creating flight..." : "Updating flight";
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
                        await LoadFlightsAsync();

                        var flightToUpdate = Flights.FirstOrDefault(f => f.Id == OperatingFlight.Id);

                        if (flightToUpdate != null)
                        {
                            // Zastąp ten element OperatingFlight
                            Flights[Flights.IndexOf(flightToUpdate)] = OperatingFlight;
                        }

                        if (_context != null)
                        {
                            await _context.UpdateItemAsync<Flight>(OperatingFlight);
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Error", "Context is null", "OK");
                        }
                    }

                    SetOperatingFlightCommand.Execute(new());

                }, busytext);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("error", ex.ToString(), "OK");
            }
            finally
            {
                await _navigation.PushAsync(new MainPage(this));
            }
        }
        [RelayCommand]
        private async Task DeleteFlightAsync(int id)
        {
            var result = await Shell.Current.DisplayActionSheet("Are you sure you want to delete flight?", "No", null, "Yes");

            if (result == "Yes")
            {
                await ExecuteAsync(async () => {
                    if (await _context.DeleteItemByKeyAsync<Flight>(id))
                    {
                        var flight = Flights.FirstOrDefault(f => f.Id == id);
                        Flights.Remove(flight);

                        Vibration.Vibrate(10000);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error while deleting", "The product was not deleted", "OK");
                    }
                }, "Delete product");
            }
            else
            {
                //DependencyService.Register<INotificationHelper, NotificationHelper>();
                //var notificationHelper = DependencyService.Get<INotificationHelper>();
                //await notificationHelper?.ShowNotification("Oops", "Your flight was not deleted!");
            }
        }
        [RelayCommand]
        private async Task AddEventToCalendarAsync(int id)
        {
            var result = await Shell.Current.DisplayActionSheet("Are you sure you want to add flight to calendar?", "Yes", null, "No");

            if (result == "Yes")
            {
                var flight = Flights.FirstOrDefault(f => f.Id == id);

                // Sprawdź, czy lot istnieje
                if (flight != null)
                {
                    // Spróbuj dodać wybrany lot do kalendarza
                    bool success = await AddEventToCalendarAsync(flight);
                    if (success)
                    {
                        await Shell.Current.DisplayAlert("Success", "Flight added to calendar successfully", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Failed to add the flight to the calendar.", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Couldn't find the selected flight.", "OK");
                }
            }
        }

        private async Task<bool> AddEventToCalendarAsync(Flight flight)
        {
           
            try
            {
                EKEventStore eventStore = new EKEventStore();
                // Utwórz nowy obiekt EKEventStore
                var status = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();
                if (status != PermissionStatus.Granted)
                {
                    // Jeśli nie ma dostępu, poproś użytkownika o niego
                    status = await Permissions.RequestAsync<Permissions.CalendarRead>();
                }
                if (status == PermissionStatus.Granted)
                {
                    await eventStore.RequestAccessAsync(EKEntityType.Event);
                }

                // Sprawdź, czy użytkownik udzielił uprawnień
                if (EKEventStore.GetAuthorizationStatus(EKEntityType.Event) == EKAuthorizationStatus.Authorized)
                {
                    // Utwórz nowy obiekt EKEvent i ustaw jego właściwości na podstawie danych lotu
                    NSDateFormatter formatter = new NSDateFormatter();
                    formatter.DateFormat = "yyyy-MM-dd HH:mm:ss"; // Ustaw format daty

                    DateTime startDateTime = flight.DepartureDate + flight.DepartureTime;
                    DateTime endDateTime = startDateTime.AddHours(3);

                    string dateTimeStartString = startDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string dateTimeEndString = endDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                    // Utwórz obiekt NSDate na podstawie łańcucha tekstowego
                    var dateStart = formatter.Parse(dateTimeStartString);
                    var dateEnd = formatter.Parse(dateTimeEndString);

                    EKEvent newEvent = EKEvent.FromStore(eventStore);
                    newEvent.Title = $"Flight {flight.FlightFromName} - {flight.FlightToName}"; // Tutaj możesz użyć dowolnej nazwy dla zdarzenia
                    newEvent.Notes = $"Flight from {flight.FlightFromName}, {flight.AirportFromName}\nto {flight.FlightToName}, {flight.AirportToName}\nPrice: {flight.Price}"; // Tutaj możesz dodać dowolne notatki
                    newEvent.StartDate = dateStart;
                    newEvent.EndDate = dateEnd;
                    newEvent.Calendar = eventStore.DefaultCalendarForNewEvents;

                    // Zapisz zdarzenie do kalendarza
                    eventStore.SaveEvent(newEvent, EKSpan.ThisEvent, out NSError error);

                    if (error != null)
                    {
                        // Obsłuż błąd, jeśli wystąpił
                        return false;
                    }
                    else
                    {
                        // Zdarzenie zostało pomyślnie dodane do kalendarza
                        return true;
                    }
                }
                else
                {
                    // Jeśli użytkownik nie udzielił uprawnień, zwróć false
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Obsłuż wyjątek
                await Shell.Current.DisplayAlert(" error", ex.ToString(), "OK");
                return false;
            }
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
