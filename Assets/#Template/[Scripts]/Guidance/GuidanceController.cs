using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DancingLineFanmade.Guidance
{
    [DisallowMultipleComponent]
    public class GuidanceController : MonoBehaviour
    {
        public static GuidanceController Instance { get; private set; }

        private Player player;
        private Transform playerTransform;

        [Title("Creating")]
        [SerializeField] private bool createBoxes = false;
        [SerializeField] private bool createLines = true;

        [Title("Settings")]
        [SerializeField] internal Transform boxHolder;
        [SerializeField] private Color guidanceBoxColor = Color.white;
        [SerializeField, MinValue(0f)] private float lineGap = 0.2f;

        private GameObject boxPrefab;
        private GameObject linePrefab;
        private Transform holder;
        private int id = 0;
        private List<GuidanceBox> boxes = new List<GuidanceBox>();
        private bool started = false;
        private float forward;

        private void Awake()
        {
            Instance = this;

            id = 0;
            boxPrefab = Resources.Load<GameObject>("Prefabs/GuidanceBox");
            linePrefab = Resources.Load<GameObject>("Prefabs/GuidanceLine");
            if (createBoxes) holder = new GameObject("GuidanceBoxHolder").transform;
            if (boxHolder) boxes = boxHolder.GetComponentsInChildren<GuidanceBox>().ToList();
            foreach (GuidanceBox b in boxes) b.SetColor(guidanceBoxColor);
            if (createLines) GenerateLines();
        }

        private void Start()
        {
            player = Player.Instance;
            playerTransform = player.transform;

            if (createBoxes)
            {
                GameObject box = Instantiate(boxPrefab, playerTransform.position - new Vector3(0f, 0.45f, 0f), Quaternion.Euler(90, player.firstDirection.y, 0));
                box.transform.parent = holder;
                box.name = "OriginalGuidanceBox";
                box.GetComponent<GuidanceBox>().canBeTriggered = false;
            }
        }

        private void Update()
        {
            forward = playerTransform.eulerAngles.y == player.firstDirection.y ? player.secondDirection.y : player.firstDirection.y;
            if (createBoxes && LevelManager.GameState == GameStatus.Playing && !started)
            {
                player.OnTurn.AddListener(() =>
                {
                    GameObject box = Instantiate(boxPrefab, player.transform.position - new Vector3(0f, 0.45f, 0f), Quaternion.Euler(90, forward, 0));
                    box.transform.parent = holder;
                    box.name = "GuidanceBox " + id;
                    id++;
                });
                started = true;
            }
        }

        private void GenerateLines()
        {
            for (int a = 0; a < boxes.Count; a++)
            {
                Transform line;
                if (a + 1 < boxes.Count && boxes[a].haveLine)
                {
                    line = Instantiate(linePrefab, 0.5f * (boxes[a].transform.position + boxes[a + 1].transform.position), Quaternion.Euler(Vector3.zero)).transform;
                    line.GetComponent<SpriteRenderer>().color = guidanceBoxColor;
                    line.localScale = new Vector3(0.15f, (boxes[a + 1].transform.position - boxes[a].transform.position).magnitude - 0.5f * boxPrefab.transform.localScale.y - 2 * lineGap, 0.15f);
                    line.parent = boxes[a].transform;
                    line.localEulerAngles = Vector3.zero;
                    line.name = line.parent.name + " - Line";
                    if (line.transform.localScale.y <= 0f) Destroy(line.gameObject);
                }
            }
        }
    }
}