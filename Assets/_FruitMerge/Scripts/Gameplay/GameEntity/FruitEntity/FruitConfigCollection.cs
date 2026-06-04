using System.Collections.Generic;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    [CreateAssetMenu(fileName = "FruitConfigCollection", menuName = "Scriptable Objects/FruitMerge/FruitConfigCollection")]
    public class FruitConfigCollection : ScriptableObject
    {
        [SerializeField] private List<FruitConfig> fruitConfigs = new();

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
