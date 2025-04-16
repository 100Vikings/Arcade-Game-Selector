using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FolderScanner : Singleton<FolderScanner>
{
    [field: SerializeField] public string GameRootFolder { get; private set; } = "C:\\Temp";
    public GameUIController uiController;

    public FileInfo[] FileList { get; private set; }
    public FileInfo[] GameInfoFiles { get ; private set; }
    public FileInfo[] GameExeFiles { get; private set; }

    public GameData[] DataArray { get; private set; }

    public void Start()
    {
        ScanForGames();
    }

    private void ScanForGames()
    {
        DirectoryInfo directoryInfo = new(GameRootFolder);

        GetGameInfoFiles(directoryInfo);
        GetGameExeFiles(directoryInfo);
    }

    private void GetGameInfoFiles(DirectoryInfo directoryInfo)
    {
        FileList = directoryInfo.GetFiles("*.json", SearchOption.AllDirectories);
        GameInfoFiles = (from file in FileList
                         where file.Name == "gameInfo.json"
                         select file).ToArray();

        //ParseGameInfoFiles(gameInfoFiles, directoryInfo);
        ParseJSONFiles(GameInfoFiles, directoryInfo);
    }

    private void ParseJSONFiles(FileInfo[] jsonFiles, DirectoryInfo directoryInfo)
    {
        DataArray = new GameData[jsonFiles.Length];
        int index = 0;

        foreach (FileInfo file in jsonFiles)
        {
            using StreamReader reader = new(file.FullName);
            DataArray[index++] = JsonUtility.FromJson<GameData>(reader.ReadToEnd());
        }

        string[] titles = (from data in DataArray select data.title).ToArray();
        string[] thumbnails = (from data in DataArray select data.thumbnail).ToArray();

        uiController.SetTitles(titles);
        LoadThumbnails(thumbnails, directoryInfo);
    }

    private void LoadThumbnails(string[] thumbnails, DirectoryInfo directoryInfo)
    {
        FileInfo[] fileList = directoryInfo.GetFiles("*.jpg", SearchOption.AllDirectories);
        string[] fullPath = new string[thumbnails.Length];

        for (int i = 0; i < thumbnails.Length; i++)
        {
            fullPath[i] = (from file in fileList
                           where file.FullName.Contains(thumbnails[i])
                           select file.FullName).First();
        }

        StartCoroutine(LoadThumbnailsCoroutine(fullPath));
    }

    private IEnumerator LoadThumbnailsCoroutine(string[] fullPath)
    {
        Sprite[] loadedSprites = new Sprite[fullPath.Length];
        int index = 0;

        foreach (string file in fullPath)
        {
            string url = $"file:///{file.Replace("\\", "/")}";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                loadedSprites[index++] = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
            else
            {
                UnityEngine.Debug.LogWarning(request.downloadHandler.error);
            }
        }

        uiController.SetThumbnails(loadedSprites);
    }

    private void GetGameExeFiles(DirectoryInfo directoryInfo)
    {
        FileInfo[] fileList = directoryInfo.GetFiles("*.exe", SearchOption.AllDirectories);
        GameExeFiles = (from file in fileList
                        where !file.Name.Contains("UnityCrash")
                        select file).ToArray();
    }
}