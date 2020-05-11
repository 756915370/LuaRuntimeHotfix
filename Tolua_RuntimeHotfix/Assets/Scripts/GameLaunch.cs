using UnityEngine;

public class GameLaunch : MonoBehaviour
{
    private void Start()
    {
        LuaManager.Instance.Startup();
        LuaManager.Instance.StartGame();
        LuaFileWatcher.CreateLuaFileWatcher(LuaManager.Instance.LuaState);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            LuaManager.Instance.SafeDoString("GameMain.TestFunc()");
        }
    }

}
