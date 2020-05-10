using System;
using UnityEngine;
using XLua;

namespace XLuaTest
{
    public class CoroutineTest : MonoBehaviour
    {
        LuaEnv luaenv = null;
        Action luaLateUpdate = null;

        // Use this for initialization
        void Start()
        {
            luaenv = new LuaEnv();
            luaenv.DoString("require 'coruntine_test'");
            luaLateUpdate = luaenv.Global.Get<Action>( "lateUpdate" );
        }

        // Update is called once per frame
        void Update()
        {
            if (luaenv != null)
            {
                luaenv.Tick();
            }
        }

        void LateUpdate()
        {
        }

        void OnDestroy()
        {
            luaenv.Dispose();
        }
    }
}
