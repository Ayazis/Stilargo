using System;
using System.IO;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>
    /// Reads a JSON config file from <c>StreamingAssets/Config</c>, which ships next to the exe so values can be
    /// tuned without rebuilding. Keys missing from the file keep the defaults of <typeparamref name="T"/>.
    /// </summary>
    public static class ConfigFile
    {
        private const string FolderName = "Config";

        public static T Read<T>(string fileName) where T : new()
        {
            string path = Path.Combine(Application.streamingAssetsPath, FolderName, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Config file '{fileName}' is missing; expected it at '{path}'.", path);
            }

            T values = new T();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(path), values);
            return values;
        }
    }
}
