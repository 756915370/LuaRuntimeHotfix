# LuaRuntimeHotfix
This project demonstrates the unity editor's ability to **make lua changes take effect immediately without replay the game.** 

这个工程展示了**unity编辑器下不需要重启游戏就能让lua文件改动后立刻生效**的功能。

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
左边是C#文件:
- **LuaFileWatcher**，检测lua文件发生变化，当发生变化时调用Hotfix.lua进行重载模块的操作。
- **GameLaunch**，启动LuaFileWatcher，启动XLuaManager
- **XLuaManager**，启动lua端的GameMain模块，并且获取lua端的update函数，并在自己的update函数里调用lua端的update   

右边是Lua文件:
- **GameMain**，新建一个PlayerMove的实例，调用PlayerMove里面的update
- **PlayerMove**，继承自BaseClass，控制方块移动
- **BaseClass**，面向对象编程里的基类
- **Hotfix**，重载lua模块