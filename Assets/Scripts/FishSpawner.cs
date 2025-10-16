using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public Fish[] fishPrefabs;
    public Rect spawnArea = new Rect(-9f, -4f, 18f, 3f); // x,y,width,height
    public int desiredFishCount = 8;
    public float spawnInterval = 1.5f;

    float t;

    void Update()
    {
        if (CountFish() >= desiredFishCount) return;
        t += Time.deltaTime;
        if (t >= spawnInterval)
        {
            t = 0f;
            SpawnOne();
        }
    }

    int CountFish() => FindObjectsOfType<Fish>().Length;

    void SpawnOne()
    {
        if (fishPrefabs.Length == 0) return;
        var prefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];
        float x = Random.Range(spawnArea.xMin, spawnArea.xMax);
        float y = Random.Range(spawnArea.yMin, spawnArea.yMax);
        Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);
    }
}
