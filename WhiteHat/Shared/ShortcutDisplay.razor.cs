using Microsoft.AspNetCore.Components;

namespace WhiteHat.Shared;
public partial class ShortcutDisplay
{
    [Parameter]
    public string Icon { get; set; }
    
    [Parameter]
    public string Title { get; set; }
    
    [Parameter]
    public string Key { get; set; }
}
