using Constants;
using GlobalScripts.Audios;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _FruitMerge.Scripts.Gameplay.UI.Popups
{
    public class LosePopup : MonoBehaviour
    {
        [SerializeField] private Button replayButton;
        [SerializeField] private AudioClip loseSound;

        private void Awake()
        {
            this.replayButton.onClick.AddListener(this.Replay);
        }

        private void OnEnable()
        {
            AudioManager.Instance.PlayFloatingAudio(this.loseSound);
        }

        private void Replay()
        {
            SceneManager.LoadSceneAsync(SceneName.LoadingScene, LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            this.replayButton.onClick.RemoveListener(this.Replay);
        }
    }
}
