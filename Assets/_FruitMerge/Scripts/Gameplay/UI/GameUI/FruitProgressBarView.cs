using System.Collections.Generic;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitTheme;
using UnityEngine;
using UnityEngine.UI;

namespace _FruitMerge.Scripts.Gameplay.UI.GameUI
{
    public class FruitProgressBarView : MonoBehaviour
    {
        [SerializeField] private List<Image> fruitIcons;
        
        private List<FruitThemeConfig.FruitProgressIcon> _fruitProgressIcons;

        public void InitFruitProgressionIcon(List<FruitThemeConfig.FruitProgressIcon> fruitProgressIcons)
        {
            this._fruitProgressIcons = fruitProgressIcons;
            this.UpdateFruitProgressView(1);
        }

        public void UpdateFruitProgressView(int fruitLevel)
        {
            int count = this.fruitIcons.Count;
            for (int i = 0; i < count; i++)
            {
                int index = i + 1;
                FruitThemeConfig.FruitProgressIcon progressIcon = this._fruitProgressIcons[i];
                this.fruitIcons[i].sprite = index <= fruitLevel ? progressIcon.openIcon : progressIcon.lockIcon;
            }
        }
    }
}
