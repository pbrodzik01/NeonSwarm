using UnityEngine;

namespace NeonSwarm
{
    public class AppLifecycle : MonoBehaviour
    {
        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveSystem.Save();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && !Application.isEditor)
            {
                SaveSystem.Save();
            }
        }

        private void OnApplicationQuit()
        {
            SaveSystem.Save();
        }
    }
}
