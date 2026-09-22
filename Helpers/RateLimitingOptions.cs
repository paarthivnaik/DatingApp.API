namespace DatingApp.API.Helpers
{
    public class RateLimitingOptions
    {
        public int RequestsPerMinute { get; set; } = 20;
        public int WindowInSeconds { get; set; } = 60;
    }
}
