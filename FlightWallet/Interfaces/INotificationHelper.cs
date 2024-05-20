using System;
using UIKit;
using Foundation;
using UserNotifications;
using System.Runtime.CompilerServices;
using FlightWallet.Interfaces;
using System.Threading.Tasks;

namespace FlightWallet.Interfaces
{
    public interface INotificationHelper
    {
        Task ShowNotification(string title, string message);
    }

    public class NotificationHelper : INotificationHelper
    {
        const string channelId = "flightwallet";
        const string channelName = "FlightWallet";
        const string channelDescription = "The default channel for notifications.";

        public async Task CheckAndRequestNotificationPermission()
        {
            var notificationCenter = UNUserNotificationCenter.Current;
            var authorizationOptions = UNAuthorizationOptions.Alert;
            var settings = await notificationCenter.GetNotificationSettingsAsync();

            if (settings.AuthorizationStatus != UNAuthorizationStatus.Authorized)
            {
                await notificationCenter.RequestAuthorizationAsync(authorizationOptions);
            }
        }

        public async Task ShowNotification(string title, string message)
        {
            // Check and request notification permission if needed
            await CheckAndRequestNotificationPermission();

            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Body = message,
                Sound = UNNotificationSound.Default
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.1, false); // Trigger immediately

            var request = UNNotificationRequest.FromIdentifier(Guid.NewGuid().ToString(), content, trigger);
            await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
        }
    }
}