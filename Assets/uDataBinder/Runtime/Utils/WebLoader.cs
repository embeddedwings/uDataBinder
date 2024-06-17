using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using uDataBinder.Utils;
using UnityEngine;
using UnityEngine.Networking;

public static class WebLoader
{
    private static async Task<byte[]> LoadBytes(string url, string directory = "cache", bool force = false, IProgress<float> progress = null)
    {
        if (!force && FileCache.Exists(url, directory))
        {
            progress?.Report(1.0f);
            return FileCache.Load(url, directory);
        }

        var webRequest = UnityWebRequest.Get(url);
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            progress?.Report(webRequest.downloadProgress);
            await Task.Yield();
        }
        progress?.Report(1.0f);

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(webRequest.error);
            return null;
        }

        var bytes = webRequest.downloadHandler.data;
        FileCache.Save(url, bytes, directory);
        return bytes;
    }

    private static async Task<Texture2D> LoadTexture(string url, string directory = "cache", bool force = false, IProgress<float> progress = null)
    {
        var bytes = await LoadBytes(url, directory, force, progress);
        if (bytes == null)
        {
            return null;
        }

        var texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        return texture;
    }

    private static async Task<Sprite> LoadSprite(string url, string directory = "cache", bool force = false, IProgress<float> progress = null)
    {
        var texture = await LoadTexture(url, directory, force, progress);
        if (texture == null)
        {
            return null;
        }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    private static async Task<AudioClip> LoadAudioClip(string url, string directory = "cache", bool force = false, IProgress<float> progress = null)
    {
        var path = url;
        if (!force && FileCache.Exists(url, directory))
        {
            path = FileCache.GetCachePath(url, directory);
        }

        var type = AudioType.UNKNOWN;
        var match = Regex.Match(url, "\\.([^/\\.]+)[\\?$]");
        if (match.Success)
        {
            switch (match.Groups[1].Value.ToLower())
            {
                case "wav":
                    type = AudioType.WAV;
                    break;
                case "mp3":
                    type = AudioType.MPEG;
                    break;
                case "ogg":
                    type = AudioType.OGGVORBIS;
                    break;
            }
        }

        var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, type);
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            progress?.Report(webRequest.downloadProgress);
            await Task.Yield();
        }
        progress?.Report(1.0f);

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(webRequest.error);
            return null;
        }

        if (path == url)
        {
            var bytes = webRequest.downloadHandler.data;
            FileCache.Save(url, bytes, directory);
        }

        var audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
        return audioClip;
    }

    public static async Task<string> LoadString(string url, string directory = "cache", bool force = false, IProgress<float> progress = null)
    {
        var bytes = await LoadBytes(url, directory, force, progress);
        return bytes == null ? null : System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static async Task<T> LoadAsset<T>(string url, string directory = "cache", bool force = false, IProgress<float> progress = null) where T : class
    {
        if (typeof(T) == typeof(Texture2D))
        {
            return await LoadTexture(url, directory, force, progress) as T;
        }
        else if (typeof(T) == typeof(Sprite))
        {
            return await LoadSprite(url, directory, force, progress) as T;
        }
        else if (typeof(T) == typeof(AudioClip))
        {
            return await LoadAudioClip(url, directory, force, progress) as T;
        }
        else if (typeof(T) == typeof(string))
        {
            return await LoadString(url, directory, force, progress) as T;
        }
        else if (typeof(T) == typeof(byte[]))
        {
            return await LoadBytes(url, directory, force, progress) as T;
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public static async Task DownloadAssets(string[] urls, string directory = "cache", bool force = false, IProgress<float> progress = null)
    {
        progress?.Report(0.0f);

        var tasks = new List<Task>();
        var progresses = new List<float>(urls.Length);

        var i = 0;
        foreach (var url in urls)
        {
            tasks.Add(LoadBytes(url, directory, force, new Progress<float>(f =>
            {
                progresses[i] = f;
                progress?.Report(progresses.Sum() / urls.Count());
            })));
        }

        await Task.WhenAll(tasks);
        progress?.Report(1.0f);
    }

    public static async Task<long> GetAssetSize(string url, string directory = "cache", bool force = false)
    {
        if (!force && FileCache.Exists(url, directory))
        {
            return FileCache.GetSize(url, directory);
        }

        var webRequest = UnityWebRequest.Head(url);
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
            await Task.Yield();
        }

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(webRequest.error);
            return 0;
        }
        return long.Parse(webRequest.GetResponseHeader("Content-Length"));
    }

    public static async Task<long> GetAssetsSize(string[] urls, string directory = "cache", bool force = false)
    {
        var tasks = new List<Task<long>>();
        foreach (var url in urls)
        {
            tasks.Add(GetAssetSize(url, directory, force));
        }

        await Task.WhenAll(tasks);
        return tasks.Sum(t => t.Result);
    }
}
