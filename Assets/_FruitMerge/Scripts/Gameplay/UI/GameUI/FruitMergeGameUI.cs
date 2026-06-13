using _FruitMerge.Scripts.Gameplay.GameManagement.ScoreCalculator;
using _FruitMerge.Scripts.Gameplay.GameTask.BoosterTasks;
using ServiceLocators.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _FruitMerge.Scripts.Gameplay.UI.GameUI
{
    public class FruitMergeGameUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image nextFruitIcon;
        [SerializeField] private Button settingButton;
        [SerializeField] private BoosterUIButton hammerBoosterButton;

        private ScoreCalculationService _scoreCalculationService;
        private BoosterControllerTask _boosterControllerTask;
        
        private void Awake()
        {
            this.RegisterServices();
        }

        private void RegisterServices()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
            this._boosterControllerTask = ServiceLocator.ForSceneOf(this).Get<BoosterControllerTask>();
            this._scoreCalculationService = ServiceLocator.ForSceneOf(this).Get<ScoreCalculationService>();
            this._scoreCalculationService.OnFruitScoreUpdated += this.UpdateScore;
            this._scoreCalculationService.UpdateCurrentScore();
            this.hammerBoosterButton.OnBoosterClick += this.ExecuteHammerBooster;
        }

        private void UpdateScore(int score)
        {
            this.scoreText.text = $"{score}";
        }

        private void ExecuteHammerBooster()
        {
            this._boosterControllerTask.HammerBoosterTask.SetBoosterAvailable(true);
            this._boosterControllerTask.HammerBoosterTask.ShowFruitBoosterOutline(true);
        }

        public void UpdateNExtFruitIcon(Sprite fruitSprite)
        {
            this.nextFruitIcon.sprite = fruitSprite;
        }

        private void OnDestroy()
        {
            this._scoreCalculationService.OnFruitScoreUpdated -= this.UpdateScore;
            this.hammerBoosterButton.OnBoosterClick -= this.ExecuteHammerBooster;
        }
    }
}
