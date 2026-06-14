using System;
using System.Threading;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using _FruitMerge.Scripts.Gameplay.GameManagement;
using _FruitMerge.Scripts.Input;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameTask.BoosterTasks
{
    public class HammerBoosterTask : IDisposable
    {
        private const string LogTag = "HammerBoosterTask"; 
        
        private readonly LayerMask _fruitLayerMask;
        private readonly GameObject _hammerBoosterPrefab;
        private readonly InputController _inputController;
        private readonly IPublisher<CameraVibrateMessage> _cameraVibratePublisher;
        private readonly IPublisher<FruitBoosterReadyMessage> _fruitBoosterReadyPublisher;
        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly FruitMemory _fruitMemory;
        
        private FruitItem _pickedFruitItem;
        private bool _boosterAvailable;

        public HammerBoosterTask(InputController inputController, GameObject hammerBoosterPrefab,
            LayerMask fruitLayerMask, FruitMemory fruitMemory)
        {
            this._fruitMemory = fruitMemory;
            this._fruitLayerMask = fruitLayerMask;
            this._inputController = inputController;
            this._hammerBoosterPrefab = hammerBoosterPrefab;
            this._cameraVibratePublisher = GlobalMessagePipe.GetPublisher<CameraVibrateMessage>();
            this._fruitBoosterReadyPublisher = GlobalMessagePipe.GetPublisher<FruitBoosterReadyMessage>();
            this._cancellationTokenSource = new CancellationTokenSource();
            this._cancellationToken = this._cancellationTokenSource.Token;

            this._inputController.OnPointerDown += this.SelectBoosterOnPointerDown;
        }

        private void SelectBoosterOnPointerDown()
        {
            if (!this._boosterAvailable)
                return;

            Vector3 pointerPosition = this._inputController.WorldPointerPosition;
            Collider2D fruitCollider = Physics2D.OverlapCircle(pointerPosition, 0.1f, this._fruitLayerMask);
            if (fruitCollider && fruitCollider.TryGetComponent(out FruitItem fruitItem))
            {
                this.PickFruitItem(fruitItem);
                this.ExecuteBooter().Forget();
            }
            else
            {
                this.ShowFruitBoosterOutline(false);
            }
            
            this.SetBoosterAvailable(false);
        }
        
        public void SetBoosterAvailable(bool isAvailable)
        {
            if (!this._fruitMemory.HasAnyFruit)
                return;
            
            this._boosterAvailable = isAvailable;
        }

        public void ShowFruitBoosterOutline(bool boosterOutlineEnabled)
        {
            if (!this._fruitMemory.HasAnyFruit)
                return;
            
            this._fruitBoosterReadyPublisher.Publish(new FruitBoosterReadyMessage
            {
                BoosterReady = boosterOutlineEnabled
            });
        }

        private void PickFruitItem(FruitItem fruitItem)
        {
            this._pickedFruitItem = fruitItem;
        }

        private async UniTask ExecuteBooter()
        {
            if (!this._pickedFruitItem)
            {
                Debug.LogError($"[{LogTag}] Picked Fruit Item is missing. Please select a valid Fruit Item.");
                return;
            }
            
            this._inputController.IsInputActive = false;
            this.ShowFruitBoosterOutline(false);
            Transform fruitParent = this._pickedFruitItem.transform.parent;
            GameObjectPoolManager.SpawnInstance(this._hammerBoosterPrefab, 
                this._pickedFruitItem.transform.position, Quaternion.identity, fruitParent);
            
            this._pickedFruitItem.StopFruitPhysics();
            await UniTask.Delay(TimeSpan.FromSeconds(1.05f), cancellationToken: this._cancellationToken);
            this._pickedFruitItem.ForceBreakFruit();
            this._cameraVibratePublisher.Publish(new CameraVibrateMessage
            {
                Duration = 0.2f,
                Amplitude = 6,
                Frequency = 6,
                ImpulseVelocity = new Vector3(-0.15f, 0.15f, 0.5f),
            });
            
            this._pickedFruitItem = null;
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this._cancellationToken);
            this._inputController.IsInputActive = true;
        }

        public void Dispose()
        {
            this._inputController.OnPointerDown -= this.SelectBoosterOnPointerDown;
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource?.Dispose();
        }
    }
}
