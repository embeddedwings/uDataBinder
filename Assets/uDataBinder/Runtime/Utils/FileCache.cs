using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace uDataBinder.Utils
{
    public static class FileCache
    {
        public static string GetHash(string str)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(str));
            return string.Join("", hash.Select(v => v.ToString("x2")));
        }

        public static string GetCacheDirectory(string directory = "cache")
        {
            var cacheDirectory = $"{Application.persistentDataPath}/{directory}";
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }
            return cacheDirectory;
        }

        public static string GetCachePath(string name, string directory = "cache")
        {
            return $"{GetCacheDirectory(directory)}/{GetHash(name)}";
        }

        public static bool Exists(string name, string directory = "cache")
        {
            return File.Exists(GetCachePath(name, directory));
        }

        public static void Save(string name, byte[] bytes, string directory = "cache")
        {
            File.WriteAllBytes(GetCachePath(name, directory), bytes);
        }

        public static byte[] Load(string name, string directory = "cache")
        {
            var path = GetCachePath(name, directory);
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            return null;
        }

        public static void Delete(string name, string directory = "cache")
        {
            var path = GetCachePath(name, directory);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static long GetSize(string name, string directory = "cache")
        {
            var path = GetCachePath(name, directory);
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            return 0;
        }

        public static void Clear(string directory = "cache")
        {
            var cacheDirectory = GetCacheDirectory(directory);
            if (Directory.Exists(cacheDirectory))
            {
                Directory.Delete(cacheDirectory, true);
            }
        }
    }
}
