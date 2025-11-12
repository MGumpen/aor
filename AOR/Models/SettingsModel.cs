namespace AOR.Models
{
    public class SettingsModel
    {
        // Preferred language for the user interface
        public string Language { get; set; } = "en";

        // Theme mode: "light" or "dark"
        public string Theme { get; set; } = "light";

        // Optional future feature — whether user wants email notifications
        public bool EmailNotifications { get; set; } = true;
    }
}
