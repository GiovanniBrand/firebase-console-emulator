namespace Firebase.Emulator.Configurations
{
    public class EmulatorOptions
    {
        public string Command { get; set; } = string.Empty;
        public int PortToCheck { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}
