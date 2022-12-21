using JWT_API.Helpers;
using System.Text.Json.Serialization;

namespace JWT_API.Models.Request
{
    public class SignUpRequest
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        public string? Validate(SignUpConfig config) {

            if (String.IsNullOrEmpty(FirstName.Trim()) ||
                String.IsNullOrEmpty(LastName.Trim()) ||
                String.IsNullOrEmpty(Email.Trim()) ||
                String.IsNullOrEmpty(Password.Trim())) {
                return "Not all required fields filled.";
            }

            if (FirstName.Length < 2 || LastName.Length < 2) {
                return "FirstName or/and LastName is too short.";
            }

            if (Password.Length < config.MinPasswordLength) {
                return "Password must be at leaset 5 characters long.";
            }

            if (FirstName.Any(char.IsDigit) || LastName.Any(char.IsDigit)) {
                return "First and Last names should not contain digits.";
            }

            if (config.ShouldContainNumber) {
                if (!Password.Any(char.IsDigit))
                    return "Password mus contain at least 1 digit.";
            }

            return null;
        }
    }
}
