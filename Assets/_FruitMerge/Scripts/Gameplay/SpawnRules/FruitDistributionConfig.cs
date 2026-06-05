using System;
using System.Collections.Generic;

namespace _FruitMerge.Scripts.Gameplay.SpawnRules
{
    [Serializable]
    public class FruitDistributionConfig
    {
        public int fruitLevel;
        public List<float> probabilities = new();
        public List<int> fruitIds = new();
    }
}