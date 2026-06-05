using System.Collections.Generic;
using Probability;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.SpawnRules
{
    [CreateAssetMenu(fileName = "FruitSpawnRuleCollection", menuName = "Scriptable Objects/FruitMerge/FruitSpawnRuleCollection")]
    public class FruitSpawnRuleCollection : ScriptableObject
    {
        [SerializeField] private List<float> probabilityConfig;
        
        [Header("Fruit Spawn Rules By Difficulty")]
        [SerializeField] private FruitSpawnRuleConfig easyModeConfig;
        [SerializeField] private FruitSpawnRuleConfig mediumModeConfig;
        [SerializeField] private FruitSpawnRuleConfig hardModeConfig;
        
        public FruitSpawnRuleConfig GetRandomConfig()
        {
            int randomIndex = ProbabilitiesController.GetItemByProbability(this.probabilityConfig);
            FruitSpawnRuleConfig result = randomIndex switch
            {
                0 => this.easyModeConfig,
                1 => this.mediumModeConfig,
                2 => this.hardModeConfig,
                _ => this.easyModeConfig
            };
            
            return result;
        }
    }
}
