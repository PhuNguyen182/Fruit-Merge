using System;
using _FruitMerge.Scripts.Gameplay.Factory.FruitFactory;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using _FruitMerge.Scripts.Gameplay.SpawnRules;
using MessagePipe;
using Probability;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitSpawner : MonoBehaviour
    {
        [SerializeField] private Transform fruitParent;
        [SerializeField] private FruitItem fruitItemPrefab;
        [SerializeField] private FruitSpawnRuleCollection fruitSpawnRuleCollection;
        [SerializeField] private FruitConfigCollection fruitConfigCollection;

        private ISubscriber<FruitReleaseMessage> _fruitReleaseMessageSubscriber;
        private ISubscriber<FruitSpawnMessage> _fruitSpawnMessageSubscriber;

        private FruitMemory _fruitMemory;
        private FruitItemFactory _fruitItemFactory;
        private FruitSpawnRuleConfig _fruitSpawnRuleConfig;
        private IDisposable _disposable;

        public void InitializeFruitSpawner()
        {
            this._fruitMemory = new FruitMemory();
            this._fruitItemFactory =
                new FruitItemFactory(this.fruitItemPrefab, this.fruitParent, this.fruitConfigCollection);
            this._fruitSpawnRuleConfig = this.fruitSpawnRuleCollection.GetRandomConfig();

            var builder = DisposableBag.CreateBuilder();
            this._fruitReleaseMessageSubscriber = GlobalMessagePipe.GetSubscriber<FruitReleaseMessage>();
            this._fruitReleaseMessageSubscriber.Subscribe(this.OnFruitRelease).AddTo(builder);

            this._fruitSpawnMessageSubscriber = GlobalMessagePipe.GetSubscriber<FruitSpawnMessage>();
            this._fruitSpawnMessageSubscriber.Subscribe(this.OnFruitSpawn).AddTo(builder);

            this._disposable = builder.Build();
        }

        private void OnFruitRelease(FruitReleaseMessage fruitReleaseMessage)
        {
            this._fruitMemory.RemoveFruit(fruitReleaseMessage.FruitInstanceID);
        }

        private void OnFruitSpawn(FruitSpawnMessage fruitSpawnMessage)
        {
            this._fruitMemory.AddFruit(fruitSpawnMessage.FruitItem);
        }

        public FruitItem SpawnNewFruit(Vector2 position)
        {
            int currentFruitLevel = this._fruitMemory.GetCurrentFruitLevel();
            FruitDistributionConfig fruitDistribution =
                this._fruitSpawnRuleConfig.GetFruitDistributionByFruitLevel(currentFruitLevel);
            int distributedFruitIndex = ProbabilitiesController.GetItemByProbability(fruitDistribution.probabilities);
            int fruitId = fruitDistribution.fruitIds[distributedFruitIndex];

            FruitItem fruitItem = this._fruitItemFactory.Create(new FruitItemParam
            {
                FruitID = fruitId,
                Position = position,
            });

            return fruitItem;
        }

        private void OnDestroy()
        {
            this._disposable?.Dispose();
        }
    }
}
