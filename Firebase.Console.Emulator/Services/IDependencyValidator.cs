namespace Firebase.Emulator.Services
{
    public interface IDependencyValidator
    {
        Task<bool> ValidateAsync();
    }
}
