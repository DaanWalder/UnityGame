using System.IO;
using UnityEditor;

namespace ProWorldEditor
{
    public static class EUtil
    {
        public static void CheckTexture(string path)
        {
            if (File.Exists(path))
            {
                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
                if (!textureImporter.isReadable)
                {
                    AssetDatabase.StartAssetEditing();
                    textureImporter.textureType = TextureImporterType.Default;
                    textureImporter.isReadable = true;
                    AssetDatabase.ImportAsset(path);
                    AssetDatabase.Refresh();
                    AssetDatabase.StopAssetEditing();
                }
            }
        }
    }
}
