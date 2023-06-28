using UnityEngine;
using System.IO;

public static class SaveManager
{
    public static readonly string path = Path.Combine(Application.persistentDataPath + "/settings.fps");
    public static readonly string json = Path.Combine(Application.persistentDataPath + "/settings.json");
    public static void SaveSettings (UISettings uISettings)
    {
        FileStream stream = new(path, FileMode.Create);
        BinaryWriter writer = new(stream);

        SettingsData settingsData = SettingsData.FromSettings(uISettings);
        writer.Write(settingsData.ToggleCrouch);
        writer.Write(settingsData.InvertYAxis);
        writer.Write(settingsData.ReduceMotion);
        writer.Write(settingsData.MoveCamera);
        writer.Write(settingsData.BobX);
        writer.Write(settingsData.BobY);
        writer.Write(settingsData.Tilt);
        writer.Write(settingsData.SensX);
        writer.Write(settingsData.SensY);
        writer.Write(settingsData.Smoothing);
        writer.Write(settingsData.Volume);
        
        stream.Close();
    }

    public static SettingsData LoadSettings ()
    {
        if (File.Exists(path))
        {
            FileStream stream = new(path, FileMode.Open);
            BinaryReader reader = new(stream);

            SettingsData settingsData = new()
            {
                ToggleCrouch = reader.ReadBoolean(),
                InvertYAxis = reader.ReadBoolean(),
                ReduceMotion = reader.ReadBoolean(),
                MoveCamera = reader.ReadBoolean(),
                BobX = reader.ReadBoolean(),
                BobY = reader.ReadBoolean(),
                Tilt = reader.ReadBoolean(),
                SensX = reader.ReadSingle(),
                SensY = reader.ReadSingle(),
                Smoothing = reader.ReadSingle(),
                Volume = reader.ReadSingle()
            };
            stream.Close();

            return settingsData;
        } else
        {
            return SettingsData.FromDefault();
        }
    }

    public static void SaveSettingsJson (UISettings uISettings)
    {
        SettingsData settingsData = SettingsData.FromSettings(uISettings);
        string jsonIn = JsonUtility.ToJson(settingsData);

        File.WriteAllText(json, jsonIn);
    }

    public static SettingsData LoadSettingsJson (bool reset)
    {
        if (reset) return SettingsData.FromDefault();
        
        if (File.Exists(json))
        {
            _ = new SettingsData();

            string jsonOut = File.ReadAllText(json);
            SettingsData settingsData = JsonUtility.FromJson<SettingsData>(jsonOut);

            return settingsData;
        } else
        {
            return SettingsData.FromDefault();
        }
    }
}