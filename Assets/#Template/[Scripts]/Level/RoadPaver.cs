using UnityEngine;

namespace DancingLineFanmade.Level
{
    [DisallowMultipleComponent]
    public class RoadPaver : MonoBehaviour
    {
        private Player player;
        private Transform playerTransform;

        [SerializeField] private Transform roadObject;
        [SerializeField] private float roadWidth = 2f;
        [SerializeField] private float roadHeight = 1f;

        private Transform roadHolder;
        private Transform road;
        private int roadIndex = 0;

        private void Start()
        {
            player = Player.Instance;
            playerTransform = player.transform;
            roadHolder = new GameObject("RoadHolder").transform;
            roadObject.localScale = new Vector3(roadWidth, roadHeight, roadWidth);
            roadObject.position = playerTransform.transform.position - new Vector3(0f, 0.5f * (roadHeight + 1f), 0f);
            road = Instantiate(roadObject, playerTransform.transform.position - new Vector3(0f, 0.5f * (roadHeight + 1f), 0f), playerTransform.rotation);
            road.name = "Road " + roadIndex;
            roadIndex++;
            road.parent = roadHolder;

            player.OnTurn.AddListener(() =>
            {
                road = Instantiate(roadObject, playerTransform.transform.position - new Vector3(0f, 0.5f * (roadHeight + 1f), 0f), playerTransform.rotation);
                road.name = "Road " + roadIndex;
                roadIndex++;
                road.parent = roadHolder;
            });
        }

        private void Update()
        {
            if (LevelManager.GameState == GameStatus.Playing)
            {
                if (road)
                {
                    road.transform.localScale = new Vector3(roadWidth, roadHeight, road.localScale.z + player.Speed * Time.deltaTime);
                    road.transform.Translate(Vector3.forward * 0.5f * player.Speed * Time.deltaTime, Space.Self);
                }
            }
        }
    }
}