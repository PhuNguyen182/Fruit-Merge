using System.Collections.Generic;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.SpawnRules
{
    [CreateAssetMenu(fileName = "FruitSpawnRuleConfig", menuName = "Scriptable Objects/FruitMerge/FruitSpawnRuleConfig")]
    public class FruitSpawnRuleConfig : ScriptableObject
    {
        [SerializeField] private List<FruitDistributionConfig> fruitDistributionConfigs = new();

        public FruitDistributionConfig GetFruitDistributionByFruitLevel(int currentFruitLevel)
        {
            int count = this.fruitDistributionConfigs.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.fruitDistributionConfigs[i].fruitLevel == currentFruitLevel)
                    return this.fruitDistributionConfigs[i];
            }

            return null;
        }
    }
}
