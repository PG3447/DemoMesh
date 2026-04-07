using UnityEngine;
using System.Collections;

public class SphereSpawner : MonoBehaviour
{
    [SerializeField]
    private int count = 10;

    [SerializeField]
    private float spawnRange = 20f;

    [SerializeField]
    private float delay = 2f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnSpheres();
            yield return new WaitForSeconds(delay);
        }
    }

    void SpawnSpheres()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-spawnRange, spawnRange),
                Random.Range(1f, spawnRange),
                Random.Range(-spawnRange, spawnRange)
            );

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pos;

            Renderer renderer = sphere.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = Random.ColorHSV();

            sphere.AddComponent<Rigidbody>();
        }
    }
}