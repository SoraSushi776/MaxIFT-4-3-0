using System.Linq;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using DancingLineFanmade.Trigger;

namespace DancingLineFanmade.Level
{
    public enum GravityAccessMode
    {
        FromPlayerSettings,
        FromActivedLevelData,
        Custom
    };

    [DisallowMultipleComponent]
    public class TrailPredictor : MonoBehaviour
    {
        //[InfoBox("如果脚本出现预测轨迹和实际轨迹对不上的情况，您可以尝试开始游戏再结束游戏")]
        [Header("Settings")] 
        [SerializeField] private GravityAccessMode gravityAccessMode = GravityAccessMode.FromActivedLevelData;
        [SerializeField, ShowIf("gravityAccessMode", GravityAccessMode.Custom)] private Vector3 customGravity = Physics.gravity;
        
        [Space, SerializeField, Min(2)] private int resolution = 100;
        [SerializeField, Min(0.1f)] private float renderDistance = 50f;

        [SerializeField] private Color trailColor = Color.red;
        [SerializeField] private Color hitColor = Color.blue;

        [Header("Data")] 
        [Tooltip("水平移动速度 (m/s)")] [Min(0.01f)] public float horizontalSpeed = 12f;
        [InfoBox("如果物体上有 Jump 组件，则会自动使用 Jump 的 power 计算垂直冲量")]
        [Tooltip("向上跳跃的瞬时冲量"), DisableIf("@this.GetComponent<Jump>() != null")] [Min(0f)] public float verticalImpulse;

        private readonly LayerMask layerMask = Physics.DefaultRaycastLayers;
        private const QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        private struct HitInfo
        {
            public Vector3 start;
            public Vector3 end;
            public Vector3 point;
            public Vector3 normal;
            public bool isFirstHit;
        }

        private HitInfo[] hitInfos;
        private HitInfo[] savedHitInfos;
        private bool hasFirstHit;
        private bool isPlaying;

        private Player _editModeGetPlayer
        {
            get
            {
                if (Player.Instance != null) return Player.Instance;
                return FindObjectOfType<Player>();
            }
        }
        private Rigidbody _editModeGetRigidbody
        {
            get
            { 
            if (_editModeGetPlayer == null) return null;
                return _editModeGetPlayer.GetComponent<Rigidbody>(); 
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => resolution = Mathf.Max(2, resolution);

        private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                isPlaying = true;
                savedHitInfos = hitInfos?.ToArray();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                isPlaying = false;
                savedHitInfos = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (!isPlaying) CalculateTrail();
            DrawTrail();
        }

        private void CalculateTrail()
        {
            hitInfos = new HitInfo[resolution - 1];
            hasFirstHit = false;

            // 1. 获取物理参数
            Vector3 gravityVec = GetGravity();
            float g = gravityVec.magnitude;
            float mass = (_editModeGetRigidbody != null) ? _editModeGetRigidbody.mass : 1f;

            float jumpPower = GetComponent<Jump>()?.power ?? verticalImpulse;
            // 2. 计算初始垂直速度 (v = p / m)
            float v0y = jumpPower / mass;

            // 3. 计算总时间：基于水平距离和速度 t = d / v
            float totalTime = renderDistance / Mathf.Max(0.1f, horizontalSpeed);

            // 4. 获取方向（忽略缩放）
            // 注意：这里假设 transform.right 是你的前进方向，如果不对请改为 transform.forward
            Vector3 startPos = transform.position;
            Vector3 moveDir = transform.right; 
            Vector3 upDir = -gravityVec.normalized; // 向上方向由重力的反方向决定

            for (var i = 0; i < resolution - 1; i++)
            {
                float t1 = (float)i / (resolution - 1) * totalTime;
                float t2 = (float)(i + 1) / (resolution - 1) * totalTime;

                Vector3 GetPosAtTime(float t)
                {
                    // 水平位移：s = v * t
                    float x = horizontalSpeed * t;
                    // 垂直位移：s = v0*t - 0.5 * g * t^2
                    float y = (v0y * t) - (0.5f * g * t * t);
                    return startPos + (moveDir * x) + (upDir * y);
                }

                Vector3 p1 = GetPosAtTime(t1);
                Vector3 p2 = GetPosAtTime(t2);

                if (hasFirstHit)
                {
                    hitInfos[i] = new HitInfo { start = p1, end = p2, isFirstHit = false };
                    continue;
                }

                if (Physics.Linecast(p1, p2, out var hit, layerMask, queryTriggerInteraction))
                {
                    hasFirstHit = true;
                    hitInfos[i] = new HitInfo { start = p1, end = hit.point, point = hit.point, normal = hit.normal, isFirstHit = true };
                }
                else
                {
                    hitInfos[i] = new HitInfo { start = p1, end = p2, isFirstHit = false };
                }
            }
        }

        private void DrawTrail()
        {
            var infos = isPlaying ? savedHitInfos : hitInfos;
            if (infos == null) return;

            for (var i = 0; i < infos.Length; i++)
            {
                var hit = infos[i];
                Gizmos.color = trailColor;
                Gizmos.DrawLine(hit.start, hit.end);

                if (hit.isFirstHit)
                {
                    Gizmos.color = hitColor;
                    Gizmos.DrawSphere(hit.point, 0.15f);
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal * 1.5f);
                    break; // 击中后停止绘制后续段
                }
            }
        }

        private Vector3 GetGravity()
        {
            switch (gravityAccessMode)
            {
                case GravityAccessMode.FromPlayerSettings: return Physics.gravity;
                case GravityAccessMode.FromActivedLevelData:
                    var p = _editModeGetPlayer;
                    return (p != null && p.levelData != null) ? p.levelData.gravity : Physics.gravity;
                case GravityAccessMode.Custom: return customGravity;
                default: return Physics.gravity;
            }
        }
#endif
    }
}