using System.Linq;
using System.Collections.Generic;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitMemory
    {
        private readonly Dictionary<int, FruitItem> _fruitItemMemory = new();

        public void AddFruit(FruitItem fruitItem)
        {
            int fruitInstanceId = fruitItem.gameObject.GetInstanceID();
            this._fruitItemMemory.TryAdd(fruitInstanceId, fruitItem);
        }

        public void RemoveFruit(int fruitInstanceId)
        {
            this._fruitItemMemory.Remove(fruitInstanceId);
        }

        public int GetCurrentFruitLevel()
        {
            int currentFruitLevel = int.MinValue;
            if (this._fruitItemMemory.Count <= 0)
            {
                currentFruitLevel = 1;
            }
            else
            {
                foreach (FruitItem fruitItem in this._fruitItemMemory.Values)
                {
                    if (currentFruitLevel < fruitItem.FruitID)
                        currentFruitLevel = fruitItem.FruitID;
                }    
            }
            
            return currentFruitLevel;
        }

        public FruitItem GetLatestFruit()
        {
            var (_, fruitItem) = this._fruitItemMemory.LastOrDefault();
            return fruitItem;
        }
        
        public void ClearFruit() => this._fruitItemMemory.Clear();
    }
}
