using System;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using MessagePipe;

namespace _FruitMerge.Scripts.Gameplay.GameManagement.ScoreCalculator
{
    public class ScoreCalculationService : IDisposable
    {
        private readonly GameScoreProgressionDataController _gameScoreProgressionDataController;
        private readonly IDisposable _disposable;

        public event Action<int> OnFruitScoreUpdated; 
        public int CurrentFruitScore { get; private set; }

        public ScoreCalculationService(GameScoreProgressionDataController gameScoreProgressionDataController)
        {
            this._gameScoreProgressionDataController = gameScoreProgressionDataController;
            var builder = DisposableBag.CreateBuilder();
            var addFruitScoreMessageSubscriber = GlobalMessagePipe.GetSubscriber<AddFruitScoreMessage>();
            addFruitScoreMessageSubscriber.Subscribe(this.OnFruitAddScoreMessageReceived).AddTo(builder);
            this._disposable = builder.Build();
        }

        private void OnFruitAddScoreMessageReceived(AddFruitScoreMessage message)
        {
            this.CurrentFruitScore += message.FruitScore;
            this._gameScoreProgressionDataController.TrySaveHighestScore(this.CurrentFruitScore);
            this.OnFruitScoreUpdated?.Invoke(this.CurrentFruitScore);
        }
        
        public int GetCurrentFruitHighestScore()
        {
            int result = this._gameScoreProgressionDataController.GetCurrentHighScore();
            return result;
        }

        public void Dispose()
        {
            this._disposable?.Dispose();
        }
    }
}
