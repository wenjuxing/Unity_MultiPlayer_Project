using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonBase<AudioManager>
{
    //不同声道组件
    AudioSource bgmAudioSource;
    AudioSource bgsAudioSource;
    AudioSource voiceAudioSource;

    //不同声道的游戏对象
    GameObject bgmController;
    GameObject bgsController;
    GameObject soundController;
    GameObject msController;
    GameObject voiceController;

    //控制器的名字
    string bgmControllerName = "BgmController";
    string bgsControllerName = "BgsController";
    string soundControllerName = "SoundController";
    string msControllerName = "MsController";
    string voiceControllerName = "VoiceController";

    private void Awake()
    {
        //创建背景音乐控制器
        bgmController = CreateController(bgmControllerName,transform);
        bgmAudioSource = bgmController.AddComponent<AudioSource>();
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true;

        //创建音效控制器
        soundController = CreateController(soundControllerName, transform);

        //创建环境音效控制器
        bgsController = CreateController(bgsControllerName, transform);
        bgsAudioSource = bgsController.AddComponent<AudioSource>();
        bgsAudioSource.playOnAwake = false;
        bgsAudioSource.loop = true;

        //创建提示音效控制器
        msController = CreateController(msControllerName, transform);

        //创建角色语音控制器
        voiceController = CreateController(voiceControllerName, transform);
        voiceAudioSource = voiceController.AddComponent<AudioSource>();
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.loop = true;
    }
    /// <summary>
    /// 创建声音控制器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    private GameObject CreateController(string name,Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go;
    }
    #region 播放音效
    /// <summary>
    /// 播放BGM
    /// </summary>
    /// <param name="bgm"></param>
    /// <param name="loop"></param>
    public void PlayBGM(AudioClip bgm,bool loop=true)
    {
        if (bgm == null) return;
        bgmAudioSource.loop = loop;
        bgmAudioSource.clip = bgm;
        bgmAudioSource.Play();
    }
    public void PauseBGM()
    {
        if (bgmAudioSource == null) return;
        bgmAudioSource.Pause();
    }
    public void UnPauseBGM()
    {
        if (bgmAudioSource == null) return;
        bgmAudioSource.UnPause();
    }
    public void StopBGM()
    {
        if (bgmAudioSource == null) return;
        bgmAudioSource.Stop();
        bgmAudioSource = null;
    }
    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySound(AudioClip sound,GameObject target)
    {
        if (sound == null || target == null) return;
        //创建临时物品播放音效
        GameObject go = new GameObject(sound.name);
        go.transform.SetParent(target.transform);
        go.transform.localPosition = Vector3.zero;
        //添加音频组件
        if (!go.TryGetComponent<AudioSource>(out AudioSource audioSource))
        {
            audioSource= go.GetComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            //3D效果近大远小
            audioSource.spatialBlend = 1f;
        }
        audioSource.clip = sound;
        audioSource.Play();
        //每秒检测一次播放完毕则销毁对象
        StartCoroutine(DestoryWhenFinished());
        IEnumerator DestoryWhenFinished()
        {
            do 
            {
                yield return new WaitForSeconds(1f);
                if (go == null || audioSource == null) yield break;
            }
            while (audioSource!=null&&audioSource.time>0);
            if (go != null) Destroy(go);
        }
    }
    #endregion
}
