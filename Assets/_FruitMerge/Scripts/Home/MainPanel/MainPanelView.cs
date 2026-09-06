using Constants;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _FruitMerge.Scripts.Home.MainPanel
{
    public class MainPanelView : MonoBehaviour
    {
        [SerializeField] private Button playGameButton;

        private void Awake()
        {
            this.playGameButton.onClick.AddListener(this.PlayFruitMergeGame);
        }

        private void PlayFruitMergeGame()
        {
            SceneManager.LoadSceneAsync(SceneName.GameplayScene, LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            this.playGameButton.onClick.RemoveListener(this.PlayFruitMergeGame);
        }
    }
}
