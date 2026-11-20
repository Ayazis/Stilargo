using System.IO;
using UnityEditor;

namespace Stilargo.Editor
{
    /// <summary>Creates project folders for generated assets, including any missing parents.</summary>
    public static class AssetFolders
    {
        public static void Ensure(string folder)
        {
            string normalized = folder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string parent = Path.GetDirectoryName(normalized).Replace('\\', '/');
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(normalized));
        }
    }
}
