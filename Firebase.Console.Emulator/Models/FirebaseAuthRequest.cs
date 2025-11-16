using System.Text.Json.Serialization;

namespace Firebase.Emulator.Models
{
    public class FirebaseAuthRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
        [JsonPropertyName("returnSecureToken")]
        public bool ReturnSecureToken { get; set; }
    }
}
