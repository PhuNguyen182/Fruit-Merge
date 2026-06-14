using System;
using _FruitMerge.Scripts.Gameplay.Factory.FruitFactory;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using _FruitMerge.Scripts.Gameplay.SpawnRules;
using _FruitMerge.Scripts.Gameplay.UI.GameUI;
using ServiceLocators.Core;
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

        private bool _hasSpawnedFruit;
        private int _currentFruitId = -1;
        private int _nextFruitId = -1;
        
        private ISubscriber<FruitReleaseMessage> _fruitReleaseMessageSubscriber;
        private ISubscriber<FruitSpawnMessage> _fruitSpawnMessageSubscriber;

        private FruitMergeGameUI _gameUI;
        private FruitMemory _fruitMemory;
        private FruitItemFactory _fruitItemFactory;
        private FruitSpawnRuleConfig _fruitSpawnRuleConfig;
        private IDisposable _disposable;
        
        public FruitMemory FruitMemory => this._fruitMemory;

        public void InitializeFruitSpawner()
        {
            this._gameUI = ServiceLocator.ForSceneOf(this).Get<FruitMergeGameUI>();
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
            if (!this._hasSpawnedFruit)
            {
                this._hasSpawnedFruit = true;
                this._currentFruitId = this.GetRandomFruitId();
            }
            else
            {
                this._currentFruitId = this._nextFruitId;
            }

            this._nextFruitId = this.GetRandomFruitId();
            FruitItem fruitItem = this._fruitItemFactory.Create(new FruitItemParam
            {
                FruitID = this._currentFruitId,
                Position = position,
            });

            var nextFruitConfig = this.fruitConfigCollection.GetFruitConfigById(this._nextFruitId);
            if (nextFruitConfig && this._gameUI)
                this._gameUI.UpdateNextFruitIcon(nextFruitConfig.fruitIcon);
            
            return fruitItem;
        }

        private int GetRandomFruitId()
        {
            int currentFruitLevel = this._fruitMemory.GetCurrentFruitLevel();
            FruitDistributionConfig fruitDistribution =
                this._fruitSpawnRuleConfig.GetFruitDistributionByFruitLevel(currentFruitLevel);
            int distributedFruitIndex = ProbabilitiesController.GetItemByProbability(fruitDistribution.probabilities);
            int fruitId = fruitDistribution.fruitIds[distributedFruitIndex];
            return fruitId;
        }

        private void OnDestroy()
        {
            this._fruitMemory?.ClearFruit();
            this._disposable?.Dispose();
        }
    }
}
