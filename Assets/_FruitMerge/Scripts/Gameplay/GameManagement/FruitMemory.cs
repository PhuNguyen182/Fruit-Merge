using System.Linq;
using System.Collections.Generic;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using Extensions;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitMemory
    {
        private readonly Dictionary<int, HashSet<int>> _fruitIdRecords = new();
        private readonly Dictionary<int, FruitItem> _fruitItemMemory = new();
        
        public bool HasAnyFruit => this._fruitItemMemory.Count > 0;

        public void AddFruit(FruitItem fruitItem)
        {
            int fruitInstanceId = fruitItem.gameObject.GetInstanceID();
            this._fruitItemMemory.TryAdd(fruitInstanceId, fruitItem);
            int fruitId = fruitItem.FruitID;

            if (this._fruitIdRecords.TryGetValue(fruitId, out HashSet<int> fruitIdRecord))
                fruitIdRecord.Add(fruitInstanceId);
            else
                this._fruitIdRecords.Add(fruitId, new HashSet<int> { fruitInstanceId });
        }

        public void RemoveFruit(int fruitInstanceId)
        {
            if (this._fruitItemMemory.TryGetValue(fruitInstanceId, out FruitItem fruitToRemove))
            {
                int fruitId = fruitToRemove.FruitID;
                if (this._fruitIdRecords.TryGetValue(fruitId, out HashSet<int> fruitIdRecord))
                    fruitIdRecord.Remove(fruitInstanceId);
            }

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

        public void ClearDuplicatedFruits()
        {
            if (this._fruitItemMemory.Count <= 0)
                return;
            
            HashSet<int> duplicatedFruitIds = new HashSet<int>();
            foreach (var kvp in this._fruitIdRecords)
            {
                if (kvp.Value.Count > 1)
                    duplicatedFruitIds.AddRange(kvp.Value);
            }
            
            if (duplicatedFruitIds.Count <= 0)
                return;

            foreach (int duplicateId in duplicatedFruitIds)
            {
                FruitItem duplicatedFruitItem = this._fruitItemMemory[duplicateId];
                duplicatedFruitItem.ReleaseImmediately();
            }
        }
        
        public void ClearFruit()
        {
            this._fruitItemMemory.Clear();
            this._fruitIdRecords.Clear();
        }
    }
}
