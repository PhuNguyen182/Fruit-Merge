using GlobalScripts.Audios;
using UnityEngine;
using UnityEngine.UI;

namespace _FruitMerge.Scripts.Gameplay.UI.Popups
{
    public class LosePopup : MonoBehaviour
    {
        [SerializeField] private Button reviveButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private GameObject gameOverPopup;

        private void Awake()
        {
            this.reviveButton.onClick.AddListener(this.Revive);
            this.replayButton.onClick.AddListener(this.ShowGameOverPopup);
        }

        private void OnEnable()
        {
            AudioManager.Instance.PlayFloatingAudio(this.loseSound);
        }

        private void ShowGameOverPopup()
        {
            this.gameOverPopup.SetActive(true);
            this.gameObject.SetActive(false);
        }

        private void Revive()
        {
            // TODO: clean all duplicated fruits
            this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            this.reviveButton.onClick.RemoveListener(this.Revive);
            this.replayButton.onClick.RemoveListener(this.ShowGameOverPopup);
        }
    }
}
