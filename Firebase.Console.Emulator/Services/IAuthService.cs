namespace Firebase.Emulator.Services
{
    public interface IAuthService
    {
        Task GetOrCreateUserToken();
    }
}
