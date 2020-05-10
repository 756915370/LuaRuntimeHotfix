using UnityEngine;

public class GameLaunch : MonoBehaviour
{
    private void Start()
    {
        XLuaManager.Instance.Startup();
        XLuaManager.Instance.StartGame();
        LuaFileWatcher.CreateLuaFileWatcher();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            XLuaManager.Instance.SafeDoString("GameMain.TestFunc()");
        }
    }

}
