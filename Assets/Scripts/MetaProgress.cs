using UnityEngine;

// 런(run) 사이에 유지되는 메타 진행도(누적 골드 + 영구 업그레이드 레벨)를 PlayerPrefs에 저장한다.
// 로비 상점에서 골드로 영구 업그레이드를 사고, 게임 시작 시 Player가 이 값을 읽어 적용한다.
public static class MetaProgress
{
    private const string K_GOLD = "meta_gold";
    private const string K_HP = "meta_hp_level";
    private const string K_DMG = "meta_dmg_level";
    private const string K_SPD = "meta_spd_level";

    public static int Gold
    {
        get { return PlayerPrefs.GetInt(K_GOLD, 0); }
        set { PlayerPrefs.SetInt(K_GOLD, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }
    public static int HpLevel
    {
        get { return PlayerPrefs.GetInt(K_HP, 0); }
        set { PlayerPrefs.SetInt(K_HP, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }
    public static int DmgLevel
    {
        get { return PlayerPrefs.GetInt(K_DMG, 0); }
        set { PlayerPrefs.SetInt(K_DMG, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }
    public static int SpeedLevel
    {
        get { return PlayerPrefs.GetInt(K_SPD, 0); }
        set { PlayerPrefs.SetInt(K_SPD, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static void AddGold(int amount)
    {
        Gold = Gold + amount;
    }

    // 영구 업그레이드 1레벨당 효과량
    public const float HP_PER_LEVEL = 25f;
    public const float DMG_PER_LEVEL = 1f;
    public const float SPEED_PER_LEVEL = 0.5f;
}
