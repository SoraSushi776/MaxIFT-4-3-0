using UnityEngine;

namespace DancingLineFanmade.Level
{
    [DisallowMultipleComponent]
    public class TimeScale : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private KeyCode key = KeyCode.T;
        [SerializeField, Range(0f, 3f)] private float enabledValue = 1.25f;
        [SerializeField, Range(0f, 3f)] private float disabledValue = 1f;

        private new bool enabled = false;

        private void Update()
        {
            if (LevelManager.GameState == GameStatus.Playing)
            {
                if (!enabled)
                {
                    if (Input.GetKeyDown(key))
                    {
                        AudioManager.Pitch = enabledValue;
                        Time.timeScale = enabledValue;
                        enabled = true;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(key))
                    {
                        AudioManager.Pitch = disabledValue;
                        Time.timeScale = disabledValue;
                        enabled = false;
                    }
                }
            }
        }
#endif
    }
}