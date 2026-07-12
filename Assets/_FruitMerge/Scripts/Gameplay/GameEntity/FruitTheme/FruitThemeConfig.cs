using System;
using System.Collections.Generic;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitTheme
{
    [CreateAssetMenu(fileName = "FruitThemeConfig", menuName = "Scriptable Objects/FruitMerge/FruitThemeConfig")]
    public class FruitThemeConfig : ScriptableObject
    {
        [Serializable]
        private class FruitItemConfig
        {
            public int fruitId;
            public FruitItem fruitPrefab;
        }
        
        [Serializable]
        public class FruitProgressIcon
        {
            public int fruitId;
            public Sprite lockIcon;
            public Sprite openIcon;
        }
        
        [SerializeField] public string themeName;
        [SerializeField] private FruitItemConfig[] fruitItems;
        [SerializeField] private List<FruitProgressIcon> fruitProgressIcons;
        
        public List<FruitProgressIcon> FruitProgressIcons => this.fruitProgressIcons;

        public FruitItem GetFruitById(int fruitId)
        {
            int count = this.fruitItems.Length;
            for (int i = 0; i < count; i++)
            {
                if (this.fruitItems[i].fruitId == fruitId)
                    return this.fruitItems[i].fruitPrefab;
            }
            
            return null;
        }

        public FruitProgressIcon GetFruitProgressIconById(int fruitId)
        {
            int count = this.fruitProgressIcons.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.fruitProgressIcons[i].fruitId == fruitId)
                    return this.fruitProgressIcons[i];
            }
            
            return null;
        }
    }
}
