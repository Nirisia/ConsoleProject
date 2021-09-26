///-----------------------------------------------------------------
///   Author : Théo Sabattié                    
///   Date   : 21/09/2020 10:00
///-----------------------------------------------------------------

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Com.Atomicorn {
    public class RemoveEmptyDirectoriesOnSave : UnityEditor.AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            int assetsLength = "Assets/".Length;
            string[] exceptPaths = (string[])paths.Clone();

            for (int i = paths.Length - 1; i > -1; i--)
                exceptPaths[i] = Path.Combine(Application.dataPath, paths[i].Substring(assetsLength, paths[i].Length - assetsLength - ".meta".Length));

            DeleteEmptyDirectories(Application.dataPath, exceptPaths);
            AssetDatabase.Refresh();

            return paths;
        }

        private static void DeleteEmptyDirectories(in string fromDirectory, string[] exceptDirectory)
        {
            string[] allDirectories = Directory.GetDirectories(fromDirectory);
            string directory;

            for (int i = 0, length = allDirectories.Length; i < length; i++)
            {
                directory = allDirectories[i];

                if (Array.Exists(exceptDirectory, d => d.Contains(directory)))
                    continue;

                if (Directory.GetFiles(directory).Length == 0)
                    DeleteDirectoryAndMeta(directory);
                else
                    DeleteEmptyDirectories(directory, exceptDirectory);
            }

            if (Directory.GetFiles(fromDirectory).Length == 0)
                DeleteDirectoryAndMeta(fromDirectory);
        }

        private static void DeleteDirectoryAndMeta(in string directory)
        {
            Directory.Delete(directory);
            string meta = $"{directory}.meta";

            if (File.Exists(meta))
                File.Delete(meta);
        }
    }
}