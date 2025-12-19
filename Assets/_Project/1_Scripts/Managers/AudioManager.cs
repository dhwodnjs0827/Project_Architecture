using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 오디오 관리 매니저
/// </summary>
public class AudioManager : MonoSingleton<AudioManager>
{
    private const string MASTER_VOLUME = "MasterVolume";
    private const string BGM_VOLUME = "BGMVolume";
    private const string SFX_VOLUME = "SFXVolume";
    
    private bool isMasterMuted = false;
    private bool isBgmMuted = false;
    private bool isSfxMuted = false;

    private float lastMasterVolume = 1f;
    private float lastBgmVolume = 1f;
    private float lastSfxVolume = 1f;

    [Header("Audio Mixer")] [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")] [SerializeField]
    private AudioSource bgmSource;

    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Settings")] [SerializeField]
    private float sfxCooldown = 0.05f; // 동시 재생 제한 시간

    private Dictionary<string, AudioClip> audioCache = new();
    private Dictionary<string, float> sfxLastPlayTime = new(); // 동시 재생 제한용

    public bool IsMasterMuted => isMasterMuted;
    public bool IsBgmMuted => isBgmMuted;
    public bool IsSfxMuted => isSfxMuted;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        if (bgmSource == null)
        {
            var bgmGo = new GameObject("BGM Source");
            bgmGo.transform.SetParent(transform);
            bgmSource = bgmGo.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            if (bgmMixerGroup != null) bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        }

        if (sfxSource == null)
        {
            var sfxGo = new GameObject("SFX Source");
            sfxGo.transform.SetParent(transform);
            sfxSource = sfxGo.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            if (sfxMixerGroup != null) sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    #region BGM

    /// <summary>
    /// BGM 재생
    /// </summary>
    public async UniTask PlayBgmAsync(string path, bool fade = true, float fadeDuration = 1f)
    {
        var clip = await LoadClipAsync(path);
        if (clip == null) return;

        if (fade && bgmSource.isPlaying)
        {
            await FadeOutAsync(bgmSource, fadeDuration);
        }

        bgmSource.clip = clip;
        bgmSource.volume = fade ? 0f : 1f;
        bgmSource.Play();

        if (fade)
        {
            await FadeInAsync(bgmSource, fadeDuration);
        }
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public async UniTask StopBgmAsync(bool fade = true, float fadeDuration = 1f)
    {
        if (!bgmSource.isPlaying) return;

        if (fade)
        {
            await FadeOutAsync(bgmSource, fadeDuration);
        }

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBgm() => bgmSource.Pause();

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBgm() => bgmSource.UnPause();

    #endregion

    #region SFX

    /// <summary>
    /// SFX 재생 (비동기)
    /// </summary>
    public async UniTask PlaySfxAsync(string path, float volume = 1f)
    {
        // 동시 재생 제한 체크
        if (!CanPlaySfx(path)) return;

        var clip = await LoadClipAsync(path);
        if (clip == null) return;

        PlaySfxInternal(clip, path, volume);
    }

    /// <summary>
    /// SFX 재생 (동기 - 캐시된 클립)
    /// </summary>
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        string key = clip.name;
        if (!CanPlaySfx(key)) return;

        PlaySfxInternal(clip, key, volume);
    }

    /// <summary>
    /// SFX 재생 (캐시된 경로)
    /// </summary>
    public void PlaySfx(string path, float volume = 1f)
    {
        if (!CanPlaySfx(path)) return;

        if (audioCache.TryGetValue(path, out var clip))
        {
            PlaySfxInternal(clip, path, volume);
        }
        else
        {
            // 캐시에 없으면 비동기로 로드
            PlaySfxAsync(path, volume).Forget();
        }
    }

    private void PlaySfxInternal(AudioClip clip, string key, float volume)
    {
        sfxSource.PlayOneShot(clip, volume);
        sfxLastPlayTime[key] = Time.time;
    }

    /// <summary>
    /// 동시 재생 가능 여부 확인
    /// </summary>
    private bool CanPlaySfx(string key)
    {
        if (sfxLastPlayTime.TryGetValue(key, out var lastTime))
        {
            if (Time.time - lastTime < sfxCooldown)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 동시 재생 제한 시간 설정
    /// </summary>
    public void SetSfxCooldown(float cooldown)
    {
        sfxCooldown = Mathf.Max(0f, cooldown);
    }

    #endregion

    #region Volume (AudioMixer)

    /// <summary>
    /// 마스터 볼륨 설정 (0 ~ 1)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        if (isMasterMuted)
        {
            lastMasterVolume = volume;
            return;
        }
        SetMixerVolume(MASTER_VOLUME, volume);
    }

    /// <summary>
    /// BGM 볼륨 설정 (0 ~ 1)
    /// </summary>
    public void SetBgmVolume(float volume)
    {
        if (isBgmMuted)
        {
            lastBgmVolume = volume;
            return;
        }
        SetMixerVolume(BGM_VOLUME, volume);
    }

    /// <summary>
    /// SFX 볼륨 설정 (0 ~ 1)
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        if (isSfxMuted)
        {
            lastSfxVolume = volume;
            return;
        }
        SetMixerVolume(SFX_VOLUME, volume);
    }

    /// <summary>
    /// 마스터 볼륨 가져오기
    /// </summary>
    public float GetMasterVolume() => GetMixerVolume(MASTER_VOLUME);

    /// <summary>
    /// BGM 볼륨 가져오기
    /// </summary>
    public float GetBgmVolume() => GetMixerVolume(BGM_VOLUME);

    /// <summary>
    /// SFX 볼륨 가져오기
    /// </summary>
    public float GetSfxVolume() => GetMixerVolume(SFX_VOLUME);

    private void SetMixerVolume(string parameter, float volume)
    {
        if (audioMixer == null)
        {
            CDebug.LogWarning("[AudioManager] AudioMixer가 할당되지 않았습니다.");
            return;
        }

        // 0~1 -> -80~0 dB 변환
        float dB = volume > 0f ? Mathf.Log10(volume) * 20f : -80f;
        audioMixer.SetFloat(parameter, dB);
    }

    private float GetMixerVolume(string parameter)
    {
        if (audioMixer == null) return 1f;

        if (audioMixer.GetFloat(parameter, out float dB))
        {
            // -80~0 dB -> 0~1 변환
            return Mathf.Pow(10f, dB / 20f);
        }

        return 1f;
    }

    #endregion
    
    #region Mute

    /// <summary>
    /// 마스터 음소거 토글
    /// </summary>
    public void ToggleMasterMute()
    {
        SetMasterMute(!isMasterMuted);
    }

    /// <summary>
    /// 마스터 음소거 설정
    /// </summary>
    public void SetMasterMute(bool mute)
    {
        if (mute == isMasterMuted) return;

        isMasterMuted = mute;
        if (mute)
        {
            lastMasterVolume = GetMasterVolume();
            SetMixerVolume(MASTER_VOLUME, 0f);
        }
        else
        {
            SetMixerVolume(MASTER_VOLUME, lastMasterVolume);
        }
    }

    /// <summary>
    /// BGM 음소거 토글
    /// </summary>
    public void ToggleBgmMute()
    {
        SetBgmMute(!isBgmMuted);
    }

    /// <summary>
    /// BGM 음소거 설정
    /// </summary>
    public void SetBgmMute(bool mute)
    {
        if (mute == isBgmMuted) return;

        isBgmMuted = mute;
        if (mute)
        {
            lastBgmVolume = GetBgmVolume();
            SetMixerVolume(BGM_VOLUME, 0f);
        }
        else
        {
            SetMixerVolume(BGM_VOLUME, lastBgmVolume);
        }
    }

    /// <summary>
    /// SFX 음소거 토글
    /// </summary>
    public void ToggleSfxMute()
    {
        SetSfxMute(!isSfxMuted);
    }

    /// <summary>
    /// SFX 음소거 설정
    /// </summary>
    public void SetSfxMute(bool mute)
    {
        if (mute == isSfxMuted) return;

        isSfxMuted = mute;
        if (mute)
        {
            lastSfxVolume = GetSfxVolume();
            SetMixerVolume(SFX_VOLUME, 0f);
        }
        else
        {
            SetMixerVolume(SFX_VOLUME, lastSfxVolume);
        }
    }

    #endregion

    #region Fade

    private async UniTask FadeInAsync(AudioSource source, float duration)
    {
        source.volume = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, elapsed / duration);
            await UniTask.Yield();
        }

        source.volume = 1f;
    }

    private async UniTask FadeOutAsync(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            await UniTask.Yield();
        }

        source.volume = 0f;
    }

    #endregion

    #region Load

    private async UniTask<AudioClip> LoadClipAsync(string path)
    {
        if (audioCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        var clip = await ResourceManager.Instance.LoadAsync<AudioClip>(path);
        if (clip != null)
        {
            audioCache[path] = clip;
        }
        else
        {
            CDebug.LogError($"[AudioManager] {path} 오디오 클립을 찾을 수 없습니다.");
        }

        return clip;
    }

    /// <summary>
    /// 오디오 클립 미리 로드
    /// </summary>
    public async UniTask PreloadAsync(string path)
    {
        await LoadClipAsync(path);
    }

    /// <summary>
    /// 캐시 정리
    /// </summary>
    public void ClearCache()
    {
        audioCache.Clear();
        sfxLastPlayTime.Clear();
    }

    #endregion
}