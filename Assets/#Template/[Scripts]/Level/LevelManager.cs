#if UNITY_EDITOR
using UnityEditor;
#endif
using DG.Tweening;
using DancingLineFanmade.Trigger;
using UnityEngine;
using DancingLineFanmade.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Playables;

namespace DancingLineFanmade.Level
{
    public delegate void OnPlayerRevive();

    public enum GameStatus
    {
        Waiting,
        Playing,
        Moving,
        Died,
        Completed
    }

    public enum Direction
    {
        First,
        Second
    }

    public static class LevelManager
    {
        private static GameStatus gameState = GameStatus.Waiting;
        public static GameStatus GameState
        {
            get => gameState;
            set => gameState = value;
        }

        public static bool getInput = true;
        public static bool Clicked
        {
            get
            {
                if (getInput) return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
                else return false;
            }
        }

        /// <summary>
        /// Shorthand for writing game default gravity(0f, -9.3f, 0f).
        /// </summary>
        public static Vector3 defaultGravity
        {
            get => new Vector3(0f, -9.3f, 0f);
        }

        public static Vector3 PlayerPosition
        {
            get => Player.Instance.transform.position;
            set => Player.Instance.transform.position = value;
        }

        public static Vector3 CameraPosition
        {
            get
            {
                if (CameraFollower.Instance) return CameraFollower.Instance.transform.position;
                else return Vector3.zero;
            }
            set
            {
                if (CameraFollower.Instance) CameraFollower.Instance.transform.position = value;
            }
        }

        private static GameObject dieCubes { get; set; }
        private static Tween trackFadeOut { get; set; }

        public static Vector3 Convert(this Vector3 vector3, bool positive = true)
        {
            int x = (int)Mathf.Abs(Mathf.Floor(vector3.x / 360));
            int y = (int)Mathf.Abs(Mathf.Floor(vector3.y / 360));
            int z = (int)Mathf.Abs(Mathf.Floor(vector3.z / 360));
            return positive ? new Vector3(vector3.x + 360 * x, vector3.y + 360 * y, vector3.z + 360 * z) : new Vector3(vector3.x - 360 * x, vector3.y - 360 * y, vector3.z - 360 * z);
        }

        public static void DialogBox(string title, string message, string ok, bool stopPlaying)
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog(title, message, ok)) if (stopPlaying) EditorApplication.isPlaying = false;
#endif
        }

        public static void PlayerDeath(Player player, DieReason reason, GameObject cubes = null, Collision collision = null, bool revive = false)
        {
            trackFadeOut = AudioManager.FadeOut(0f, 10f);
            if (CameraFollower.Instance) CameraFollower.Instance.KillAll();
            player.allowTurn = false;
            foreach (Animator a in player.playedAnimators) a.speed = 0f;
            foreach (PlayableDirector p in player.playedTimelines) p.Pause();
            foreach (PlayAnimator p in Object.FindObjectsOfType<PlayAnimator>(true)) foreach (SingleAnimator s in p.animators) if (!s.dontRevive) s.StopAnimator();
            player.Events?.Invoke(5);
            switch (reason)
            {
                case DieReason.Hit:
                    GameState = GameStatus.Died;
                    AudioManager.PlayClip(Resources.Load<AudioClip>("Audios/Hit"), 1f);
                    if (cubes != null)
                    {
                        dieCubes = Object.Instantiate(cubes, player.transform.position, player.transform.rotation);
                        dieCubes?.GetComponent<PlayerCubes>().Play(collision);
                    }

                    break;
                case DieReason.Drowned:
                    GameState = GameStatus.Moving;
                    AudioManager.PlayClip(Resources.Load<AudioClip>("Audios/Drowned"), 1f);
                    break;
                case DieReason.Border:
                    GameState = GameStatus.Moving;
                    break;
            }
            if (!revive) GameOverNormal(false); else GameOverRevive();
            Cursor.visible = true;
        }

        public static void GameOverNormal(bool complete)
        {
            float percentage = complete ? 1f : AudioManager.Progress;

            if (GameState == GameStatus.Died || GameState == GameStatus.Completed || GameState == GameStatus.Moving)
                LevelUI.Instance.NormalPage(percentage, Player.Instance.BlockCount, Player.Instance.CrownCount);
        }

        public static void GameOverRevive()
        {
            if (GameState == GameStatus.Died || GameState == GameStatus.Moving)
            {
                LevelUI.Instance.RevivePage(AudioManager.Progress);
            }
        }

        public static void InitPlayerPosition(Player player, Vector3 position, bool changeDirection, Direction direction = Direction.First)
        {
            PlayerPosition = position;
            CameraPosition = position;
            if (changeDirection)
            {
                switch (direction)
                {
                    case Direction.First: player.transform.eulerAngles = player.firstDirection; break;
                    case Direction.Second: player.transform.eulerAngles = player.secondDirection; break;
                }
            }
        }

        private static void RevivePlayer()
        {
            Debug.Log("Player revived!");
        }

        public static OnPlayerRevive revivePlayer = RevivePlayer;

        public static void DestroyRemain()
        {
            GameState = GameStatus.Waiting;
            trackFadeOut?.Kill();
            if (dieCubes) Object.Destroy(dieCubes);
        }

        public static void CompareCheckpointIndex(int index, UnityAction callback)
        {
            if (index > Player.Instance.Checkpoints.Count - 1) callback.Invoke();
        }

        public static bool CompareCheckpointIndex(int index)
        {
            if (index > Player.Instance.Checkpoints.Count - 1) return true; else return false;
        }

        public static void SetFPSLimit(int frame)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = frame;
        }

        public static bool IsPointedOnUI()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            int touchCount = Input.touchCount;
            if (touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId) || CheckRaycastObjects(touch.position)) return true;
                    else return false;
                }
                else return false;
            }
            else return false;
#else
            if (Clicked)
            {
                if (EventSystem.current.IsPointerOverGameObject() && CheckRaycastObjects(Input.mousePosition)) return true;
                else return false;
            }
            else return false;
#endif
        }

        private static bool CheckRaycastObjects(Vector3 position)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.pressPosition = new Vector2(position.x, position.y);
            data.position = new Vector2(position.x, position.y);

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            return results.Count > 0;
        }

        public static GameObject CreateTrigger(Vector3 position, Vector3 rotation, Vector3 scale, bool local, string name)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (local) obj.transform.localPosition = position; else obj.transform.position = position;
            if (local) obj.transform.localEulerAngles = rotation; else obj.transform.eulerAngles = rotation;
            obj.transform.localScale = scale;
            obj.GetComponent<BoxCollider>().isTrigger = true;
            obj.GetComponent<MeshRenderer>().enabled = false;
            obj.name = name;
            return obj;
        }

        public static Color GetColorByContent(Color color)
        {
            float brightness = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
            return brightness > 0.6f ? Color.black : Color.white;
        }

        public static void DrawDirection(Transform center, float length)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center.position, center.position + Vector3.right * length);
            Gizmos.DrawIcon(center.position + Vector3.right * length, "Directions/90");

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center.position, center.position + Vector3.left * length);
            Gizmos.DrawIcon(center.position + Vector3.left * length, "Directions/270");

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center.position, center.position + Vector3.forward * length);
            Gizmos.DrawIcon(center.position + Vector3.forward * length, "Directions/0");

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center.position, center.position + Vector3.back * length);
            Gizmos.DrawIcon(center.position + Vector3.back * length, "Directions/180");
        }
    }
}