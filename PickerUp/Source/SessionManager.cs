using PickerUp.Source.Colors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PickerUp.Source;

public class SessionManager {

    private static SemaphoreSlim _fileLock = new(1);
    private const string SessionFileName = "session.json";

    public static async void LoadSessionFromFileAsync() {

        try{

            IStorageFile sessionFile = await StorageFile.GetFileFromPathAsync(Path.Combine(MainWindow.AppLocalFolder, SessionFileName));
            string jsonSession = await FileIO.ReadTextAsync(sessionFile);

            var sessionData = JsonSerializer.Deserialize<SessionData>(jsonSession);
            ColorPicked.Pick(sessionData.ColorPicked);


            // History.Colors = sessionData.History;
            foreach (IColor historyColor in sessionData.History){
                History.AddColor(historyColor);
            }

        }
        catch (FileNotFoundException){
            return;
        }
    }

    public static void SaveSession() {
        SessionData sessionData = new() {
            ColorPicked = ColorPicked.Get(),
            History = History.Colors
        };

        Task.Run(async () => await SaveSessionAsFileAsync(sessionData));
    }

    private static async Task<bool> SaveSessionAsFileAsync(SessionData session) {

        StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(MainWindow.AppLocalFolder);

        string json = JsonSerializer.Serialize(session);

        try{
            await _fileLock.WaitAsync();
            StorageFile sessionFile = await localFolder.CreateFileAsync(SessionFileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sessionFile, json);

            return true;
        }
        catch{
            return false;
        }
        finally{
            _fileLock.Release();
        }
    }

    private struct SessionData {
        public IColor ColorPicked { get; set; }

        public IColor[] History { get; set; }
    }
}
