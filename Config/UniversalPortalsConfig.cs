using BepInEx.Configuration;

public class UniversalPortalsConfig
{
    public static UniversalPortalsConfig instance = new UniversalPortalsConfig();

    public UniversalPortalsConfig()
    {
    }

    public ConfigEntry<bool> ShowMarkersOnMapSelection { get; set; }
    public ConfigEntry<bool> SaveLastSelection { get; set; }
}