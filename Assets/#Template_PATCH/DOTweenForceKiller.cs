using UnityEngine.SceneManagement;
using UnityEngine;
using DG.Tweening;

namespace DancingLineFanmade.PATCH
{
    public static class DOTweenForceKiller
    {
        static DOTweenForceKiller()
        {
            RegisterEvents();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterEvents()
        {
            SceneManager.sceneUnloaded -= OnSceneUnLoad;
            SceneManager.sceneUnloaded += OnSceneUnLoad;
            
            Application.quitting -= OnAppQuit;
            Application.quitting += OnAppQuit;
        }

        private static void OnSceneUnLoad(Scene _)
        {
            ExecuteCleanup();
        }

        private static void OnAppQuit()
        {
            ExecuteCleanup();
        }

        private static void ExecuteCleanup()
        {
            
            if (DOTween.instance == null)
            {
                return;
            }
            int killedCount = DOTween.KillAll();
            DOTween.Clear(true);
            Debug.Log($"<color=#FF0000>[DOTween Force Kill]</color> Killed {killedCount} Tween(s).");
        }
    }
}