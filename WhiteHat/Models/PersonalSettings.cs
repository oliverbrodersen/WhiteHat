using WhiteHat.Enums;

namespace WhiteHat.Models
{
    public class PersonalSettings
    {
        public bool ShowShortcuts { get; set; }
        public bool DismissWelcome { get; set; }
        public PaneSize PaneSize { get; set; }
    }
}
