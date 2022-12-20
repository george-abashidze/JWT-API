using System.Text.Json.Serialization;

namespace JWT_API.Models.Request
{
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
