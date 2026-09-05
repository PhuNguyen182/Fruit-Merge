using _FruitMerge.Scripts.Gameplay.GameManagement;
using GlobalScripts.Audios;
using ServiceLocators.Core;
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

        private FruitMergeGameController _fruitMergeGameController;
        private FruitMemory _fruitMemory;

        private void Awake()
        {
            this.reviveButton.onClick.AddListener(this.Revive);
            this.replayButton.onClick.AddListener(this.ShowGameOverPopup);
            
            this._fruitMergeGameController = ServiceLocator.ForSceneOf(this).Get<FruitMergeGameController>(); 
            this._fruitMemory = this._fruitMergeGameController.FruitSpawner.FruitMemory;
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
            this._fruitMemory.ClearDuplicatedFruits();
            this._fruitMergeGameController.ContinueGame();
            this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            this.reviveButton.onClick.RemoveListener(this.Revive);
            this.replayButton.onClick.RemoveListener(this.ShowGameOverPopup);
        }
    }
}
