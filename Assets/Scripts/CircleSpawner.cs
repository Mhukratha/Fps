using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    public GameObject prefab;          // Prefab ที่จะ spawn
    public float radius = 5f;          // รัศมีวงกลม
    public int numberToSpawn = 10;     // จำนวน Prefab ที่จะเกิด

    void Start()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Vector3 spawnPos = GetRandomPointOnCircle();
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomPointOnCircle()
    {
        Vector2 randomPos = Random.insideUnitCircle * radius; // สุ่มจุดภายในวงกลม
        return new Vector3(randomPos.x, 0f, randomPos.y) + transform.position;
    }

}
