using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectSpawner : MonoBehaviour {

    [SerializeField] private enum ObjectType { SmallStar, BigStar, Enemy }

    [SerializeField] private Tilemap landTilemap;
    [SerializeField] private GameObject[] objectPrefabs; // 0 = SmallStar, 1 = BigStar, 2 = Enemy
    [SerializeField] private float bigStarProbability = 0.2f; // 20% chance of spawning big star
    [SerializeField] private float enemyProbability = 0.1f;
    [SerializeField] private int maxObjects = 5;
    [SerializeField] private float starLifeTime = 10f; // Only for stars
    [SerializeField] private float spawnInterval = 1f;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> spawnObjects = new List<GameObject>();
    private bool isSpawning = false;

    private void Start() {
        GatherSpawnPositions();
        StartCoroutine(SpawnObjectsIfNeeded());
    }

    void Update() {
        if (!isSpawning && ActiveObjectsCount() < maxObjects) {
            StartCoroutine(SpawnObjectsIfNeeded());
        }
    }

    private int ActiveObjectsCount() {
        spawnObjects.RemoveAll(item => item == null);
        return spawnObjects.Count;
    }

    private IEnumerator SpawnObjectsIfNeeded() {
        isSpawning = true;
        while (ActiveObjectsCount() < maxObjects) {

            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false;
    }

    private bool PositionHasObject(Vector3 position) {
        return spawnObjects.Any(checkObj => checkObj && Vector3.Distance(checkObj.transform.position, position) < 1f);
    }

    private ObjectType RandomObjectType() {
        float randomChoice = Random.value;

        if (randomChoice < enemyProbability) {
            return ObjectType.Enemy;
        }
        else if (randomChoice < enemyProbability + bigStarProbability) {
            return ObjectType.BigStar;
        }
        else {
            return ObjectType.SmallStar;
        }
    }

    private void SpawnObject() {
        if (validSpawnPositions.Count == 0) return;

        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while (!validPositionFound && validSpawnPositions.Count > 0) {
            int randomIndex = Random.Range(0, validSpawnPositions.Count);
            Vector3 potentialPosition = validSpawnPositions[randomIndex];
            Vector3 leftPosition = potentialPosition + Vector3.left;
            Vector3 rightPosition = potentialPosition + Vector3.right;

            if (!PositionHasObject(leftPosition) && !PositionHasObject(rightPosition)) {
                spawnPosition = potentialPosition;
                validPositionFound = true;
            }

            validSpawnPositions.RemoveAt(randomIndex);
        }

        if (validPositionFound) {
            ObjectType objectType = RandomObjectType();
            GameObject gameObject = Instantiate(objectPrefabs[(int)objectType], spawnPosition, Quaternion.identity);
            spawnObjects.Add(gameObject);

            if (objectType != ObjectType.Enemy) {
                StartCoroutine(DestroyObjectAfterTime(gameObject, starLifeTime));
            }
        }
    }

    private IEnumerator DestroyObjectAfterTime(GameObject gameObject, float time) {
        yield return new WaitForSeconds(time);

        if (gameObject) {
            spawnObjects.Remove(gameObject);
            validSpawnPositions.Add(gameObject.transform.position);
            Destroy(gameObject);
        }
    }

    private void GatherSpawnPositions() {
        validSpawnPositions.Clear();
        BoundsInt boundsInt = landTilemap.cellBounds;
        TileBase[] allTiles = landTilemap.GetTilesBlock(boundsInt);
        Vector3 start = landTilemap.CellToWorld(new Vector3Int(boundsInt.xMin, boundsInt.yMin, 0));

        for (int x = 0; x < boundsInt.size.x; x++) {
            for (int y = 0; y < boundsInt.size.y; y++) {
                TileBase tile = allTiles[x + y * boundsInt.size.x];
                if (tile != null) {
                    // x + 0.5f and y + 2f to be in the middle of the tile
                    Vector3 place = start + new Vector3(x + 0.5f, y + 2f, 0);
                    validSpawnPositions.Add(place);
                }
            }
        }
    }
}
