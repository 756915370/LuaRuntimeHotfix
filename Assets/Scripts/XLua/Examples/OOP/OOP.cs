using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class OOP : MonoBehaviour
{
	private LuaEnv _luaEnv = null;

	// Use this for initialization
	void Start()
	{
		_luaEnv = new LuaEnv();
		_luaEnv.DoString("require 'TestClass'");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
