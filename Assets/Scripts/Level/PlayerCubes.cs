using UnityEngine;

namespace DancingLineFanmade.Level
{
    [DisallowMultipleComponent]
    public class PlayerCubes : MonoBehaviour
    {
        private Transform[] cubes;

        internal void Play(Collision collision)
        {
            Transform[] componentsInChildren = GetComponentsInChildren<Transform>(true);
            cubes = new Transform[componentsInChildren.Length - 1];
            for (int i = 1; i < componentsInChildren.Length; i++) cubes[i - 1] = componentsInChildren[i];

            if (collision?.contacts.Length > 0)
            {
                for (int i = 0; i < cubes.Length; i++)
                {
                    cubes[i].gameObject.SetActive(true);
                    float num2 = Random.Range(0.6f, 1f);
                    cubes[i].transform.localScale = new Vector3(num2, num2, num2);
                    cubes[i].transform.rotation = Random.rotation;
                    Vector3 normalized = cubes[i].transform.rotation.eulerAngles.normalized;
                    cubes[i].gameObject.GetComponent<Rigidbody>().AddForce(normalized, ForceMode.Impulse);
                }
            }
        }
    }
}