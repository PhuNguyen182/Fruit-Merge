using System;
using _FruitMerge.Scripts.Gameplay.Factory.FruitFactory;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitTheme;
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
        private const string DefaultTheme = "theme_0";
        
        [SerializeField] private Transform fruitParent;
        [SerializeField] private FruitSpawnRuleCollection fruitSpawnRuleCollection;
        [SerializeField] private FruitConfigCollection fruitConfigCollection;
        [SerializeField] private FruitThemeCollection fruitThemeCollection;

        private bool _hasSpawnedFruit;
        private int _currentFruitId = -1;
        private int _nextFruitId = -1;
        
        private ISubscriber<FruitReleaseMessage> _fruitReleaseMessageSubscriber;
        private ISubscriber<FruitSpawnMessage> _fruitSpawnMessageSubscriber;

        private FruitMergeGameUI _gameUI;
        private FruitMemory _fruitMemory;
        private FruitThemeConfig _fruitThemeConfig;
        private FruitItemFactory _fruitItemFactory;
        private FruitSpawnRuleConfig _fruitSpawnRuleConfig;
        private IDisposable _disposable;
        
        public FruitMemory FruitMemory => this._fruitMemory;

        public void InitializeFruitSpawner()
        {
            this._fruitThemeConfig = this.fruitThemeCollection.GetThemeConfigByName(DefaultTheme);
            this._gameUI = ServiceLocator.ForSceneOf(this).Get<FruitMergeGameUI>();
            this._gameUI.InitFruitProgressionIcon(this._fruitThemeConfig.FruitProgressIcons);
            
            this._fruitMemory = new FruitMemory();
            this._fruitItemFactory =
                new FruitItemFactory(this.fruitParent, this.fruitConfigCollection);
            this._fruitSpawnRuleConfig = this.fruitSpawnRuleCollection.GetRandomConfig();

            var builder = DisposableBag.CreateBuilder();
            this._fruitReleaseMessageSubscriber = GlobalMessagePipe.GetSubscriber<FruitReleaseMessage>();
            this._fruitReleaseMessageSubscriber.Subscribe(this.OnFruitRelease).AddTo(builder);

            this._fruitSpawnMessageSubscriber = GlobalMessagePipe.GetSubscriber<FruitSpawnMessage>();
            this._fruitSpawnMessageSubscriber.Subscribe(this.OnFruitSpawn).AddTo(builder);

            this._disposable = builder.Build();
            this.PreloadFruit();
        }

        private void PreloadFruit()
        {
            for (int i = 1; i <= 5; i++)
            {
                FruitItem prefab = this._fruitThemeConfig.GetFruitById(i);
                int quantity = i >= 4 ? 10 : 20;
                GameObjectPoolManager.PoolPreLoad(prefab, quantity, this.fruitParent);
            }
        }

        private void OnFruitRelease(FruitReleaseMessage fruitReleaseMessage)
        {
            this._fruitMemory.RemoveFruit(fruitReleaseMessage.FruitInstanceID);
        }

        private void OnFruitSpawn(FruitSpawnMessage fruitSpawnMessage)
        {
            this._fruitMemory.AddFruit(fruitSpawnMessage.FruitItem);
        }

        public void UpdateFruitBarView()
        {
            int currentFruitLevel = this._fruitMemory.GetCurrentFruitLevel();
            this._gameUI.UpdateFruitProgressView(currentFruitLevel);
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
            FruitItem fruitPrefab = this._fruitThemeConfig.GetFruitById(this._currentFruitId);
            FruitItem fruitItem = this._fruitItemFactory.Create(new FruitItemParam
            {
                FruitID = this._currentFruitId,
                Position = position,
                Prefab = fruitPrefab,
            });
            
            this.UpdateFruitBarView();
            fruitItem.InitFruitThemeConfig(this._fruitThemeConfig);
            var fruitIcon = this._fruitThemeConfig.GetFruitProgressIconById(this._nextFruitId);
            if (fruitIcon != null && this._gameUI)
                this._gameUI.UpdateNextFruitIcon(fruitIcon.openIcon);
            
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
