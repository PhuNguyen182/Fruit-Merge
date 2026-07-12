using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using DracoRuan.CoreSystems.DesignPatterns.Factory;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.Factory.FruitFactory
{
    public class FruitItemFactory : BaseFactory<FruitItemParam, FruitItem>
    {
        private readonly Transform _fruitItemParent;
        private readonly FruitConfigCollection _fruitConfigCollection;
        private readonly int _maxFruitLevel;
        
        public Transform FruitItemParent => this._fruitItemParent;

        public FruitItemFactory(Transform fruitItemParent, FruitConfigCollection fruitConfigCollection)
        {
            this._fruitItemParent = fruitItemParent;
            this._fruitConfigCollection = fruitConfigCollection;
            this._maxFruitLevel = this._fruitConfigCollection.GetMaxFruitLevel();
        }

        public override FruitItem Create(FruitItemParam arg)
        {
            FruitItem fruitItem = GameObjectPoolManager.SpawnInstance(arg.Prefab,
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
