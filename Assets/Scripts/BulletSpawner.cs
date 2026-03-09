using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float baseSpawnInterval = 1f;
    public float minSpawnInterval = 0.2f;
    public float spawnAcceleration = 0.01f;
    public float spawnOffset = 1f;
    public float baseBulletSpeed = 5f;
    public float speedAcceleration = 0.1f;

    private float timer;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameRunning)
        {
            timer = 0f; // 선택: 시작 직후 즉시 스폰 방지
            return;
        }


        timer += Time.deltaTime;
        float elapsed = Time.timeSinceLevelLoad;
        float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - elapsed * spawnAcceleration);
        if (timer < interval) return;
        timer = 0f;
        SpawnBullet(baseBulletSpeed + elapsed * speedAcceleration);
    }

    private void SpawnBullet(float bulletSpeed)
    {
        if (bulletPrefab == null || mainCamera == null) return;

        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;

        int edge = Random.Range(0, 4);
        Vector2 spawnPos;
        Vector2 direction;

        switch (edge)
        {
            case 0: spawnPos = new Vector2(-halfWidth - spawnOffset, Random.Range(-halfHeight, halfHeight)); direction = Vector2.right; break;
            case 1: spawnPos = new Vector2(halfWidth + spawnOffset, Random.Range(-halfHeight, halfHeight)); direction = Vector2.left; break;
            case 2: spawnPos = new Vector2(Random.Range(-halfWidth, halfWidth), halfHeight + spawnOffset); direction = Vector2.down; break;
            default: spawnPos = new Vector2(Random.Range(-halfWidth, halfWidth), -halfHeight - spawnOffset); direction = Vector2.up; break;
        }

        GameObject newBullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        if (newBullet.TryGetComponent(out Bullet bullet))
        {
            bullet.direction = direction;
            bullet.speed = bulletSpeed;
        }
    }
}
