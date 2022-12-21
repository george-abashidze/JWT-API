namespace JWT_API.Helpers
{
    public class SignUpConfig
    {
        public int MinPasswordLength { get; set; }
        public bool ShouldContainNumber { get; set; }
        public bool RequireEmailComfirmation { get; set; }
    }
}
