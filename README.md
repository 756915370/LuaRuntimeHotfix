# LuaRuntimeHotfix
This project demonstrates the unity editor's ability to **make lua changes take effect immediately without replay the game.** 

这个工程展示了**unity编辑器下不需要重启游戏就能让lua文件改动后立刻生效**的功能。  
目前实现的功能:
- **保留旧模块的数据，替换旧模块的函数**
- **其它模块缓存了旧模块的函数的处理**
- **upvalue值的处理**
- **需要更新的模块的元表的处理** 
- **处理了表循环引用导致无限递归的情况**
***
使用的unity版本是2019.3.0，**根目录下是xlua版本的工程** ，[**/Tolua_RuntimeHotfix**](https://github.com/756915370/LuaRuntimeHotfix/tree/master/Tolua_RuntimeHotfix)是tolua版本的工程。  
- **package.load[filename] 获取已加载的模块失败怎么办？**   
请检查是不是**filename**这个变量传入有误，**FileSystemEventArgs.FullPath**是文件的完整路径，如 **"F:\Git\LuaRuntimeHotfix\Assets\LuaScripts\NewDirectory1\Test.lua"**,需要转化成 **"NewDirectory1.Test"** 才能正确的调用**package.load**。  
关于文件路径转化的代码在**LuaFileWatcher.cs**，如果有需要请自行修改。
***
打开工程里的场景SampleScene，里面是一个方块，功能很简单，按上下可以移动方块。其中逻辑是写在lua里面的。
- **如何验证热重载功能？**   
改动PlayerMove.lua的update函数，比如把10改成-10，会发现方块会倒着走了。

```csharp
local function Update(self,deltaTime)
    local verticalInput = CS.UnityEngine.Input.GetAxis("Vertical")
    local movement = self.transform.forward * verticalInput * 10 * deltaTime
    self.rigidbody:MovePosition(self.rigidbody.position + movement)
end
```

![在这里插入图片描述](https://img-blog.csdnimg.cn/20200510221922378.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L2o3NTY5MTUzNzA=,size_16,color_FFFFFF,t_70)
***
工程里各个脚本的作用:
![在这里插入图片描述](https://img-blog.csdnimg.cn/20200510221229784.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L2o3NTY5MTUzNzA=,size_16,color_FFFFFF,t_70) 
#### 左边是C#文件:
- **LuaFileWatcher**，检测lua文件发生变化，当发生变化时调用Hotfix.lua进行重载模块的操作。
- **GameLaunch**，启动LuaFileWatcher，启动XLuaManager
- **XLuaManager**，启动lua端的GameMain模块，并且获取lua端的update函数，并在自己的update函数里调用lua端的update   

#### 右边是Lua文件:
- **GameMain**，新建一个PlayerMove的实例，调用PlayerMove里面的update
- **PlayerMove**，继承自BaseClass，控制方块移动
- **BaseClass**，面向对象编程里的基类
- **Hotfix**，重载lua模块
***
#### 关于代码的讲解:  
 - [【Lua运行时热更新①】检测Lua文件发生变化](https://zhuanlan.zhihu.com/p/139548726)  
 - [【Lua运行时热更新②】重载Lua模块、替换函数](https://zhuanlan.zhihu.com/p/139549412)  

CSDN博客：[https://blog.csdn.net/j756915370](https://blog.csdn.net/j756915370)  
知乎专栏：[https://zhuanlan.zhihu.com/c_1241442143220363264](https://zhuanlan.zhihu.com/c_1241442143220363264)  
Q群：891809847
