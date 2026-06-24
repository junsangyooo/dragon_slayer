using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 레벨업 시 N장의 업그레이드 카드를 띄워 하나를 고르게 하고, 선택을 플레이어 스탯/무기에 적용한다.
// 카드 UI는 씬에 미리 만들지 않고 런타임에 코드로 생성한다(씬 YAML 수작업 위험 회피).
// 프레임 스프라이트는 Resources/UI/upgrade_card_frame 에서 로드한다.
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    // 하나의 업그레이드 정의
    private class Upgrade
    {
        public string name;             // 배너에 표시될 이름 (영문)
        public string iconLabel;        // 아이콘 원 안에 표시될 짧은 코드
        public Func<int, string> desc;  // (적용 후 레벨) -> 설명 문구
        public Color color;             // 아이콘/포인트 색
        public int level;               // 현재 레벨 (0 = 아직 미획득)
        public int maxLevel;
        public Action apply;            // 한 레벨 적용 (level++ 이후 호출)
    }

    private readonly List<Upgrade> pool = new List<Upgrade>();
    private int pendingLevelUps = 0;
    private bool choosing = false;

    private Sprite frameSprite;
    private GameObject canvasGO;   // 현재 떠 있는 카드 UI 루트

    private const int CHOICES = 3; // 한 번에 보여줄 카드 수

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        LoadFrameSprite();
        BuildPool();
    }

    private void LoadFrameSprite()
    {
        // PNG는 Unity가 Texture2D로 임포트되므로, 런타임에 Sprite로 변환한다(임포트 타입 의존 X).
        Texture2D tex = Resources.Load<Texture2D>("UI/upgrade_card_frame");
        if (tex != null)
        {
            frameSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
        else
        {
            Debug.LogWarning("UpgradeManager: Resources/UI/upgrade_card_frame 를 찾지 못했습니다. 카드 프레임이 비어 보일 수 있습니다.");
        }
    }

    private void BuildPool()
    {
        Player p = Player.Instance;
        WeaponsAndBuffs wab = (p != null) ? p.GetComponent<WeaponsAndBuffs>() : null;
        pool.Clear();

        // --- 무기: 파이어볼 (지금 작동하는 유일한 무기) ---
        pool.Add(new Upgrade
        {
            name = "FIREBALL", iconLabel = "FIRE", color = new Color(1f, 0.5f, 0.12f), maxLevel = 6,
            desc = lv => "Fires " + lv + " fireball" + (lv == 1 ? "" : "s") + "\nat the nearest enemy.",
            apply = () => { if (wab != null) wab.UpgradeFireball(); }
        });

        // --- 버프 9종 (Player 스탯에 직접 적용) ---
        pool.Add(new Upgrade
        {
            name = "POWER", iconLabel = "ATK", color = new Color(0.95f, 0.25f, 0.2f), maxLevel = 8,
            desc = lv => "Weapon damage +15%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffWeaponDamage(0.15f); }
        });
        pool.Add(new Upgrade
        {
            name = "VITALITY", iconLabel = "HP", color = new Color(0.4f, 0.85f, 0.3f), maxLevel = 8,
            desc = lv => "Max HP +20 and heal\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffMaxHealth(20f); }
        });
        pool.Add(new Upgrade
        {
            name = "SWIFT", iconLabel = "SPD", color = new Color(0.35f, 0.8f, 0.9f), maxLevel = 6,
            desc = lv => "Move speed +10%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffMoveSpeed(0.10f); }
        });
        pool.Add(new Upgrade
        {
            name = "HASTE", iconLabel = "AS", color = new Color(0.95f, 0.85f, 0.25f), maxLevel = 8,
            desc = lv => "Attack speed +12%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffAttackSpeed(0.12f); }
        });
        pool.Add(new Upgrade
        {
            name = "PRECISION", iconLabel = "CRIT", color = new Color(0.95f, 0.95f, 0.95f), maxLevel = 6,
            desc = lv => "Critical chance +5%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffCritRate(0.05f); }
        });
        pool.Add(new Upgrade
        {
            name = "DEVASTATE", iconLabel = "CDMG", color = new Color(0.85f, 0.3f, 0.85f), maxLevel = 6,
            desc = lv => "Critical damage +25%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffCritDamage(0.25f); }
        });
        pool.Add(new Upgrade
        {
            name = "MAGNET", iconLabel = "MAG", color = new Color(0.4f, 0.6f, 0.95f), maxLevel = 5,
            desc = lv => "Pickup range +25%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffMagneticRadius(0.25f); }
        });
        pool.Add(new Upgrade
        {
            name = "VELOCITY", iconLabel = "PROJ", color = new Color(1f, 0.7f, 0.2f), maxLevel = 5,
            desc = lv => "Projectile speed +15%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffWeaponMoveSpeed(0.15f); }
        });
        pool.Add(new Upgrade
        {
            name = "GUARD", iconLabel = "DEF", color = new Color(0.6f, 0.7f, 0.85f), maxLevel = 5,
            desc = lv => "Invincibility time +15%\n(Lv." + lv + ")",
            apply = () => { if (p != null) p.BuffInvincibilityTime(0.15f); }
        });
    }

    // Player.LevelUp() 에서 호출. 여러 번 쌓이면 순차적으로 처리한다.
    public void QueueLevelUp()
    {
        pendingLevelUps++;
        if (!choosing)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        choosing = true;
        Time.timeScale = 0f; // 카드 고르는 동안 게임 일시정지
        while (pendingLevelUps > 0)
        {
            if (IsGameOver()) { AbortToGameOver(); yield break; }
            pendingLevelUps--;
            bool picked = false;
            ShowChoices(() => { picked = true; });
            // timeScale=0 에서도 yield return null 은 매 프레임 재개된다(WaitForSeconds 와 달리).
            while (!picked)
            {
                if (IsGameOver()) { AbortToGameOver(); yield break; }
                yield return null;
            }
        }
        if (IsGameOver()) { AbortToGameOver(); yield break; } // 게임오버 freeze를 덮어쓰지 않음
        Time.timeScale = 1f; // 재개
        choosing = false;
    }

    private bool IsGameOver()
    {
        // 게임오버뿐 아니라 승리로 런이 끝난 경우에도 카드 UI를 닫고 timeScale을 건드리지 않는다.
        return GameManager.Instance != null && GameManager.Instance.IsRunOver();
    }

    // 게임오버 시: 카드 UI를 닫고 큐를 비우되, timeScale은 GameManager(=0)가 소유하므로 건드리지 않는다.
    private void AbortToGameOver()
    {
        CloseUI();
        pendingLevelUps = 0;
        choosing = false;
    }

    // 아직 만렙이 아닌 업그레이드 중 무작위 CHOICES개를 골라 카드로 띄운다.
    private void ShowChoices(Action onPicked)
    {
        List<Upgrade> avail = pool.FindAll(u => u.level < u.maxLevel);
        if (avail.Count == 0)
        {
            // 더 줄 게 없으면 그냥 통과
            onPicked();
            return;
        }
        // Fisher-Yates 셔플
        for (int i = 0; i < avail.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, avail.Count);
            Upgrade tmp = avail[i]; avail[i] = avail[j]; avail[j] = tmp;
        }
        int n = Mathf.Min(CHOICES, avail.Count);
        List<Upgrade> choices = avail.GetRange(0, n);
        BuildUI(choices, onPicked);
    }

    private void Choose(Upgrade u, Action onPicked)
    {
        u.level++;
        if (u.apply != null) u.apply();
        CloseUI();
        onPicked();
    }

    private void CloseUI()
    {
        if (canvasGO != null)
        {
            Destroy(canvasGO);
            canvasGO = null;
        }
    }

    // ---------------- 런타임 UI 생성 ----------------

    private void BuildUI(List<Upgrade> choices, Action onPicked)
    {
        EnsureEventSystem();

        // 오버레이 캔버스
        canvasGO = new GameObject("UpgradeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // HUD 위에
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 어두운 딤 배경 (뒤쪽 입력 차단)
        GameObject dim = NewUI("Dim", canvasGO.transform);
        Image dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.65f);
        dimImg.raycastTarget = true;
        Stretch(dim.GetComponent<RectTransform>());

        // "LEVEL UP" 타이틀
        GameObject titleGO = NewUI("Title", canvasGO.transform);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.86f);
        titleRT.anchorMax = new Vector2(0.5f, 0.86f);
        titleRT.sizeDelta = new Vector2(900f, 160f);
        titleRT.anchoredPosition = Vector2.zero;
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        ConfigText(title, "LEVEL UP!", 90, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center, false);

        // 카드 배치
        float cardW = 300f;
        float cardH = cardW / 0.594f; // 프레임 종횡비(965x1623)
        float gap = 36f;
        int n = choices.Count;
        float totalW = n * cardW + (n - 1) * gap;
        float startX = -totalW / 2f + cardW / 2f;

        for (int i = 0; i < n; i++)
        {
            Upgrade u = choices[i];
            float x = startX + i * (cardW + gap);
            BuildCard(u, x, cardW, cardH, onPicked);
        }
    }

    private void BuildCard(Upgrade u, float x, float cardW, float cardH, Action onPicked)
    {
        // 카드 루트 (프레임 이미지 + 버튼)
        GameObject card = NewUI("Card_" + u.name, canvasGO.transform);
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(cardW, cardH);
        rt.anchoredPosition = new Vector2(x, -30f);

        Image frame = card.AddComponent<Image>();
        if (frameSprite != null)
        {
            frame.sprite = frameSprite;
            frame.preserveAspect = true;
        }
        else
        {
            frame.color = new Color(0.25f, 0.12f, 0.08f, 0.95f); // 프레임 못 불러왔을 때 폴백
        }
        frame.raycastTarget = true;

        Button btn = card.AddComponent<Button>();
        btn.targetGraphic = frame;
        Upgrade captured = u;
        btn.onClick.AddListener(() => Choose(captured, onPicked));

        // 아이콘 라벨 (상단 원 영역)
        TextMeshProUGUI icon = MakeChildText(card.transform, u.iconLabel, 48, u.color,
            new Vector2(0.20f, 0.60f), new Vector2(0.80f, 0.85f), true);

        // 이름 (배너)
        MakeChildText(card.transform, u.name, 40, new Color(1f, 0.95f, 0.85f),
            new Vector2(0.10f, 0.42f), new Vector2(0.90f, 0.54f), true);

        // 레벨 표시
        MakeChildText(card.transform, "Lv." + (u.level + 1), 26, new Color(1f, 0.85f, 0.4f),
            new Vector2(0.10f, 0.36f), new Vector2(0.90f, 0.42f), true);

        // 설명 (하단 패널)
        MakeChildText(card.transform, u.desc(u.level + 1), 26, new Color(0.92f, 0.9f, 0.85f),
            new Vector2(0.12f, 0.12f), new Vector2(0.88f, 0.34f), true);
    }

    // 같은 GameObject 에 RectTransform 을 가진 빈 UI 노드 생성
    private GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private TextMeshProUGUI MakeChildText(Transform parent, string text, int size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, bool autoSize)
    {
        GameObject go = NewUI("Text", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        ConfigText(t, text, size, color, TextAlignmentOptions.Center, autoSize);
        return t;
    }

    private void ConfigText(TextMeshProUGUI t, string text, int size, Color color, TextAlignmentOptions align, bool autoSize)
    {
        t.text = text;
        t.color = color;
        t.alignment = align;
        t.fontStyle = FontStyles.Bold;
        t.enableWordWrapping = true;
        t.raycastTarget = false; // 클릭은 카드 버튼이 받도록
        if (t.font == null && TMP_Settings.defaultFontAsset != null)
        {
            t.font = TMP_Settings.defaultFontAsset;
        }
        if (autoSize)
        {
            t.enableAutoSizing = true;
            t.fontSizeMax = size;
            t.fontSizeMin = 10;
        }
        else
        {
            t.fontSize = size;
        }
    }

    private void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        if (FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }
}
