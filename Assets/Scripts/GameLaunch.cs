using UnityEngine;

public class GameLaunch : MonoBehaviour
{
    private void Start()
    {
        XLuaManager.Instance.Startup();
        XLuaManager.Instance.StartGame();
        LuaFileWatcher.CreateLuaFileWatcher(XLuaManager.Instance.luaEnv);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            XLuaManager.Instance.SafeDoString("GameMain.TestFunc()");
        }
    }

}
