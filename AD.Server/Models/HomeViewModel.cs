namespace AD.Server.Models;

public class HomeViewModel
{
    public List<HQPanelEntry> Shortcuts { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string? GroupNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<ClaimInfo> Claims { get; set; } = new();
}

public class ClaimInfo
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class HQPanelEntry
{
    public string Title { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
