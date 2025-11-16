namespace Firebase.Emulator.Models
{
    internal class TestEmulatorConfig
    {
        public string Command { get; set; } = string.Empty;
        public int PortToCheck { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}
