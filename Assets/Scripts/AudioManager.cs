using UnityEngine;

// 오디오 파일이 없어 코드로 간단한 절차적 SFX/BGM을 생성해 재생한다(에셋 의존 0).
// 지연 생성 싱글톤: 아무 곳에서나 AudioManager.Instance.PlayX() 로 호출하면 자동 생성된다.
// 나중에 실제 음원으로 교체하려면 BuildClips()의 클립을 Resources.Load 등으로 바꾸면 된다.
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                _instance = go.AddComponent<AudioManager>();
            }
            return _instance;
        }
    }

    private const int SR = 44100;

    private AudioSource sfx;
    private AudioSource music;
    private AudioClip cDeath, cPickup, cLevelUp, cHurt, cVictory, cDefeat, cBgm;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.volume = 0.10f;

        BuildClips();
        music.clip = cBgm;
        music.Play();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // ----- 외부에서 호출하는 재생 메서드 -----
    // 씬 시작 시 호출(멱등): BGM이 안 돌고 있으면 재생.
    public void StartBgm() { if (music != null && cBgm != null && !music.isPlaying) { music.clip = cBgm; music.Play(); } }
    public void PlayDeath()   { if (sfx != null && cDeath   != null) sfx.PlayOneShot(cDeath, 0.5f); }
    public void PlayPickup()  { if (sfx != null && cPickup  != null) sfx.PlayOneShot(cPickup, 0.45f); }
    public void PlayLevelUp() { if (sfx != null && cLevelUp != null) sfx.PlayOneShot(cLevelUp, 0.6f); }
    public void PlayHurt()    { if (sfx != null && cHurt    != null) sfx.PlayOneShot(cHurt, 0.5f); }
    public void PlayVictory() { if (sfx != null && cVictory != null) sfx.PlayOneShot(cVictory, 0.7f); }
    public void PlayDefeat()  { if (sfx != null && cDefeat  != null) sfx.PlayOneShot(cDefeat, 0.7f); }

    private void BuildClips()
    {
        cDeath   = Noise(0.18f, 0.5f);
        cPickup  = Sweep(880f, 1320f, 0.10f, false, 0.5f);
        cLevelUp = Arp(new float[] { 523f, 659f, 784f, 1047f }, 0.30f, 0.5f);
        cHurt    = Sweep(300f, 110f, 0.16f, true, 0.5f);
        cVictory = Arp(new float[] { 523f, 659f, 784f, 1047f, 1319f }, 0.55f, 0.55f);
        cDefeat  = Sweep(440f, 110f, 0.55f, false, 0.5f);
        cBgm     = BuildBgm();
    }

    // 시작 주파수에서 끝 주파수로 미끄러지는 톤(사인 또는 사각파) + 선형 감쇠.
    private AudioClip Sweep(float startFreq, float endFreq, float dur, bool square, float vol)
    {
        int n = Mathf.Max(1, (int)(SR * dur));
        float[] data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float u = (float)i / n;
            float f = Mathf.Lerp(startFreq, endFreq, u);
            phase += 2f * Mathf.PI * f / SR;
            float s = square ? Mathf.Sign(Mathf.Sin(phase)) : Mathf.Sin(phase);
            data[i] = s * (1f - u) * vol;
        }
        AudioClip clip = AudioClip.Create("sweep", n, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    // 여러 음을 순서대로 짧게 연주하는 아르페지오(사인) + 음마다 감쇠.
    private AudioClip Arp(float[] freqs, float totalDur, float vol)
    {
        int n = Mathf.Max(1, (int)(SR * totalDur));
        float[] data = new float[n];
        int noteSamples = Mathf.Max(1, n / freqs.Length);
        for (int i = 0; i < n; i++)
        {
            int note = Mathf.Min(freqs.Length - 1, i / noteSamples);
            float local = (float)(i - note * noteSamples) / noteSamples; // 0..1 within note
            float f = freqs[note];
            float s = Mathf.Sin(2f * Mathf.PI * f * i / SR);
            float env = 1f - local; // 음 내부 감쇠
            data[i] = s * env * vol;
        }
        AudioClip clip = AudioClip.Create("arp", n, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    // 화이트 노이즈 + 감쇠 (사망/타격감).
    private AudioClip Noise(float dur, float vol)
    {
        int n = Mathf.Max(1, (int)(SR * dur));
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float u = (float)i / n;
            data[i] = (Random.value * 2f - 1f) * (1f - u) * vol;
        }
        AudioClip clip = AudioClip.Create("noise", n, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    // 잔잔한 저음 BGM 루프(부드러운 사인 음들). 음원 볼륨이 낮아 거슬리지 않는다.
    private AudioClip BuildBgm()
    {
        float[] notes = { 130.81f, 164.81f, 196f, 164.81f }; // C3 E3 G3 E3
        float noteDur = 1.0f;
        int noteSamples = (int)(SR * noteDur);
        int n = noteSamples * notes.Length;
        float[] data = new float[n];
        for (int k = 0; k < notes.Length; k++)
        {
            float f = notes[k];
            for (int i = 0; i < noteSamples; i++)
            {
                int idx = k * noteSamples + i;
                float local = (float)i / noteSamples;
                // 음 시작/끝을 부드럽게(어택·릴리즈)해서 클릭음 방지
                float env = Mathf.Sin(Mathf.PI * local);
                float s = Mathf.Sin(2f * Mathf.PI * f * i / SR);
                data[idx] = s * env * 0.5f;
            }
        }
        AudioClip clip = AudioClip.Create("bgm", n, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }
}
