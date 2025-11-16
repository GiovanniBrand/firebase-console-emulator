using System.Text.Json.Serialization;

namespace Firebase.Emulator.Models
{
    public class FirebaseAuthResponse
    {
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; }
        [JsonPropertyName("localId")]
        public string LocalId { get; set; }
    }
}
