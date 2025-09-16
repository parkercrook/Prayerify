namespace Prayerify.Services
{
    public interface IGenericService
    {
        Task ShowAlertAsync(string title, string message, string cancel = "OK");
    }
    
    public class GenericService : IGenericService
    {
        public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            var page = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
            if (page != null)
                await page.DisplayAlert(title, message, cancel);
        }
    }
}
