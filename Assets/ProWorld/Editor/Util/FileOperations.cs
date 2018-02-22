using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Ionic.Zip;
using UnityEngine;

namespace ProWorldEditor
{
    public class FileOperations
    {
        public static void SaveToFile(string path, object data)
        {
            using (var zipFile = new ZipFile())
            {
                zipFile.AlternateEncoding = Encoding.Unicode;

                //serialize item to memorystream
                using (var m = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(m, data);
                    m.Position = 0;

                    var zipEntry = zipFile.AddEntry("entry", m);

                    zipEntry.AlternateEncoding = Encoding.Unicode;

                    zipFile.Save(path);
                }
            }
        }
        public static EditorData LoadFromFile(string path)
        {
            using (var m = new MemoryStream())
            {
                using (var zipFile = new ZipFile(path))
                {
                    zipFile.AlternateEncoding = Encoding.Unicode;

                    // If no file found, just create a new grid
                    if (zipFile["entry"] == null) return new EditorData();

                    ZipEntry ze = zipFile["entry"];
                    ze.AlternateEncoding = Encoding.Unicode;
                    //e.Password = _password;
                    ze.Extract(m);
                    m.Position = 0;

                    //now serialize it back to the correct type
                    IFormatter formatter = new BinaryFormatter();
                    try
                    {
                        var world = (EditorData) formatter.Deserialize(m);
                        //HandleVersionChanges(data);
                        return world;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Load failed:" + e);
                        return new EditorData();
                    }
                }
            }

        }
        public static void SaveTemp(EditorData data)
        {
            if (Application.persistentDataPath == string.Empty)
            {
                Debug.Log("Application.persistentDataPath is empty.");
                return;
            }

            //var path = Application.dataPath + "/temp.pw";
            var path = Application.persistentDataPath + "/temp.pw";
            SaveToFile(path, data);
        }
        public static EditorData LoadTemp()
        {
            if (Application.persistentDataPath == string.Empty)
            {
                Debug.Log("Application.persistentDataPath is empty.");
                return new EditorData();
            }

            //var path = Application.dataPath + "/temp.pw";
            var path = Application.persistentDataPath + "/temp.pw";
            if (!File.Exists(path))
                return new EditorData();

            var f = LoadFromFile(path);
            return f;
        }
    }
}
