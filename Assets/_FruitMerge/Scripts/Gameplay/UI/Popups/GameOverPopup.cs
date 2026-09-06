using System.Collections.Generic;
using Constants;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _FruitMerge.Scripts.Gameplay.GameManagement;
using _FruitMerge.Scripts.Gameplay.UI.Misc;
using TMPro;
using Random = UnityEngine.Random;

namespace _FruitMerge.Scripts.Gameplay.UI.Popups
{
    public class GameOverPopup : MonoBehaviour
    {
        [SerializeField] private Button normalReplayButton;
        [SerializeField] private Button randomReplayButton;
        [SerializeField] private List<TMP_Text> playerRanks;
        [SerializeField] private FakeNameCollection fakeNames;

        private void Awake()
        {
            this.normalReplayButton.onClick.AddListener(this.NormalReplay);
            this.randomReplayButton.onClick.AddListener(this.RandomReplay);
        }

        private void OnEnable()
        {
            this.ShowFakeRankers();
        }

        private void ShowFakeRankers()
        {
            int count = this.playerRanks.Count;
            for (int i = 0; i < count; i++)
            {
                int randomNumber = Random.Range(1, 1000);
                string rankerName = this.fakeNames.GetRandomName();
                this.playerRanks[i].text = $"{rankerName}{randomNumber}";
            }
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
