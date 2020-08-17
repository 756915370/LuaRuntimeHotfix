上篇说到**检测Lua文件发生变化**，这篇来讲怎么重载lua模块。
请配合[github工程](https://github.com/756915370/LuaRuntimeHotfix)来看。关于重载lua的功能在**Hotfix.lua**脚本上。这个脚本有三个函数，**hotfix**、**update_table**、**update_func**。
***
#### hotfix
```csharp
function hotfix(filename)
    print("start hotfix: ",filename)
    local oldModule
    if package.loaded[filename] then
        oldModule = package.loaded[filename]
        package.loaded[filename] = nil
    else
        print('this file nevev loaded: ',filename)
        return
    end
    local ok,err = pcall(require, filename)
    if not ok then
        package.loaded[filename] = oldModule
        print('reload lua file failed.',err)
        return
    end

    local newModule = package.loaded[filename]
    
    update_table(newModule, oldModule)

    if oldModule.OnReload ~= nil then
        oldModule:OnReload()
    end
    print('replaced succeed')
    package.loaded[filename] = oldModule
end
```
#### package.loaded
**package.loaded**是lua内部用于记录哪些模块被**require**的表，如果一个模块已经被**require**，那么下次再调用**require**就不会生效。
所以第一步就是把**package.loaded记录的旧模块清空并重新require**，然后使用**pcall**捕捉异常，如果出现错误，**package.loaded**仍然赋值旧模块，并返回。~~网上的很多博客也就停留在这一步了。)~~ 下一步就是**更新旧模块**，调用**update_table**，最后把**package.loaded[filename]** 指向旧模块。
***
#### update_table

```csharp
function update_table(new_table, old_table, updated_tables)
    assert("table" == type(new_table))
    assert("table" == type(old_table))
    
    -- Compare 2 tables, and update old table.
    for key, value in pairs(new_table) do
        local old_value = old_table[key]
        local type_value = type(value)
        if type_value == "function" then
            update_func(value, old_value)
            old_table[key] = value
        elseif type_value == "table" then
            if ( updated_tables[value] == nil ) then
                updated_tables[value] = true
                update_table(value, old_value,updated_tables)
            end
        end
    end 

    -- Update metatable.
    local old_meta = debug.getmetatable(old_table)
    local new_meta = debug.getmetatable(new_table)
    if type(old_meta) == "table" and type(new_meta) == "table" then
        update_table(new_meta, old_meta,updated_tables)
    end
end
```
这个函数用于更新旧表，首先断言这两个参数的类型是不是table。
**需要考虑到表的循环引用问题否则会出现无限递归，所以额外传入一个表用来记录哪些表已经更新了，如果更新了记录下来，下次执行代码时不会再次递归。**
然后遍历表里面的元素，有三种情况：**数据**、**函数**、**表**。

- **数据不做处理**，让旧表保留旧的数据。
- **函数，需要处理upvalue值**，处理完之后，**替换旧表里的旧函数**。
- **表**，嵌套调用**update_table**

最后处理元表，把旧表的元表和新表的元表拿出来，嵌套调用**update_table**。
##### 为什么需要处理元表?
考虑下面的代码，这是我工程里**BaseClass.lua**的一段代码:
```csharp
    local vtbl = {}
    _class[class_type] = vtbl

    setmetatable(class_type, {
        __newindex = function(t,k,v)
            vtbl[k] = v
        end
    ,
        --For call parent method
        __index = vtbl,
    })
```
当我对class_type表里添加元素时，实际上是往vtbl也就是class_type的元表进行操作。这样当我遍历class_type的时候，实际上是找不到我添加的元素的，因为这个元素在元表里。所以**在更新表的时候，需要把新表和旧表的元表都取出来，然后再次调用update_table**。
***
#### update_func
```csharp
function update_func(new_func, old_func)
    assert("function" == type(new_func))
    assert("function" == type(old_func))

    -- Get upvalues of old function.
    local old_upvalue_map = {}
    for i = 1, math.huge do
        local name, value = debug.getupvalue(old_func, i)
        if not name then break end
        old_upvalue_map[name] = value
    end

    -- Update new upvalues with old.
    for i = 1, math.huge do
        local name, value = debug.getupvalue(new_func, i)
        if not name then break end
        print('set up value: name:',name)
        local old_value = old_upvalue_map[name]
        if old_value then
            debug.setupvalue(new_func, i, old_value)
        end  
    end 
end
```
这个函数用于处理**upvalue**，什么是**upvalue**，就是**一个函数使用了不在这个函数内的变量**。如下代码:

```csharp
local count = 1
PlayerMove.TestFunc = function()
    count = count + 1
    print("count:",count)
end
```
其中的**count**就是**upvalue**，当我更新函数的时候需要保留**upvalue**的值。
#### getupvalue、setupvalue
lua内部提供了这两个方法，我们只需要把旧函数的upvalue拿出来再赋值给新函数的upvalue就可以了。
***
到这里，看似热重载的功能已经完成了，但是还有一种情况没有考虑到，就是**如果旧表的旧函数在其他地方已经被存了一份该怎么办?** 这个时候就算你把旧表的旧函数更新为新函数了，实际上其他地方存的还是旧函数。
比如我工程里**GameMain.lua**的一段代码:

```csharp
local function Start()
    print("GameMain start...")
    playerMove = PlayerMove.New()
    playerMove:Start()
    GameMain.playerFunc = playerMove.TestFunc
end

local function TestFunc()
    print("call from self...")
    GameMain.playerFunc()
    print("call from playermove...")
    playerMove.TestFunc()
end
```
当我更新旧表的函数后，**playerMove.TestFunc()** 的确是调用的新函数，但是**GameMain.playerFunc()** 还是旧函数。我的解决办法是在**PlayerMove.lua增加OnReload**这个函数，在这个函数对**GameMain.playerFunc**重新赋值，然后在**hotfix**方法里这么调用:

```csharp
    if oldModule.OnReload ~= nil then
        oldModule:OnReload()
    end
```
**OnReload**代码如下：

```csharp
local function OnReload(self)
    print('call onReload from: ',self.__cname)
    if self.__ctype == ClassType.class then
        print("this is a class not a instance")
        for k,v in pairs(self.instances) do
            print("call instance reload: ",k)
            if v.OnReload ~= nil then
                v:OnReload()
            end
        end
    else
        if self.__ctype == ClassType.instance then
            print("this is a instance")
            GameMain.playerFunc = self.TestFunc
        end
    end
end
```
##### 为什么需要分别考虑类和实例的情况？
我们都知道在lua里写面向对象，当我调用 **[class].New()** 时，实际上是生成了一个元表是[class]的实例。调用**package.load[filename]** 获得的是**类的类型，并不是类的实例**。那么我又需要调用类的实例的方法的时候怎么办？我的解决方法是类在生成一个实例时，**用一个表去记录它都生成了哪些实例**。同时**设置成弱引用**，这样当实例销毁并执行gc后，这个记录表也会自动取消对实例的引用。
在工程里的**BaseClass.lua**里面如下:
```csharp
    class_type.instances = {}
    setmetatable(class_type.instances, { __mode = "v" })
    class_type.New = function(...)
    	local obj = {}
		......
        table.insert(class_type.instances,obj)
        return obj
    end
```
- 其中**class_type.instance**用于记录生成的实例
- **__mode = "v"**，说明表里的value是弱引用
- **table.insert** 用于往表插入元素

这就是为什么**OnReload**里面判断了一下是**class**还是**instance**的原因。