using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using DracoRuan.CoreSystems.DesignPatterns.Factory;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.Factory.FruitFactory
{
    public class FruitItemFactory : BaseFactory<FruitItemParam, FruitItem>
    {
        private const string LogTag = "FruitItemFactory";
        
        private readonly FruitItem _fruitItemPrefab;
        private readonly Transform _fruitItemParent;
        private readonly Transform _fruitItemSpawnPoint;
        private readonly FruitConfigCollection _fruitConfigCollection;

        public FruitItemFactory(FruitItem prefab, 
            Transform fruitItemParent, Transform fruitItemSpawnPoint,
            FruitConfigCollection fruitConfigCollection)
        {
            this._fruitItemPrefab = prefab;
            this._fruitItemParent = fruitItemParent;
            this._fruitItemSpawnPoint = fruitItemSpawnPoint;
            this._fruitConfigCollection = fruitConfigCollection;
        }

        public override FruitItem Create(FruitItemParam arg)
        {
            if (!this._fruitItemPrefab)
            {
                Debug.LogError($"[{LogTag}] Fruit item prefab cannot be null or empty!");
                return null;
            }
            
            if (!this._fruitItemSpawnPoint)
            {
                Debug.LogError($"[{LogTag}] Invalid spawn point!");
                return null;
            }

            FruitItem fruitItem = GameObjectPoolManager.SpawnInstance(this._fruitItemPrefab,
                this._fruitItemSpawnPoint.position, Quaternion.identity, this._fruitItemParent);
            
            int fruitId = arg.FruitID;
            FruitConfig fruitConfig = this._fruitConfigCollection.GetFruitConfigById(fruitId);
            fruitItem.SetFruitPhysicsActive(false);
            fruitItem.ApplyFruitConfig(fruitConfig);
            return fruitItem;
        }
    }
}
