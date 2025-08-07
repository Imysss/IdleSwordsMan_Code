using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using static Define;

//게임 전체에서 배경음악(BGM), 효과음(SFX)을 통합적으로 제어하기 위한 중앙 집중형 관리 클래스
//enum Sound 구조를 바탕으로 각 사운드 타입에 따라 AudioSource를 미리 생성하고 효율적으로 사운드를 재생/중단/볼륨 제어할 수 있다.

//장점
//1. AudioSource를 Enum 기반 배열로 관리: Sound enum을 배열 인덱스로 사용하여 타입별로 빠르게 접근 가능
//2. 사운드 Root 오브젝트 고정화: DonDestroyOnLoad로 씬 전환 시에도 사운드 유지
//3. BGM과 SFX 분리 처리: Play 메서드 내부에서 Sound 타입에 따라 처리 분기 (loop 여부, PlayOneShot 등)
//4. 중복 로딩 방지: _audioClips Dictionary로 AudioClip 캐싱 처리
//5. 옵션 동기화 구조 분리: Managers.Game.BGMOn, SFXOn, BGMVolume 등으로 외부 설정과 연동 가능
//6. 외부에서 문자열 키로 Clip 접근: Play(type, key)를 통해 리소스 키 기반 사운드 재생 가능
public class SoundManager
{
    //BGM, SFX 등의 타입별로 AudioSource를 배열로 미리 할당하여 성능 최적화 및 접근 속도 향상
    private AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.Max];
    //사운드 중복 로딩 방지를 위한 Clip 캐시
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private GameObject _soundRoot = null;

    
    public void Init()
    {
        if (_soundRoot == null)
        {
            _soundRoot = GameObject.Find("SoundRoot");
            if (_soundRoot == null)
            {
                _soundRoot = new GameObject { name = "SoundRoot" };
                //씬 전환 시에도 사운드 유지
                UnityEngine.Object.DontDestroyOnLoad(_soundRoot);
                
                string[] soundTypeNames = System.Enum.GetNames(typeof(Sound));
                
                //각 사운드 타입별로 GameObject + AudioSource 생성 및 SoundRoot 하위로 구성
                for (int count = 0; count < soundTypeNames.Length - 1; count++)
                {
                    GameObject go = new GameObject { name = soundTypeNames[count] };
                    _audioSources[count] = go.AddComponent<AudioSource>();
                    go.transform.parent = _soundRoot.transform;
                }
                //BGM은 반복 재생을 기본 설정으로 둠
                _audioSources[(int)Sound.Bgm].loop = true;
            }
        }
    }

    public void Play(Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Play();
    }

    //리소스 키를 통해 BGM 또는 SFX 재생
    //SFX는 OneShot 재생, BGM은 clip 세팅 후 루프 재생
    // loop가 true면 반복 재생
    public void Play(Sound type, string key, float pitch = 1.0f, bool loop = false)
    {
        AudioSource audioSource = _audioSources[(int)type];
        if (type == Sound.Bgm)
        {
            LoadAudioClip(key, (audioClip) =>
            {
                if(audioSource.isPlaying)
                    audioSource.Stop();

                audioSource.clip = audioClip;
                if(Managers.Game.BGMOn)
                    audioSource.Play();
            });
        }
        else
        {
            LoadAudioClip(key, (audioClip) =>
            {
                audioSource.pitch = pitch;

                if (Managers.Game.SFXOn)
                {
                    if (loop)     
                    {
                        audioSource.clip = audioClip;
                        audioSource.loop = true;
                        audioSource.Play();
                    }
                    else
                    {
                        audioSource.PlayOneShot(audioClip);
                    }
                }
            });
        }
    }
    
    //클립 중복 로딩 방지 및 필요 시 리소스 로드하여 사용
    public void Play(Sound type, AudioClip audioClip, float pitch = 1.0f)
    {
        AudioSource audioSource = _audioSources[(int)type];
        if (type == Sound.Bgm)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            
            audioSource.clip = audioClip;
            if (Managers.Game.BGMOn)
                audioSource.Play();
        }
        else
        {
            audioSource.pitch = pitch;
            if (Managers.Game.SFXOn)
                audioSource.PlayOneShot(audioClip);
        }
    }

    public void Stop(Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Stop();
    }

    public void SetVolume(Sound type, float volume)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.volume = volume;
    }
    
    //게임 전체의 사운드를 키고 끌 때 사용(절전 모드 등)
    public void MuteAll(bool isMuted)
    {
        foreach (AudioSource source in _audioSources)
        {
            if (source != null)
            {
                source.mute = isMuted;
            }
        }
    }

    public void PlayButtonClick()
    {
        Play(Define.Sound.Sfx, "uiclick6");
    }

    public void PlayRewardButtonClick()
    {
        Play(Define.Sound.Sfx, "uireward2");
    }

    public void PlayUpgradeButtonClick()
    {
        Play(Define.Sound.Sfx, "uiupgradeclick");
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {   
            audioSource.Stop();
        }
        _audioClips.Clear();
    }
    
    private void LoadAudioClip(string key, Action<AudioClip> callback)
    {
        if (_audioClips.TryGetValue(key, out AudioClip cachedClip))
        {
            callback?.Invoke(cachedClip);
            return;
        }

        Managers.Resource.LoadAsync<AudioClip>(key, (loadedClip) =>
        {
            if (loadedClip != null)
            {
                _audioClips[key] = loadedClip;
                callback?.Invoke(loadedClip);
            }
            else
            {
                callback?.Invoke(null);
            }
        });
    }
}
