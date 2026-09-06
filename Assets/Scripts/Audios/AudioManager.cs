using System;
using Audios;
using UnityEngine;
using UnityEngine.Audio;
using DracoRuan.CoreSystems.DesignPatterns.Singleton;

namespace GlobalScripts.Audios
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [SerializeField] private bool playSequence;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource itemSource;
        [SerializeField] private FloatingAudioComponent floatingAudioComponent;
        [SerializeField] private PlaySequenceMusic sequenceMusicPlayer;

        public static event Action<float> OnMasterChange;
        public static event Action<float> OnMusicChange;
        public static event Action<float> OnSoundChange;

        private const string MasterVolumeMixer = "MasterVolume";
        private const string MusicVolumeMixer = "MusicVolume";
        private const string SoundVolumeMixer = "SoundVolume";

        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SoundVolumeKey = "SoundVolume";

        public float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MasterVolumeKey, 1);
            set
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, value);
                OnMasterChange?.Invoke(value);
            }
        }

        public float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 1);
            set
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, value);
                OnMusicChange?.Invoke(value);
            }
        }

        public float SoundVolume
        {
            get => PlayerPrefs.GetFloat(SoundVolumeKey, 1);
            set
            {
                PlayerPrefs.SetFloat(SoundVolumeKey, value);
                OnSoundChange?.Invoke(value);
            }
        }

        protected override void OnAwake()
        {
            
        }

        private void Start()
        {
            OnMasterChange += AdjustMasterVolume;
            OnMusicChange += AdjustMusicVolume;
            OnSoundChange += AdjustSoundVolume;

            MasterVolume += 0;
            MusicVolume += 0;
            SoundVolume += 0;
        }

        public bool IsMusicPlaying()
        {
            return musicSource.isPlaying;
        }

        public void PlayMusic(AudioClip musicClip, bool loop = false, float volume = 1f)
        {
            if (musicClip == null)
                return;

            musicSource.Stop();
            musicSource.loop = loop;
            musicSource.clip = musicClip;
            musicSource.volume = volume;
            musicSource.Play();
        }

        public void PlayItemSound(AudioClip soundClip, float volumeScale = 1, bool loop = false)
        {
            if (soundClip == null || sfxSource == null)
                return;

            itemSource.loop = loop;
            itemSource.PlayOneShot(soundClip, volumeScale);
        }

        public void PlaySoundEffect(AudioClip soundClip, float volumeScale = 1, bool loop = false)
        {
            if (soundClip == null || sfxSource == null)
                return;

            sfxSource.loop = loop;
            sfxSource.PlayOneShot(soundClip, volumeScale);
        }

        public void PlayFloatingAudio(AudioClip audioClip, float volumeScale = 1, bool loop = false)
        {
            if (floatingAudioComponent == null)
                return;

            FloatingAudioComponent floatingAudio = GameObjectPoolManager.SpawnInstance(floatingAudioComponent,
                Vector3.zero, Quaternion.identity, SoundEffectContainer.Transform);
            floatingAudio.SetAudioClip(audioClip);
            floatingAudio.SetVolume(volumeScale);
            floatingAudio.SetLoop(loop);
            floatingAudio.PlayAudio();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        private void AdjustMasterVolume(float value)
        {
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(MasterVolumeMixer, volume);
        }

        private void AdjustMusicVolume(float value)
        {
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(MusicVolumeMixer, volume);
        }

        private void AdjustSoundVolume(float value)
        {
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(SoundVolumeMixer, volume);
        }

        private void OnDestroy()
        {
            OnMasterChange -= AdjustMasterVolume;
            OnMusicChange -= AdjustMusicVolume;
            OnSoundChange -= AdjustSoundVolume;
        }
    }
}
