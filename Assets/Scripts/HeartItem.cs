using UnityEngine;

public class HeartItem : MonoBehaviour
{
    [SerializeField] private float lifeTime = 6f;

    private void OnEnable()
    {
        CancelInvoke(nameof(SelfDestruct));
        if (lifeTime > 0f) Invoke(nameof(SelfDestruct), lifeTime);
    }

    public void Configure(float duration)
    {
        lifeTime = Mathf.Max(0.5f, duration);
        CancelInvoke(nameof(SelfDestruct));
        Invoke(nameof(SelfDestruct), lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerMove>(out _)) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        GameManager.Instance.AddLife();
        Destroy(gameObject);
    }

    private void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
