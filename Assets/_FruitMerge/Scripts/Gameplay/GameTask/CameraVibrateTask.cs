using System;
using _FruitMerge.Scripts.Gameplay.GameEntity.Camera;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using MessagePipe;

namespace _FruitMerge.Scripts.Gameplay.GameTask
{
    public class CameraVibrateTask : IDisposable
    {
        private readonly CameraShakeController _cameraShakeController;
        private readonly IDisposable _disposable;
        
        public CameraVibrateTask(CameraShakeController cameraShakeController)
        {
            this._cameraShakeController = cameraShakeController;
            var builder = DisposableBag.CreateBuilder();
            var cameraVibrateSubscriber = GlobalMessagePipe.GetSubscriber<CameraVibrateMessage>();
            cameraVibrateSubscriber.Subscribe(this.OnCameraVibrateMessageReceived).AddTo(builder);
            this._disposable = builder.Build();
        }

        private void OnCameraVibrateMessageReceived(CameraVibrateMessage message)
        {
            this._cameraShakeController.VibrateCamera(message);
        }

        public void Dispose()
        {
            this._disposable?.Dispose();
        }
    }
}