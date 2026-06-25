using System.Collections.Generic;
using UnityEngine;

// 무기/적이 공유하는 피해 인터페이스. 모든 적(Bat/Spider/Slime/OneEye/Boss)이 구현한다.
// 무기는 Physics2D.OverlapCircle 등으로 닿은 콜라이더에서 IDamageable을 찾아 TakeDamage를 호출한다.
public interface IDamageable
{
    void TakeDamage(float dmg);
}

// 무기 시각효과 공용 유틸. 에셋 의존 없이 코드로 원형 스프라이트를 생성해 재사용한다.
public static class WeaponVisuals
{
    private static Sprite _circle;

    // 가장자리가 부드러운 흰색 원 스프라이트(틴트해서 사용). 64px / 64PPU = 지름 1 유닛.
    public static Sprite Circle()
    {
        if (_circle != null) return _circle;
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / r;
                float a;
                if (dist <= 0.8f) a = 1f;
                else if (dist <= 1f) a = Mathf.InverseLerp(1f, 0.8f, dist);
                else a = 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return _circle;
    }

    private static Dictionary<string, Sprite> _fxCache;
    // Resources/Weapons/<name> 텍스처를 1유닛 너비 스프라이트로 로드(캐시). 없으면 원으로 대체.
    public static Sprite Fx(string name)
    {
        if (_fxCache == null) _fxCache = new Dictionary<string, Sprite>();
        Sprite cached;
        if (_fxCache.TryGetValue(name, out cached)) return cached;
        Texture2D tex = Resources.Load<Texture2D>("Weapons/" + name);
        Sprite sp = (tex != null)
            ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width)
            : Circle();
        _fxCache[name] = sp;
        return sp;
    }

    // 지정 스프라이트로 시각효과 GameObject 생성(지름 diameter 유닛). 스프라이트는 1유닛 너비라 localScale=diameter.
    public static GameObject SpawnSprite(Sprite sprite, Vector3 pos, float diameter, int order)
    {
        GameObject go = new GameObject("WeaponFX");
        go.transform.position = pos;
        go.transform.localScale = new Vector3(diameter, diameter, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return go;
    }

    // 원형 시각효과 GameObject 하나를 만들어 반환(지름 diameter 유닛, 색 color, 정렬순서 order).
    public static GameObject SpawnCircle(Vector3 pos, float diameter, Color color, int order)
    {
        GameObject go = new GameObject("WeaponFX");
        go.transform.position = pos;
        go.transform.localScale = new Vector3(diameter, diameter, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Circle();
        sr.color = color;
        sr.sortingOrder = order;
        return go;
    }

    // 중심 반경 안의 모든 적(IDamageable)에 dmg 만큼 피해.
    public static void DamageInRadius(Vector3 center, float radius, float dmg)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            IDamageable d = hits[i].GetComponent<IDamageable>();
            if (d != null) d.TakeDamage(dmg);
        }
    }
}
