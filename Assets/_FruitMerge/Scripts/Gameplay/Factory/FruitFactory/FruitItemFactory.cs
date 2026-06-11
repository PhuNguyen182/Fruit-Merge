using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using DracoRuan.CoreSystems.DesignPatterns.Factory;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.Factory.FruitFactory
{
    public class FruitItemFactory : BaseFactory<FruitItemParam, FruitItem>
    {
        private const string LogTag = "FruitItemFactory";
        private const int PreloadCountForFruitPrefab = 30;
        
        private readonly FruitItem _fruitItemPrefab;
        private readonly Transform _fruitItemParent;
        private readonly FruitConfigCollection _fruitConfigCollection;
        private readonly int _maxFruitLevel;
        
        public Transform FruitItemParent => this._fruitItemParent;

        public FruitItemFactory(FruitItem prefab, Transform fruitItemParent,
            FruitConfigCollection fruitConfigCollection)
        {
            this._fruitItemPrefab = prefab;
            this._fruitItemParent = fruitItemParent;
            this._fruitConfigCollection = fruitConfigCollection;
            this._maxFruitLevel = this._fruitConfigCollection.GetMaxFruitLevel();
            
            GameObjectPoolManager.PoolPreLoad(this._fruitItemPrefab.gameObject, 
                PreloadCountForFruitPrefab, this._fruitItemParent);
        }

        public override FruitItem Create(FruitItemParam arg)
        {
            if (!this._fruitItemPrefab)
            {
                Debug.LogError($"[{LogTag}] Fruit item prefab cannot be null or empty!");
                return null;
            }
            
            FruitItem fruitItem = GameObjectPoolManager.SpawnInstance(this._fruitItemPrefab,
                arg.Position, Quaternion.identity, this._fruitItemParent);
            
            int fruitId = arg.FruitID;
            FruitConfig fruitConfig = this._fruitConfigCollection.GetFruitConfigById(fruitId);
            fruitItem.SetMaxFruitLevel(this._maxFruitLevel);
            fruitItem.SetFruitPhysicsActive(false);
            fruitItem.SetFruitColliderActive(false);
            fruitItem.ApplyFruitConfig(fruitConfig);
            fruitItem.InitFruitFactory(this);
            return fruitItem;
        }
    }
}
