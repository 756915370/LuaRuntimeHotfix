using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using XLua;

[Hotfix]
public class LuaUpdater : MonoBehaviour
{
    private Action<float> _luaUpdate = null;

    public void OnInit(LuaEnv luaEnv)
    {
        _luaUpdate = luaEnv.Global.Get<Action<float>>("Update");
    }

    void Update()
    {
        if (_luaUpdate != null)
        {
            _luaUpdate(Time.deltaTime);
        }
    }

    public void OnDispose()
    {
        _luaUpdate = null;
    }

    void OnDestroy()
    {
        OnDispose();
    }
}