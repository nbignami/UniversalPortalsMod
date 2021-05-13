using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

public class UniversalPortalsConfig
{
    public static UniversalPortalsConfig instance = new UniversalPortalsConfig();

    public UniversalPortalsConfig()
    {
        ShowMarkersOnMapSelection = false;
        SaveLastSelection = false;
    }

    public bool ShowMarkersOnMapSelection;
    public bool SaveLastSelection;

    private string FilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "UniversalPortalsConfiguration.json");

    public void LoadFromFile()
    {
        try
        {
            using (var reader = new StreamReader(FilePath))
            {
                var loadedConfig = JsonUtility.FromJson<UniversalPortalsConfig>(reader.ReadToEnd());

                ShowMarkersOnMapSelection = loadedConfig.ShowMarkersOnMapSelection;
                SaveLastSelection = loadedConfig.SaveLastSelection;
            }
        }
        catch (System.Exception)
        {
            SaveToFile();
        }
    }

    public void SaveToFile()
    {
        try
        {
            using (var writer = new StreamWriter(File.Create(FilePath)))
            {
                var json = JsonUtility.ToJson(this, true);

                writer.Write(json);
            }
        }
        catch (System.Exception ex)
        {
            FileLog.Log(ex.Message);
        }
    }

    public static void SetConfig(ZRpc sender, ZPackage package)
    {
        try
        {
            instance = package.ToObject<UniversalPortalsConfig>();
        }
        catch (System.Exception ex)
        {
            FileLog.Log(ex.Message);
        }
    }
}