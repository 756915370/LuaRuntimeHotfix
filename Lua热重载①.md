这几天由于项目组需要一直在琢磨一个功能，就是如何在**unity编辑器下不需要重启游戏就能让lua文件改动后立刻生效**。如果能够实现这个功能，那会大幅提高开发效率。查了一圈，网上的结果都不太满意，要么只有理论没有源码，要么有源码但是考虑的情况过于简单。所以自己打算写博客告诉大家，我是怎么实现的，并且提供完整源码。[github工程地址](https://github.com/756915370/LuaRuntimeHotfix)
使用的unity2019.3.0 + xlua。改成其他lua也是可以用，只要在传入luaEnv的时候做相应改动就可以了。
***
这个功能大体分为两大步：
- **检测哪些lua文件发生变化**
- **重新加载lua模块，保留数据，替换函数** *（因为我们做了函数、数据分离，所以我这个工程目前只考虑替换函数）*
其中重载lua模块还要考虑以下问题:
	- **其它模块缓存了旧模块的函数的处理**
	- **upvalue值的处理**
	- **需要更新的模块的元表的处理**
	- **对于正在运行的函数的处理，比如update**
	- **解决了表循环引用导致无限递归的情况**（2020.8.17更新）
***
内容有点多，一篇文章应该塞不下，所以这第一篇先讲第一步，**怎么检测哪些lua文件发生变化。** 涉及到工程里两个类:
- **DirectoryWatcher**，检测文件变化
- **LuaFileWatcher**，处理检测文件发生变化后该做什么事情
***
#### DirectoryWatcher
```csharp
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
        watcher.Filter = "*。lua";
        watcher.Changed += handler;
        watcher.EnableRaisingEvents = true;
        watcher.InternalBufferSize = 10240;
    }
}
```
这里涉及到一个C#的系统类**FileSystemWatcher**
##### FileSystemWatcher
监控指定文件或目录的文件的创建、删除、改动、重命名等活动。可以动态地定义需要监控的文件类型及文件属性改动的类型。
- **IncludeSubdirectories**  是否包含子文件。
- **Path** 目标路径。
- **NotifyFilter** 设置文件的哪些属性的变动会触发 Changed事件。这里设置成了当文件内容发生变化时会触发。
- **Filter** 设置筛选字符串，用于确定在目录中监视哪些类型的文件。这里只需要筛选 **.lua后缀文件**即可。
- **Changed** 文件发生改变时的监听事件，需要一个**FileSystemEventHandler** 类型的委托。除了**Changed**外还可以监听**Renamed**、**Deleted**、**Created**。
- **EnableRaisingEvents** 设置是否开始监控，默认为false 
-  **InternalBufferSize** 能够监听的改动大小。如果监听事件没有触发，请把这个值设得大一点。
还有一些其他属性，详细看[MSDN关于FileSystemWatcher](https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=netcore-3.1)
***
#### LuaFileWatcher
```csharp
public  class LuaFileWatcher
{
    //private static ReloadDelegate ReloadFunction;
    
    private static HashSet<string> _changedFiles = new HashSet<string>();
    
    public static void CreateLuaFileWatcher(LuaEnv luaEnv)
    {
        var scriptPath = Path.Combine(Application.dataPath, "LuaScripts");
        var directoryWatcher =
            new DirectoryWatcher(scriptPath, new FileSystemEventHandler(LuaFileOnChanged));
        //ReloadFunction = luaEnv.Global.Get<ReloadDelegate>("hotfix");
        EditorApplication.update -= Reload;
        EditorApplication.update += Reload;
    }

    private static void LuaFileOnChanged(object obj, FileSystemEventArgs args)
    {
        var fullPath = args.FullPath;
        var luaFolderName = "LuaScripts";
        var requirePath = fullPath.Replace(".lua", "");
        var luaScriptIndex = requirePath.IndexOf(luaFolderName) + luaFolderName.Length + 1;
        requirePath = requirePath.Substring(luaScriptIndex);
        requirePath = requirePath.Replace('\\','.');
        _changedFiles.Add(requirePath);
    }

    private static void Reload()
    {
        if (EditorApplication.isPlaying == false)
        {
            return;
        }
        if (_changedFiles.Count == 0)
        {
            return;
        }

        foreach (var file in _changedFiles)
        {
            //ReloadFunction(file);
            Debug.Log("Reload:" + file);
        }
        _changedFiles.Clear();
    }
}
```
**LuaFileWatcher**做了以下几件事情:
- **将文件路径转化为lua里调用require函数需要的参数**
之前提到的**FileSystemEventHandler**的委托有两个参数，一个是**object**即对应的文件，另一个参数**FileSystemEventArgs**包含了文件的数据，其中有**FullPath即文件路径**。是文件的完整路径，如 **"F:\Git\LuaRuntimeHotfix\Assets\LuaScripts\NewDirectory1\Test.lua"**。需要转化成 **"NewDirectory1.Test"** 。"LuaScripts"是我的工程里的lua文件夹名称。
这一步是**LuaFileOnChanged**这个函数的主要内容。
- **将改动的文件记录下来，并在主线程中对这些文件进行重载**
为什么需要记录，原因是因为**FileSystemWatcher是多线程的。每新建一个FileSystemWatcher都相当于开了一个新线程。** 如果不拿一个列表记录，**直接在多线程下重载lua模块，极其容易导致unity崩溃!!!** 这个我不知道是unity的限制还是lua的限制，总之这个崩溃问题困扰了我好几天，最后才发现原因。
解决办法也很简单，**拿一个列表去存文件路径，再到主线程下处理，让EditorApplication.update绑定处理函数**，这样unity编辑器每刷新一次就会调用处理函数。
需要注意的一点是，**EditorApplication.update即使unity编辑器没有在运行模式也会跑**，所以需要加入下面代码判断编辑器是不是在运行模式:

```csharp
	if (EditorApplication.isPlaying == false)
	 {
	     return;
	 }
```

这里我使用**HashSet** 这个数据结构，因为**HashSet不会存储重复元素**，如果用**List**还要考虑列表里可能有重复元素的情况。
这一步是**Reload**这个函数的主要内容，当然这个函数里还需要调用lua端的方法，这部分内容下篇文章再说，今天先做到能够打印出需要修改的全部模块路径。
***
关于第一步**检测哪些lua文件发生变化**的代码就讲解到这里，剩下的内容下一篇文章进行讲解。

系列文章：
[【Lua运行时热重载功能实现①】检测Lua文件发生变化](https://blog.csdn.net/j756915370/article/details/106039151)
[【Lua运行时热重载功能实现②】重载Lua模块、替换函数](https://blog.csdn.net/j756915370/article/details/106043421)
***
CSDN博客：[https://blog.csdn.net/j756915370](https://blog.csdn.net/j756915370)
知乎专栏：[https://zhuanlan.zhihu.com/c_1241442143220363264](https://zhuanlan.zhihu.com/c_1241442143220363264)
Q群：891809847