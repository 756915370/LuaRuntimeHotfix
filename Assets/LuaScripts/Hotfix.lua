--package.cpath = package.cpath .. ';C:/Users/lenovo/.Rider2018.3/config/plugins/intellij-emmylua/classes/debugger/emmy/windows/x64/?.dll'
--local dbg = require('emmy_core')
--dbg.tcpConnect('localhost', 9966)

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

function update_table(new_table, old_table)
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
            update_table(value, old_value)
        end
    end 

    -- Update metatable.
    local old_meta = debug.getmetatable(old_table)
    local new_meta = debug.getmetatable(new_table)
    if type(old_meta) == "table" and type(new_meta) == "table" then
        update_table(new_meta, old_meta)
    end
end