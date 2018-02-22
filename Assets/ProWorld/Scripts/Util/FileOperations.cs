using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Ionic.Zip;
using UnityEngine;

namespace ProWorldSDK
{
    public class FileOperations
    {
        public static void SaveToFile(string path, World world)
        {
            using (var zipFile = new ZipFile())
            {
                zipFile.AlternateEncoding = Encoding.Unicode;

                //serialize item to memorystream
                using (var m = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(m, world);
                    m.Position = 0;

                    var zipEntry = zipFile.AddEntry("entry", m);

                    zipEntry.AlternateEncoding = Encoding.Unicode;

                    zipFile.Save(path);
                }
            }
        }
        public static World LoadFromFile(string path)
        {
            using (var m = new MemoryStream())
            {
                using (var zipFile = new ZipFile(path))
                {
                    zipFile.AlternateEncoding = Encoding.Unicode;

                    // If no file found, just create a new grid
                    if (zipFile["entry"] == null) return new World();

                    ZipEntry e = zipFile["entry"];
                    e.AlternateEncoding = Encoding.Unicode;
                    //e.Password = _password;
                    e.Extract(m);
                    m.Position = 0;

                    //now serialize it back to the correct type
                    IFormatter formatter = new BinaryFormatter();
                    var world = (World)formatter.Deserialize(m);

                    return world;
                }
            }

        }

        public static void SaveImage(float[,] data, string path)
        {
            if (data == null) throw new UnityException("data is invalid for path:" +path );

            var height = data.GetLength(0);
            var width = data.GetLength(1);

            var texture = new Texture2D(width, height);
            Util.ApplyMapToTexture(ref texture, data);

            var bytes = texture.EncodeToPNG();
            var file = File.Open(path, FileMode.Create);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();

            Object.DestroyImmediate(texture);
        }
        public static void SaveImage(Color[,] data, string path)
        {
            var height = data.GetLength(0);
            var width = data.GetLength(1);

            var texture = new Texture2D(width, height);

            var d = new Color[height * width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; y < height; x++)
                {
                    d[y * width + x] = data[y, x];
                }
            }

            texture.SetPixels(d);
            texture.Apply();

            var bytes = texture.EncodeToPNG();
            var file = File.Open(path, FileMode.Create);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();

            Object.DestroyImmediate(texture);
        }
    }
}