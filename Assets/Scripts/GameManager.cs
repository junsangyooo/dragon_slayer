using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    EnemySpawner enemySpawner;
    //UI에 필요한 변수들
    [SerializeField]
    private Image DashCoolTime;
    private float dashingCooldown = 2.5f;
    [SerializeField]
    private GameObject pauseImage;

    [SerializeField]
    private TextMeshProUGUI time;

    [SerializeField]
    private TextMeshProUGUI gold_text;
    private int gold;

    [SerializeField]
    private Image hp_fill;
    [SerializeField]
    private Image exp_fill;

    private bool isPaused;
    private bool pauseImageOn;

    private int playTime;
    public int getPlayTime() {return playTime;}

    // 게임 플레이에 필요한 변수들
    public bool playing;
    public bool GetPlaying() {return playing;}
    private bool isGameOver = false;
    private bool isWin = false;
    public bool IsGameOver() {return isGameOver;}
    public bool IsWin() {return isWin;}
    public bool IsRunOver() {return isGameOver || isWin;} // 게임오버 또는 승리로 런이 끝난 상태
    public List<GameObject> enemies = new List<GameObject>();

    private void Awake() {
        Instance = this;
        // 레벨업 업그레이드 매니저를 런타임에 부착 (씬 수작업 불필요)
        if (GetComponent<UpgradeManager>() == null) {
            gameObject.AddComponent<UpgradeManager>();
        }
    }

    private void Start() {
        enemySpawner = GetComponent<EnemySpawner>();
        GameStart();
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void GameStart() {
        playing = true;
        isPaused = false;
        pauseImageOn = false;
        playTime = 0;
        enemySpawner.StartSpawning();
        StartCoroutine("PlayTime");
    }

    IEnumerator PlayTime() {
        while (playing) {
            yield return new WaitForSeconds(1f);
            playTime++;
            UpdateTime();
        }
    }

    public void UpdateTime() {
        int second = playTime % 60;
        int minute = (playTime - second) / 60;
        if (minute < 10 && second < 10) {
            time.text = "0" + minute.ToString() + ":0" + second.ToString();
        } else if (minute < 10) {
            time.text = "0" + minute.ToString() + ":" + second.ToString();
        } else if (second < 10) {
            time.text =  minute.ToString() + ":0" + second.ToString();
        } else {
            time.text = minute.ToString() + ":" + second.ToString();
        }
    }

    public void AddGold() {
        gold++;
        UpdateGold();
    }
    public void UpdateGold() {
        gold_text.text = gold.ToString();
    }
    public void UpdateHP() {
        hp_fill.fillAmount = Player.Instance.getCurrentHP() / Player.Instance.getMaxHP();
    }
    public void UpdateEXP() {
        int max = Player.Instance.getMaxExp();
        if (max <= 0) {
            exp_fill.fillAmount = 0f;
            return;
        }
        // float 캐스팅으로 정수 나눗셈(0 또는 1로 잘림) 방지.
        exp_fill.fillAmount = (float)Player.Instance.getCurrentExp() / max;
    }
    public void UpdateDashCool(float curTime) {
        DashCoolTime.fillAmount = curTime / dashingCooldown;
    }

    public void PauseButtonClicked() {
        isPaused = !isPaused;
        pauseImage.gameObject.SetActive(isPaused);
        if (pauseImageOn) {
            Resume();
        } else {
            Pause();
        }
        pauseImageOn = !pauseImageOn;
    }

    public void Pause() {
        Time.timeScale = 0;
    }

    public void Resume() {
        Time.timeScale = 1;
    }

    public void GameOver() {
        if (isWin || isGameOver) return; // 이미 승리/게임오버면 중복 UI 방지 (같은 프레임 사망·보스처치 대비)
        isGameOver = true;
        Pause();
        ShowDefeat();
    }

    // 보스 처치 시 호출: 승리 처리 + 승리 화면.
    public void Win() {
        if (isWin || isGameOver) return;
        isWin = true;
        playing = false; // 스포너/타이머 코루틴 종료
        Pause();         // 화면 고정 (UpgradeManager는 IsRunOver를 보고 timeScale을 건드리지 않음)
        ShowRunEndScreen("VICTORY!", new Color(1f, 0.85f, 0.3f));
    }

    // 사망 시 호출: 패배 화면.
    private void ShowDefeat() {
        ShowRunEndScreen("DEFEAT", new Color(0.95f, 0.35f, 0.3f));
    }

    // 런타임으로 런 종료(승리/패배) 화면 생성. RETRY(현재 씬 리로드) + RETURN TO LOBBY 버튼. 씬 수작업 불필요.
    private void ShowRunEndScreen(string title, Color titleColor) {
        GameObject cv = new GameObject("RunEndCanvas");
        Canvas canvas = cv.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1500;
        CanvasScaler sc = cv.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080f, 1920f);
        sc.matchWidthOrHeight = 0.5f;
        cv.AddComponent<GraphicRaycaster>();

        GameObject dim = new GameObject("Dim", typeof(RectTransform));
        dim.transform.SetParent(cv.transform, false);
        Image dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform drt = dim.GetComponent<RectTransform>();
        drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one;
        drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;

        MakeUIText(cv.transform, title, 110, titleColor,
            new Vector2(0.5f, 0.66f), new Vector2(960f, 220f));
        int sec = playTime % 60;
        int minute = playTime / 60;
        string stats = "Survived  " + minute.ToString("00") + ":" + sec.ToString("00") + "\nGold  " + gold;
        MakeUIText(cv.transform, stats, 46, Color.white,
            new Vector2(0.5f, 0.50f), new Vector2(960f, 300f));

        MakeButton(cv.transform, "RETRY", new Color(0.3f, 0.65f, 0.35f, 1f),
            new Vector2(0.5f, 0.34f), RestartRun);
        MakeButton(cv.transform, "RETURN TO LOBBY", new Color(0.85f, 0.55f, 0.15f, 1f),
            new Vector2(0.5f, 0.22f), ReturnToLobby);
    }

    private void MakeUIText(Transform parent, string s, int size, Color c, Vector2 anchor, Vector2 sizeDelta) {
        GameObject go = new GameObject("UIText", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = Vector2.zero;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = s; t.fontSize = size; t.color = c;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        if (t.font == null && TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
    }

    private void MakeButton(Transform parent, string label, Color bg, Vector2 anchor, UnityEngine.Events.UnityAction onClick) {
        GameObject go = new GameObject("Btn_" + label, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(560f, 120f); rt.anchoredPosition = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = bg;
        Button b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);
        MakeUIText(go.transform, label, 36, Color.white, new Vector2(0.5f, 0.5f), new Vector2(560f, 120f));
    }

    private void RestartRun() {
        Time.timeScale = 1f; // 씬 로드 전 timeScale 복구
        SceneManager.LoadScene("Cave");
    }

    private void ReturnToLobby() {
        Time.timeScale = 1f; // 씬 로드 전 timeScale 복구 (안 하면 로비도 멈춤)
        SceneManager.LoadScene("Lobby");
    }

    public void GameEnd() {
        playing = false;
        OnDestroy();
    }

}

