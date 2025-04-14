using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BgmType
{
    Bgm0,
    Bgm1
}

public enum SfxType
{
    Attack,
    Hit
}


public class GameManager : MonoBehaviour
{
    [Header("----- UI 컴포넌트 참조 -----")]
    public Image HpBar;

    [Header("----- 사운드 컴포넌트 참조 -----")]
    public AudioSource BgmAudioSource;  // 배경음악 재생하는 오디오소스 컴포넌트
    public AudioSource SfxAudioSource;  // 효과음 재생하는 오디오소스 컴포넌트

    [Header("----- 사운드 클립 리소스 참조 -----")]
    public AudioClip[] BgmClips;    // 배경음악 오디오 클립 배열
    public AudioClip[] SfxClips;    // 효과음 오디오 클립 배열

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayBgm(BgmType.Bgm0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayBgm(BgmType.Bgm1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            PauseBgm();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StopBgm();
        }
    }


    public void UpdateHpBar(float currentHp, float maxHp)
    {
        HpBar.fillAmount = currentHp / maxHp;
    }

    // 효과음 재생하는 함수
    public void PlaySfx(SfxType sfxType)
    {
        AudioClip clip = SfxClips[(int)sfxType];
        SfxAudioSource.PlayOneShot(clip);
        // PlayOneShot()은 어떤 효과음을 한 번 재생시키는 함수
    }

    // 배경음악 재생하는 함수
    public void PlayBgm(BgmType bgmType)
    {
        AudioClip clip = BgmClips[(int)bgmType];
        BgmAudioSource.clip = clip;
        BgmAudioSource.Play();
    }

    // 배경음악 멈추는 함수
    public void StopBgm()
    {
        BgmAudioSource.Stop();
    }

    // 배경음악 일시 정지를 전환하는 함수
    public void PauseBgm()
    {
        // 사운드가 현재 재생 중이면
        if(BgmAudioSource.isPlaying == true)
        {
            BgmAudioSource.Pause();
        }
        // 사운드가 현재 재생 중이 아니면
        else
        {
            BgmAudioSource.UnPause();
        }
    }
}
