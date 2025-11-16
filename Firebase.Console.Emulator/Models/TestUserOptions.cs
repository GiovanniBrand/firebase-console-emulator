namespace Firebase.Emulator.Models
{
    internal class TestUserOptions
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool ReturnSecureToken { get; set; }
    }
}
