using UnityEngine;

namespace Audios
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AutoDespawn))]
    public class FloatingAudioComponent : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AutoDespawn autoDespawn;

        public void SetLoop(bool loop)
        {
            this.audioSource.loop = loop;
        }

        public void SetVolume(float volume)
        {
            this.audioSource.volume = volume;
        }

        public void SetAudioClip(AudioClip audioClip)
        {
            this.audioSource.clip = audioClip;
            this.autoDespawn.SetDuration(audioClip.length + 0.1f);
        }

        public void PlayAudio() => this.audioSource.Play();

        public void StopAudio() => this.audioSource.Stop();

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.audioSource ??= this.GetComponent<AudioSource>();
            this.autoDespawn ??= this.GetComponent<AutoDespawn>();
        }
#endif
    }
}
