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
using System.Linq;

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
        public static GameStatus GameState { get; set; } = GameStatus.Waiting;

        public const bool getInput = true;

        public static bool Clicked
        {
            get
            {
                if (getInput)
                {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                    if (Input.touchCount <= 0)
                        return false;
                    var touch = Input.GetTouch(0);
                    return touch.phase == TouchPhase.Began;
#else
                    return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return);
#endif
                }
            }
        }

        /// <summary>
        /// Shorthand for writing game default gravity(0f, -9.3f, 0f).
        /// </summary>
        public static Vector3 defaultGravity => new(0f, -9.3f, 0f);

        /// <summary>
        /// Shorthand for writing linear curve(0f, 0f, 1f, 1f).
        /// </summary>
        public static AnimationCurve linearCurve => AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public static Vector3 PlayerPosition
        {
            set => Player.Instance.transform.position = value;
        }

        public static Vector3 CameraPosition
        {
            set
            {
                if (CameraFollower.Instance)
                    CameraFollower.Instance.transform.position = value;
            }
        }

        private static GameObject dieCubes { get; set; }
        private static Tween trackFadeOut { get; set; }

        public static Vector3 Convert(this Vector3 vector3, bool positive = true)
        {
            var x = (int)Mathf.Abs(Mathf.Floor(vector3.x / 360));
            var y = (int)Mathf.Abs(Mathf.Floor(vector3.y / 360));
            var z = (int)Mathf.Abs(Mathf.Floor(vector3.z / 360));
            return positive
                ? new Vector3(vector3.x + 360 * x, vector3.y + 360 * y, vector3.z + 360 * z)
                : new Vector3(vector3.x - 360 * x, vector3.y - 360 * y, vector3.z - 360 * z);
        }

        public static void DialogBox(string title, string message, string ok, bool stopPlaying)
        {
#if UNITY_EDITOR
            if (!EditorUtility.DisplayDialog(title, message, ok))
                return;
            if (stopPlaying)
                EditorApplication.isPlaying = false;
#endif
        }

        public static void PlayerDeath(Player player, DieReason reason, GameObject cubes = null,
            Collision collision = null, bool revive = false)
        {
            trackFadeOut = AudioManager.FadeOut(0f, 10f);
            if (CameraFollower.Instance)
                CameraFollower.Instance.KillAll();
            player.allowTurn = false;
            foreach (var a in player.playedAnimators)
            {
                a.speed = 0f;
            }

            foreach (var p in player.playedTimelines)
            {
                p.Pause();
            }

            foreach (var p in Object.FindObjectsOfType<PlayAnimator>(true))
            {
                foreach (var s in p.animators.Where(s => !s.dontRevive))
                {
                    s.StopAnimator();
                }
            }

            player.Events?.Invoke(5);
            switch (reason)
            {
                case DieReason.Hit:
                    GameState = GameStatus.Died;
                    AudioManager.PlayClip(Resources.Load<AudioClip>("Audios/Hit"), 1f);
                    if (cubes != null)
                        dieCubes = Object.Instantiate(cubes, player.transform.position, player.transform.rotation);
                    dieCubes?.GetComponent<PlayerCubes>().Play(collision);
                    break;
                case DieReason.Drowned:
                    GameState = GameStatus.Moving;
                    AudioManager.PlayClip(Resources.Load<AudioClip>("Audios/Drowned"), 1f);
                    break;
                case DieReason.Border:
                    GameState = GameStatus.Moving;
                    break;
            }

            if (!revive)
                GameOverNormal(false);
            else GameOverRevive();
            Cursor.visible = true;
        }

        public static void GameOverNormal(bool complete)
        {
            var percentage = complete ? 1f : AudioManager.Progress;
            if (GameState is GameStatus.Died or GameStatus.Completed or GameStatus.Moving)
                LevelUI.Instance.NormalPage(percentage, Player.Instance.BlockCount, Player.Instance.CrownCount);
        }

        public static void GameOverRevive()
        {
            if (GameState is GameStatus.Died or GameStatus.Moving)
                LevelUI.Instance.RevivePage(AudioManager.Progress);
        }

        public static void SetPlayerPosition(Player player, Vector3 position, bool changeDirection, Direction direction,
            bool setCameraPosition)
        {
            PlayerPosition = position;
            if (setCameraPosition)
                CameraPosition = position;
            if (changeDirection)
            {
                player.transform.eulerAngles = direction switch
                {
                    Direction.First => player.firstDirection,
                    Direction.Second => player.secondDirection,
                    _ => player.transform.eulerAngles
                };
            }
        }

        private static void RevivePlayer()
        {
            Debug.Log("玩家已复活。");
        }

        public static OnPlayerRevive revivePlayer = RevivePlayer;

        public static void DestroyRemain()
        {
            GameState = GameStatus.Waiting;
            trackFadeOut?.Kill();
            if (dieCubes)
                Object.Destroy(dieCubes);
        }

        public static void CompareCheckpointIndex(int index, UnityAction callback)
        {
            if (index > Player.Instance.Checkpoints.Count - 1)
                callback.Invoke();
        }

        public static bool IsPointedOnUI()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            var touchCount = Input.touchCount;
            if (touchCount != 1) 
                return false;
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                return EventSystem.current.IsPointerOverGameObject(touch.fingerId) || CheckRaycastObjects(touch.position);
            return false;
#else
            if (Clicked)
                return EventSystem.current.IsPointerOverGameObject() && CheckRaycastObjects(Input.mousePosition);
            return false;
#endif
        }

        private static bool CheckRaycastObjects(Vector3 position)
        {
            var data = new PointerEventData(EventSystem.current)
            {
                pressPosition = new Vector2(position.x, position.y),
                position = new Vector2(position.x, position.y)
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            return results.Count > 0;
        }

        public static GameObject CreateTrigger(Vector3 position, Vector3 rotation, Vector3 scale, bool local,
            string name)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (local)
                obj.transform.localPosition = position;
            else obj.transform.position = position;
            if (local)
                obj.transform.localEulerAngles = rotation;
            else obj.transform.eulerAngles = rotation;
            obj.transform.localScale = scale;
            obj.GetComponent<BoxCollider>().isTrigger = true;
            obj.GetComponent<MeshRenderer>().enabled = false;
            obj.name = name;
            return obj;
        }

        public static Color GetColorByContent(Color color)
        {
            var brightness = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
            return brightness > 0.6f ? Color.black : Color.white;
        }

        public static GUIStyle GUIStyle(Color background, Color text, int size)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, background);
            texture.Apply();

            var style = new GUIStyle()
            {
                normal =
                {
                    textColor = text,
                    background = texture
                },
                fontSize = size
            };
            return style;
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

        public static void DrawDirection(Transform center, float length, float thickness)
        {
#if UNITY_EDITOR
            var style = GUIStyle(new Color(0f, 0f, 0f, 0.6f), Color.white, 20);

            Handles.color = Color.red;
            Handles.DrawLine(center.position, center.position + Vector3.forward * length, thickness);
            Handles.Label(center.position + Vector3.forward * length, "0", style);

            Handles.color = Color.blue;
            Handles.DrawLine(center.position, center.position + Vector3.right * length, thickness);
            Handles.Label(center.position + Vector3.right * length, "90", style);

            Handles.color = Color.yellow;
            Handles.DrawLine(center.position, center.position + Vector3.back * length, thickness);
            Handles.Label(center.position + Vector3.back * length, "180", style);

            Handles.color = Color.green;
            Handles.DrawLine(center.position, center.position + Vector3.left * length, thickness);
            Handles.Label(center.position + Vector3.left * length, "270", style);
#endif
        }
    }
}