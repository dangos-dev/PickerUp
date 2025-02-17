using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PickerUp.Source;

public static class Settings {
    /// Backing fields
    private static bool _alwaysOnTop;

    private static bool _autoCopy;
    private static bool _showAlpha;

    /// Propiedades
    public static bool AlwaysOnTop {
        get => _alwaysOnTop;
        set {
            if (_alwaysOnTop == value) return;

            _alwaysOnTop = value;
            SaveSettings();
        }
    }

    public static bool AutoCopy {
        get => _autoCopy;
        set {
            if (_autoCopy == value) return;

            _autoCopy = value;
            SaveSettings();
        }
    }

    public static bool ShowAlpha {
        get => _showAlpha;
        set {
            if (_showAlpha == value) return;

            _showAlpha = value;
            SaveSettings();
        }
    }

    private static SemaphoreSlim _fileLock = new SemaphoreSlim(1);
    private const string ConfigFileName = "config.ini";

    public static async void LoadSettingsFromFileAsync() {

        try{

            IStorageFile settingsFile = await StorageFile.GetFileFromPathAsync(Path.Combine(MainWindow.AppLocalFolder, ConfigFileName));
            var configLines = await FileIO.ReadLinesAsync(settingsFile);

            // Convertir el archivo a un diccionario de pares de claves-valores
            var settingsDictionary = new Dictionary<string, string>();
            foreach (var line in configLines){
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('[') || line.StartsWith(';')) continue;

                var kvp = line.Split('=', 2);// Split the key and value
                if (kvp.Length == 2){
                    settingsDictionary[kvp[0].Trim()] = kvp[1].Trim();
                }
            }

            AlwaysOnTop = settingsDictionary.ContainsKey("AlwaysOnTop") && settingsDictionary["AlwaysOnTop"] == "true";
            AutoCopy = settingsDictionary.ContainsKey("AutoCopy") && settingsDictionary["AutoCopy"] == "true";
            ShowAlpha = settingsDictionary.ContainsKey("ShowAlpha") && settingsDictionary["ShowAlpha"] == "true";
        }
        catch (FileNotFoundException){
            // Si el archivo no existe devuelve valores predeterminados
            AlwaysOnTop = false;
            AutoCopy = false;
            ShowAlpha = false;
        }

    }

    public static void SaveSettings() {
        var settingsDictionary = new Dictionary<string, string> {
            { "AlwaysOnTop", AlwaysOnTop.ToString().ToLower() },
            { "AutoCopy", AutoCopy.ToString().ToLower() },
            { "ShowAlpha", ShowAlpha.ToString().ToLower() }
        };

        Task.Run(async () => await SaveSettingsAsFileAsync(settingsDictionary));
    }

    private static async Task<bool> SaveSettingsAsFileAsync(Dictionary<string, string> settings) {

        StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(MainWindow.AppLocalFolder);

        var lines = new List<string> {
            "[Settings]"
        };

        lines.AddRange(settings.Select(kvp => $"{kvp.Key}={kvp.Value}"));

        try{
            await _fileLock.WaitAsync();
            StorageFile configFile = await localFolder.CreateFileAsync(ConfigFileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(configFile, lines);

            return true;
        }
        catch{
            return false;
        }
        finally{
            _fileLock.Release();
        }
    }
}
