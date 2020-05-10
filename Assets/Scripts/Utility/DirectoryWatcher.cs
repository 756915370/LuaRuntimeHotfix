using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DirectoryWatcher
{
    public DirectoryWatcher(string dirPath, FileSystemEventHandler handler)
    {
        Debug.Log("create directory watcher");
        CreateWatch(dirPath, handler);
    }

    void CreateWatch(string dirPath, FileSystemEventHandler handler)
    {
        if (!Directory.Exists(dirPath)) return;

        var watcher = new FileSystemWatcher();
        watcher.IncludeSubdirectories = true; //includeSubdirectories;
        watcher.Path = dirPath;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Filter = "*.lua";
        watcher.Changed += handler;
        watcher.EnableRaisingEvents = true;
        watcher.InternalBufferSize = 10240;
    }
}