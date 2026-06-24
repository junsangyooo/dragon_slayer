using UnityEngine;

// 잠깐 보였다가 서서히 투명해지며 자동 파괴되는 시각 효과(번개 플래시 등).
public class FadeAndDie : MonoBehaviour
{
    public float life = 0.25f;
    private SpriteRenderer sr;
    private float t;
    private float startAlpha = 1f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) startAlpha = sr.color.a;
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(startAlpha, 0f, t / life);
            sr.color = c;
        }
        if (t >= life) Destroy(gameObject);
    }
}
