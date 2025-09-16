namespace Prayerify.Core.Services
{
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message, string cancel = "OK");
    }
    
    public class DialogService : IDialogService
    {
        public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            // This is a simplified version for testing
            // In the actual MAUI app, this would use Shell.Current?.CurrentPage
            await Task.CompletedTask;
        }
    }
}
