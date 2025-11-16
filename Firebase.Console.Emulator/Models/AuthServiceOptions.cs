namespace Firebase.Emulator.Models
{
    internal class AuthServiceOptions
    {
        public string AuthEmulatorSignInUrl { get; set; } = string.Empty;
        public string AuthEmulatorSignUpUrl { get; set; } = string.Empty;
        public TestUserOptions TestUser { get; set; } = new();
    }
}
