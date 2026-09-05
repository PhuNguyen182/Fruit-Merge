using Constants;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _FruitMerge.Scripts.Gameplay.GameManagement;

namespace _FruitMerge.Scripts.Gameplay.UI.Popups
{
    public class GameOverPopup : MonoBehaviour
    {
        [SerializeField] private Button normalReplayButton;
        [SerializeField] private Button randomReplayButton;

        private void Awake()
        {
            this.normalReplayButton.onClick.AddListener(this.NormalReplay);
            this.randomReplayButton.onClick.AddListener(this.RandomReplay);
        }

        private void NormalReplay()
        {
            GameUtils.ShouldUseRandomTheme = false;
            SceneManager.LoadSceneAsync(SceneName.LoadingScene, LoadSceneMode.Single);
        }

        private void RandomReplay()
        {
            GameUtils.ShouldUseRandomTheme = true;
            SceneManager.LoadSceneAsync(SceneName.LoadingScene, LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            this.normalReplayButton.onClick.RemoveListener(this.NormalReplay);
            this.randomReplayButton.onClick.RemoveListener(this.RandomReplay);
        }
    }
}
