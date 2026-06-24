using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public static Main Instance {get; private set; }

    private void Awake() {
        Instance = this;
    }
    private void OnDestroy() {
        Instance = null;
    }

    [SerializeField]
    private GameObject RankingImage;
    [SerializeField]
    private GameObject SettingImage;
    [SerializeField]
    private GameObject MissionImage;
    [SerializeField]
    private GameObject UpgradeImage;

    public bool ImageDisplaying;

    [SerializeField]
    private TextMeshProUGUI GoldText;

    // Start is called before the first frame update
    void Start()
    {
        ImageDisplaying = false;
        RefreshGoldText();
        AudioManager.Instance.StartBgm();
    }

    private void RefreshGoldText()
    {
        if (GoldText != null) GoldText.text = "Gold: " + MetaProgress.Gold;
    }

    public void displaySetting() {
        if (!ImageDisplaying) {
            SettingImage.gameObject.SetActive(true);
            ImageDisplaying = true;
        }
    }
    public void displayRanking() {
        if (!ImageDisplaying) {
            RankingImage.gameObject.SetActive(true);
            ImageDisplaying = true;
        }
    }
    public void displayMission() {
        if (!ImageDisplaying) {
            MissionImage.gameObject.SetActive(true);
            ImageDisplaying = true;
        }
    }
    public void displayUpgrade() {
        if (ImageDisplaying) return;
        ImageDisplaying = true;
        BuildShop();
    }

    public void closeSetting() {
        SettingImage.gameObject.SetActive(false);
        ImageDisplaying = false;
    }
    public void closeRanking() {
        RankingImage.gameObject.SetActive(false);
        ImageDisplaying = false;
    }
    public void closeMission() {
        MissionImage.gameObject.SetActive(false);
        ImageDisplaying = false;
    }
    public void closeUpgrade() {
        if (shopCanvas != null) { Destroy(shopCanvas); shopCanvas = null; }
        if (UpgradeImage != null) UpgradeImage.gameObject.SetActive(false);
        ImageDisplaying = false;
        RefreshGoldText();
    }

    public void gameStart() {
        SceneManager.LoadScene("Cave");
    }

    // ===================== 로비 상점 (런타임 UI, 씬 수작업 불필요) =====================
    private GameObject shopCanvas;

    private int CostHp()  { return 30 * (MetaProgress.HpLevel + 1); }
    private int CostDmg() { return 50 * (MetaProgress.DmgLevel + 1); }
    private int CostSpd() { return 40 * (MetaProgress.SpeedLevel + 1); }

    private void BuildShop() {
        if (shopCanvas != null) Destroy(shopCanvas);
        shopCanvas = new GameObject("ShopCanvas");
        Canvas c = shopCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 1200;
        CanvasScaler sc = shopCanvas.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080f, 1920f);
        sc.matchWidthOrHeight = 0.5f;
        shopCanvas.AddComponent<GraphicRaycaster>();

        GameObject dim = NewUI("Dim", shopCanvas.transform);
        Image di = dim.AddComponent<Image>();
        di.color = new Color(0f, 0f, 0f, 0.82f);
        Stretch(dim.GetComponent<RectTransform>());

        MakeText(shopCanvas.transform, "SHOP", 84, new Color(1f, 0.85f, 0.3f), new Vector2(0.5f, 0.88f), new Vector2(800f, 160f));
        MakeText(shopCanvas.transform, "Gold: " + MetaProgress.Gold, 46, Color.white, new Vector2(0.5f, 0.80f), new Vector2(800f, 100f));

        MakeShopRow("MAX HEALTH +25", MetaProgress.HpLevel, CostHp(), 0.66f, BuyHp);
        MakeShopRow("WEAPON DAMAGE +1", MetaProgress.DmgLevel, CostDmg(), 0.54f, BuyDmg);
        MakeShopRow("MOVE SPEED +0.5", MetaProgress.SpeedLevel, CostSpd(), 0.42f, BuySpd);

        MakeButton(shopCanvas.transform, "CLOSE", new Color(0.7f, 0.3f, 0.25f), new Vector2(0.5f, 0.16f), new Vector2(420f, 110f), closeUpgrade);
    }

    private void MakeShopRow(string label, int level, int cost, float y, UnityEngine.Events.UnityAction buy) {
        MakeText(shopCanvas.transform, label + "   Lv." + level, 36, Color.white, new Vector2(0.34f, y), new Vector2(640f, 90f));
        bool canAfford = MetaProgress.Gold >= cost;
        Color bg = canAfford ? new Color(0.3f, 0.6f, 0.35f) : new Color(0.42f, 0.42f, 0.42f);
        Button b = MakeButton(shopCanvas.transform, "BUY (" + cost + ")", bg, new Vector2(0.76f, y), new Vector2(340f, 90f), buy);
        b.interactable = canAfford;
    }

    private void BuyHp()  { int c = CostHp();  if (MetaProgress.Gold >= c) { MetaProgress.Gold -= c; MetaProgress.HpLevel++;    BuildShop(); RefreshGoldText(); } }
    private void BuyDmg() { int c = CostDmg(); if (MetaProgress.Gold >= c) { MetaProgress.Gold -= c; MetaProgress.DmgLevel++;   BuildShop(); RefreshGoldText(); } }
    private void BuySpd() { int c = CostSpd(); if (MetaProgress.Gold >= c) { MetaProgress.Gold -= c; MetaProgress.SpeedLevel++; BuildShop(); RefreshGoldText(); } }

    // ----- UI 헬퍼 -----
    private GameObject NewUI(string name, Transform parent) {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
    private void Stretch(RectTransform rt) {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
    private TextMeshProUGUI MakeText(Transform parent, string s, int size, Color col, Vector2 anchor, Vector2 sizeDelta) {
        GameObject go = NewUI("Text", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = Vector2.zero;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = s; t.fontSize = size; t.color = col;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        if (t.font == null && TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
        return t;
    }
    private Button MakeButton(Transform parent, string label, Color bg, Vector2 anchor, Vector2 size, UnityEngine.Events.UnityAction onClick) {
        GameObject go = NewUI("Btn_" + label, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = bg;
        Button b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);
        MakeText(go.transform, label, 32, Color.white, new Vector2(0.5f, 0.5f), size);
        return b;
    }

}
