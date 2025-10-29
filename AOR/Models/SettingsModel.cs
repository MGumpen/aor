namespace AOR.Models
{
    public class SettingsModel
    {
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "light";
        public bool EmailNotifications { get; set; } = true;
    }
}

