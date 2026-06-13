using System;
using _FruitMerge.Scripts.Gameplay.GameTask.BoosterTasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _FruitMerge.Scripts.Gameplay.UI.GameUI
{
    public class BoosterUIButton : MonoBehaviour
    {
        [SerializeField] private BoosterType boosterType;
        [SerializeField] private Button boosterButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text boosterCountText;
        [SerializeField] private GameObject adsObject;
        [SerializeField] private bool isAdsBooster;
        
        public BoosterType BoosterType => this.boosterType;
        public event Action OnBoosterClick;

        private void Awake()
        {
            this.boosterButton.onClick.AddListener(this.OnButtonClick);
        }

        private void OnButtonClick()
        {
            this.OnBoosterClick?.Invoke();
        }
        
        public void SetInteractable(bool interactable) => this.canvasGroup.interactable = interactable;

        private void OnDestroy()
        {
            this.boosterButton.onClick.RemoveListener(this.OnButtonClick);
        }
    }
}
