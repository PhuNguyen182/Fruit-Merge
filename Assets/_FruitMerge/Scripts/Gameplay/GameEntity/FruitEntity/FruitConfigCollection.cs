using System.Collections.Generic;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    [CreateAssetMenu(fileName = "FruitConfigCollection", menuName = "Scriptable Objects/FruitMerge/FruitConfigCollection")]
    public class FruitConfigCollection : ScriptableObject
    {
        [SerializeField] private List<FruitConfig> fruitConfigs = new();
        
        private int _maxFruitLevel = -1;

        public int GetMaxFruitLevel()
        {
            if (this._maxFruitLevel != -1) 
                return this._maxFruitLevel;
            
            FruitConfig lastFruitConfig = this.fruitConfigs[^1];
            this._maxFruitLevel = lastFruitConfig?.fruitId ?? -1;
            return this._maxFruitLevel;
        }

        public FruitConfig GetFruitConfigById(int fruitId)
        {
            int count = fruitConfigs.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.fruitConfigs[i].fruitId == fruitId)
                    return this.fruitConfigs[i];
            }
            
            return null;
        }
    }
}
