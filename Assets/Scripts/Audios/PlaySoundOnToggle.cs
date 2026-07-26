using UnityEngine;
using UnityEngine.UI;

namespace GlobalScripts.Audios
{
    [RequireComponent(typeof(Toggle))]
    public class PlaySoundOnToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private float volumeScale = 0.6f;
        [SerializeField] private AudioClip clip;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(isOn =>
            {
                AudioManager.Instance.PlaySoundEffect(clip, volumeScale);
            });
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            toggle ??= GetComponent<Toggle>();
        }
#endif
    }
}
