using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BlinkTMPText : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 1f;

    private TextMeshProUGUI tmp;
    private Color baseColor;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        baseColor = tmp.color;
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.unscaledTime * speed, 1f);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
