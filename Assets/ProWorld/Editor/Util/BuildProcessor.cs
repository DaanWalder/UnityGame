using System.IO;
using ProWorldEditor;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class BuildProcessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var projectName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
        var dataPath = Path.GetDirectoryName(pathToBuiltProject) + "/" + projectName + "_Data/ProWorld/";

        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);

        var sourcePaths = Directory.GetDirectories(Application.dataPath, EditorData.Folder, SearchOption.AllDirectories);

        foreach (var source in sourcePaths)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                var newPath = dataPath + dirPath.Substring(source.Length);
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);
            }

            //Copy all the files
            foreach (var fileName in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(fileName) == ".pw")
                {
                    // We load the editor script
                    var pw = FileOperations.LoadFromFile(fileName);

                    var newFile = dataPath + fileName.Substring(source.Length);
                    newFile = Path.ChangeExtension(newFile, "world");

                    ProWorldSDK.FileOperations.SaveToFile(newFile, pw.World);
                }
            }
        }
    }

    [PostProcessScene]
    public static void OnPostprocessScene()
    {
        var wbs = (WorldBuilder[]) Object.FindObjectsOfType(typeof (WorldBuilder));

        foreach (var worldBuilder in wbs)
        {
            var sourcePaths = Directory.GetDirectories(Application.dataPath, EditorData.Folder, SearchOption.AllDirectories);

            foreach (var sourcePath in sourcePaths)
            {
                var fullPath = sourcePath + worldBuilder.WorldFileLocation;
                fullPath = fullPath.Replace('\\', '/');

                if (File.Exists(fullPath))
                {
                    var editorData = FileOperations.LoadFromFile(fullPath);
                    worldBuilder.World = editorData.World;

                    break;
                }
            }
        }
    }
}
