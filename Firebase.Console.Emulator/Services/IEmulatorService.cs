namespace Firebase.Emulator.Services
{
    public interface IEmulatorService
    {
        Task<bool> StartFirebaseEmulator();
        void StopFirebaseEmulator();
    }
}
