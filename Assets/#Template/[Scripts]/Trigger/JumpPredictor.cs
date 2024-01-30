using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    public enum LineDirection
    {
        Left,
        Right
    }

    [DisallowMultipleComponent]
    public class JumpPredictor : MonoBehaviour
    {
        [SerializeField] private int speedX = 12;
        [SerializeField] private float width = 0.2f;
        [SerializeField] private int count = 80;
        [SerializeField] private LineDirection direction = LineDirection.Right;
        [SerializeField] private bool reverse = false;

        private LineRenderer lineRenderer;
        private float x;
        private float y;
        private float angle;
        private float speedY;
        private float speed;

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
        internal void Draw()
        {
            x = 0;
            y = 0;

            lineRenderer = GetComponent<LineRenderer>() ? GetComponent<LineRenderer>() : gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;

            Vector3[] points = new Vector3[count];

            speedY = GetComponent<Jump>().power / 50.5f;
            angle = Mathf.Atan(speedY / speedX);
            Vector2 vec = new Vector2(speedX, speedY);
            speed = vec.magnitude;

            for (int i = 0; i < count; i++)
            {
                switch (direction)
                {
                    case LineDirection.Left:
                        if (!reverse) points[i] = new Vector3(0, y, x) + transform.position;
                        else points[i] = new Vector3(0, y, -x) + transform.position;
                        break;
                    case LineDirection.Right:
                        if (!reverse) points[i] = new Vector3(x, y, 0) + transform.position;
                        else points[i] = new Vector3(-x, y, 0) + transform.position;
                        break;
                }
                x += 1;
                y = (x * Mathf.Tan(angle)) - (Physics.gravity.magnitude * x * x / (2 * (speed * Mathf.Cos(angle)) * (speed * Mathf.Cos(angle))));
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