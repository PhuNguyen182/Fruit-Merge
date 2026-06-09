using System;
using System.Collections.Generic;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using DracoRuan.CoreSystems.PlayerLoopSystem.UpdateServices;
using MessagePipe;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitDeadline : MonoBehaviour, IUpdateHandler
    {
        [SerializeField] private FruitDeadlineConfig deadlineConfig;
        
        private HashSet<int> _fruitInstanceIds;
        private ISubscriber<FruitDeadlineCollideMessage> _fruitDeadlineCollideMessageSubscriber;
        private IDisposable _disposable;

        private bool _isTimeout;
        private float _deadlineDuration;
        
        public event Action OnFruitDeadlineTimeOut;

        private void Awake()
        {
            this._fruitInstanceIds = new HashSet<int>(30);
        }

        private void OnEnable()
        {
            UpdateServiceManager.RegisterUpdateHandler(this);
        }

        public void InitializeFruitDeadline()
        {
            var builder = DisposableBag.CreateBuilder();
            this._fruitDeadlineCollideMessageSubscriber = GlobalMessagePipe.GetSubscriber<FruitDeadlineCollideMessage>();
            this._fruitDeadlineCollideMessageSubscriber.Subscribe(this.OnFruitCollideMessageReceived).AddTo(builder);
            this._disposable = builder.Build();
        }

        private void OnFruitCollideMessageReceived(FruitDeadlineCollideMessage fruitDeadlineCollideMessage)
        {
            if (fruitDeadlineCollideMessage.IsCollided)
                this._fruitInstanceIds.Add(fruitDeadlineCollideMessage.FruitEntityId);
            else
                this._fruitInstanceIds.Remove(fruitDeadlineCollideMessage.FruitEntityId);
        }

        public void Tick(float deltaTime)
        {
            if (this._isTimeout)
                return;

            if (this._fruitInstanceIds.Count > 0)
            {
                this._deadlineDuration += deltaTime;
                if (this._deadlineDuration < this.deadlineConfig.deadlineDuration)
                    return;

                this._isTimeout = true;
                this.OnFruitDeadlineTimeOut?.Invoke();
            }
            else
            {
                this._deadlineDuration = 0;
            }
        }

        private void OnDisable()
        {
            UpdateServiceManager.DeregisterUpdateHandler(this);
        }

        private void OnDestroy()
        {
            this._fruitInstanceIds.Clear();
            this._disposable?.Dispose();
        }
    }
}
