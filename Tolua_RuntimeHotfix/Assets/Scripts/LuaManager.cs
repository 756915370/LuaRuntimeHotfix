using System;
using System.IO;
using LuaInterface;
using UnityEngine;



public class LuaManager : MonoSingleton<LuaManager>
{
    private const string gameMainScriptName = "GameMain";
    private const string hotfixScriptName = "Hotfix";
    
    private LuaState _luaState;

    private LuaFunction _luaUpdate = null;
    
    public LuaState LuaState
    {
        get { return _luaState; }
    }

    protected override void Init()
    {
        base.Init();
        _luaState = new LuaState();
        _luaState.Start();
        LuaBinder.Bind(_luaState);
        LoadScript(hotfixScriptName);
    }

    public void StartGame()
    {
        LoadScript(gameMainScriptName);
        SafeDoString("GameMain.Start()");
        _luaUpdate = LuaState.GetFunction("Update");
    }

    private void LoadScript(string scriptName)
    {
        SafeDoString(string.Format("require('{0}')",scriptName));
    }
    
    public void SafeDoString(string scriptContent)
    {
        if (_luaState != null)
        {
            try
            {
                _luaState.DoString(scriptContent);
            }
            catch (System.Exception ex)
            {
                string msg = string.Format("Lua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                Debug.LogError(msg, null);
            }
        }
    }

    private void Update()
    {
        if (_luaUpdate != null)
        {
            _luaUpdate.Call(Time.deltaTime);
        }
    }

    public override void Dispose()
    {
        if (_luaState != null)
        {
            _luaState.Dispose();
            _luaState = null;
        }
    }
}
