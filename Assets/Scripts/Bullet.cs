using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 5f;

    public float rotationSpeed = 180f;
    [SerializeField] private float minSize = 0.8f;
    [SerializeField] private float maxSize = 1.4f;

    [HideInInspector] public Vector2 direction = Vector2.right;

    private float currentRotation;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        // 시계/반시계 랜덤
        currentRotation = Random.value > 0.5f ? rotationSpeed : -rotationSpeed;

        // 크기 랜덤
        float randomScale = Random.Range(minSize, maxSize);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameRunning) return;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward * currentRotation * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerMove>(out _)) return;

        if (GameManager.Instance != null)
            GameManager.Instance.DamagePlayer();

        Destroy(gameObject);
    }
}
