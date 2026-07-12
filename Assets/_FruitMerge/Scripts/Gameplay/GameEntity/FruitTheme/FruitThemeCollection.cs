using System.Collections.Generic;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitTheme
{
    [CreateAssetMenu(fileName = "FruitThemeCollection", menuName = "Scriptable Objects/FruitMerge/FruitThemeCollection")]
    public class FruitThemeCollection : ScriptableObject
    {
        [SerializeField] public List<FruitThemeConfig> fruitThemeConfigs;

        public FruitThemeConfig GetThemeConfigByName(string themeName)
        {
            int count = this.fruitThemeConfigs.Count;
            for (int i = 0; i < count; i++)
            {
                string themeConfigName = this.fruitThemeConfigs[i].themeName;
                if (string.CompareOrdinal(themeConfigName, themeName) == 0)
                    return this.fruitThemeConfigs[i];
            }
            
            return null;
        }
    }
}
