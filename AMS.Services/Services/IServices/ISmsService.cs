namespace AMS.Services.Services.IServices
{
    public interface ISmsService
    {
         Task SendMessageAsync(string to, string message);
    }
}
