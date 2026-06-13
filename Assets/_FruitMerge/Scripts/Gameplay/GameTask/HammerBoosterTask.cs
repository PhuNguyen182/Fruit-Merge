using System;
using System.Threading;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using _FruitMerge.Scripts.Input;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameTask
{
    public class HammerBoosterTask : IDisposable
    {
        private const string LogTag = "HammerBoosterTask"; 
        
        private readonly GameObject _hammerBoosterPrefab;
        private readonly InputController _inputController;
        private readonly IPublisher<CameraVibrateMessage> _cameraVibratePublisher;
        private readonly IPublisher<FruitBoosterReadyMessage> _fruitBoosterReadyPublisher;
        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        private FruitItem _pickedFruitItem;

        public HammerBoosterTask(InputController inputController, GameObject hammerBoosterPrefab)
        {
            this._inputController = inputController;
            this._hammerBoosterPrefab = hammerBoosterPrefab;
            this._cameraVibratePublisher = GlobalMessagePipe.GetPublisher<CameraVibrateMessage>();
            this._fruitBoosterReadyPublisher = GlobalMessagePipe.GetPublisher<FruitBoosterReadyMessage>();
            this._cancellationTokenSource = new CancellationTokenSource();
            this._cancellationToken = this._cancellationTokenSource.Token;
        }

        public void ShowFruitBoosterOutline(bool boosterOutlineEnabled)
        {
            this._fruitBoosterReadyPublisher.Publish(new FruitBoosterReadyMessage
            {
                BoosterReady = boosterOutlineEnabled
            });
        }

        public void PickFruitItem(FruitItem fruitItem)
        {
            this._pickedFruitItem = fruitItem;
        }

        public async UniTask ExecuteBooter()
        {
            if (!this._pickedFruitItem)
            {
                Debug.LogError($"[{LogTag}] Picked Fruit Item is missing. Please select a valid Fruit Item.");
                return;
            }
            
            this._inputController.IsInputActive = false;
            this.ShowFruitBoosterOutline(false);
            await UniTask.Delay(TimeSpan.FromSeconds(1.05f), cancellationToken: this._cancellationToken);
            Transform fruitParent = this._pickedFruitItem.transform.parent;
            GameObjectPoolManager.SpawnInstance(this._hammerBoosterPrefab, 
                this._pickedFruitItem.transform.position, Quaternion.identity, fruitParent);
            
            this._pickedFruitItem.ForceBreakFruit();
            this._cameraVibratePublisher.Publish(new CameraVibrateMessage
            {
                Duration = 0.2f,
                Amplitude = 6,
                Frequency = 6,
                ImpulseVelocity = new Vector3(-0.15f, 0.15f, 0.5f),
            });
            
            this._pickedFruitItem = null;
            this._inputController.IsInputActive = true;
        }

        public void Dispose()
        {
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource?.Dispose();
        }
    }
}
