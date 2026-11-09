using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] animalPrefabs;
    private List<GameObject> _spawnedAnimals = new List<GameObject>();
    private BoxCollider _spawnArea;

    [Header("Spawning Settings")]
    [SerializeField] private float spawnCheckInterval = 1f; // How often to check and spawn
    private float _spawnTimer;

    void Awake()
    {
        _spawnArea = GetComponent<BoxCollider>();
    }

    void Update()
    {
        // Clean up any null/destroyed animal references from the list
        _spawnedAnimals.RemoveAll(animal => animal == null);

        // Periodically check to spawn more animals
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= spawnCheckInterval)
        {
            _spawnTimer = 0f;
            CheckAndSpawnAnimals();
        }
    }

    private void CheckAndSpawnAnimals()
    {
        if (animalPrefabs == null || animalPrefabs.Length == 0) return;

        foreach (var prefab in animalPrefabs)
        {
            // Count how many of this specific animal type are currently active
            int currentCount = 0;
            foreach (var spawned in _spawnedAnimals)
            {
                if (spawned != null && GetOriginalPrefabName(spawned) == prefab.name)
                {
                    currentCount++;
                }
            }

            // If there are fewer than 5, try to spawn one
            if (currentCount < 5)
            {
                if (TryGetValidGroundPosition(out Vector3 spawnPosition))
                {
                    GameObject newAnimal = Instantiate(prefab, spawnPosition, Quaternion.identity);
                    _spawnedAnimals.Add(newAnimal);
                }
            }
        }
    }

    private string GetOriginalPrefabName(GameObject spawnedAnimal)
    {
        // Unity automatically appends "(Clone)" to instantiated objects
        return spawnedAnimal.name.Replace("(Clone)", "").Trim();
    }

    private Vector3 GetRandomPositionInSpawnArea()
    {
        Vector3 center = _spawnArea.bounds.center;
        Vector3 size = _spawnArea.bounds.size;

        float randomX = Random.Range(center.x - size.x / 2f, center.x + size.x / 2f);
        float randomZ = Random.Range(center.z - size.z / 2f, center.z + size.z / 2f);
        float y = center.y + (size.y / 2f); // Start raycast from the top of the box collider

        return new Vector3(randomX, y, randomZ);
    }

    private bool TryGetValidGroundPosition(out Vector3 validPosition)
    {
        validPosition = Vector3.zero;
        int maxAttempts = 10; // Prevent infinite loops if ground isn't found
        int groundLayerMask = LayerMask.GetMask("Ground");

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = GetRandomPositionInSpawnArea();

            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 100f, groundLayerMask))
            {
                validPosition = hit.point;
                return true;
            }
        }

        return false; // Failed to find ground within max attempts
    }
}