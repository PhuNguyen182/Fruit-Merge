using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using _FruitMerge.Scripts.Input;
using MessagePipe;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameTask
{
    public class HammerBoosterTask
    {
        private const string LogTag = "HammerBoosterTask"; 
        
        private readonly InputController _inputController;
        private readonly IPublisher<CameraVibrateMessage> _cameraVibratePublisher;
        private readonly IPublisher<FruitBoosterReadyMessage> _fruitBoosterReadyPublisher;
        private FruitItem _pickedFruitItem;

        public HammerBoosterTask(InputController inputController)
        {
            this._inputController = inputController;
            this._cameraVibratePublisher = GlobalMessagePipe.GetPublisher<CameraVibrateMessage>();
            this._fruitBoosterReadyPublisher = GlobalMessagePipe.GetPublisher<FruitBoosterReadyMessage>();
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

        public void ExecuteBooter()
        {
            if (!this._pickedFruitItem)
            {
                Debug.LogError($"[{LogTag}] Picked Fruit Item is missing. Please select a valid Fruit Item.");
                return;
            }
            
            this._inputController.IsInputActive = false;
            this.ShowFruitBoosterOutline(false);
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
    }
}
