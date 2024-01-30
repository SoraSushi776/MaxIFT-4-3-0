using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent]
    public class FallPredictor : MonoBehaviour
    {
        [SerializeField] private int speed = 12;
        [SerializeField] private float width = 0.2f;
        [SerializeField] private int count = 80;

        private LineRenderer lineRenderer;
        private float x;
        private float y;

        private void OnEnable()
        {
            lineRenderer.positionCount = 0;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0;
            lineRenderer.endWidth = 0;
            lineRenderer.startColor = Color.clear;
            lineRenderer.endColor = Color.clear;
            lineRenderer.enabled = false;
        }

#if UNITY_EDITOR
        private void Draw()
        {
            x = 0;
            y = 0;

            lineRenderer = GetComponent<LineRenderer>() ? GetComponent<LineRenderer>() : gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
            lineRenderer.useWorldSpace = false;

            Vector3[] points = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                points[i] = new Vector3(x, y, 0);
                x += 1;
                y = -(0.5f * Physics.gravity.magnitude * (x / speed) * (x / speed));
            }

            lineRenderer.positionCount = count;
            lineRenderer.SetPositions(points);
        }

        private void OnValidate()
        {
            if (count >= 0) Draw();
            else
            {
                count = 0;
                lineRenderer.positionCount = 0;
            }
        }
#endif
    }
}