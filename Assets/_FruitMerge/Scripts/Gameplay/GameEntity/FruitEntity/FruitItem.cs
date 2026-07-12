using System;
using System.Threading;
using _FruitMerge.Scripts.Gameplay.Factory.FruitFactory;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using Cysharp.Threading.Tasks;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using DracoRuan.CoreSystems.PlayerLoopSystem.UpdateServices;
using UnityEngine;
using Random = UnityEngine.Random;
using MessagePipe;
using Spine.Unity;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    public class FruitItem : MonoBehaviour, IUpdateHandler
    {
        private const string LogTag = "FruitItem";

        [SerializeField] private FruitDropRay fruitDropRay;
        [SerializeField] private SkeletonAnimation fruitSkeletonRenderer;
        [SerializeField] private LayerMask fruitLayerMask;
        [SerializeField] private LayerMask barrierLayerMask;

        [Header("Fruit Physics")] 
        [SerializeField] private Rigidbody2D fruitBody;
        [SerializeField] private CircleCollider2D fruitCollider;
        [SerializeField] private FruitConfig defaultFruitConfig;
        [SerializeField] private float minCenterTolerance;
        [SerializeField] private float maxCenterTolerance;
        [SerializeField] private float fruitSafeDistance = 0.5f;
        [SerializeField] private float upForce = 1f;

        private ISubscriber<FruitBoosterReadyMessage> _fruitBoosterReadySubscriber;
        private IPublisher<FruitDeadlineCollideMessage> _fruitDeadlineCollideMessagePublisher;
        private IPublisher<FruitReleaseMessage> _fruitReleasePublisher;
        private IPublisher<AddFruitScoreMessage> _addFruitScorePublisher;
        private IPublisher<FruitSpawnMessage> _fruitSpawnPublisher;

        private IDisposable _disposable;
        private ParticleSystem _fruitEffect;
        private FruitItemFactory _fruitItemFactory;
        private CancellationToken _cancellationToken;

        private bool _isDropped;
        private bool _hasResetCenterOfMass;
        private int _maxFruitLevel;

        private bool IsFirstCollider { get; set; }
        private int FruitScore { get; set; }
        public int FruitID { get; private set; }
        public Vector2 FruitMMassCenter => this.fruitBody.centerOfMass;

        private void Awake()
        {
            this._cancellationToken = this.GetCancellationTokenOnDestroy();
            this.InitializePublishers();
            this.SetRandomMassCenter();
        }

        private void OnEnable()
        {
            UpdateServiceManager.RegisterUpdateHandler(this);
        }

        private void InitializePublishers()
        {
            var builder = DisposableBag.CreateBuilder();
            this._fruitBoosterReadySubscriber = GlobalMessagePipe.GetSubscriber<FruitBoosterReadyMessage>();
            this._fruitBoosterReadySubscriber.Subscribe(this.OnFruitBoosterReadyMessageReceived).AddTo(builder);
            this._disposable = builder.Build();
            
            this._fruitDeadlineCollideMessagePublisher = GlobalMessagePipe.GetPublisher<FruitDeadlineCollideMessage>();
            this._fruitReleasePublisher = GlobalMessagePipe.GetPublisher<FruitReleaseMessage>();
            this._addFruitScorePublisher = GlobalMessagePipe.GetPublisher<AddFruitScoreMessage>();
            this._fruitSpawnPublisher = GlobalMessagePipe.GetPublisher<FruitSpawnMessage>();
        }

        private void SetRandomMassCenter()
        {
            Vector2 circleUnit = Random.insideUnitCircle.normalized;
            float centerTolerance = Random.Range(this.minCenterTolerance, this.maxCenterTolerance);
            this.fruitBody.centerOfMass = circleUnit * centerTolerance;
        }

        private void OnFruitBoosterReadyMessageReceived(FruitBoosterReadyMessage message)
        {
            if (this._isDropped)
                this.SetTintedFruitEnable(message.BoosterReady);
        }

        public void Tick(float deltaTime)
        {
            this.fruitDropRay.Tick(deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            this.TryResetCenterOfMass();
            this.OnColliderToOtherFruit(other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (this._isDropped && ((1 << other.gameObject.layer) & this.fruitLayerMask.value) != 0)
            {
                this.FireDeadlineCollideMessage(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (this._isDropped && ((1 << other.gameObject.layer) & this.fruitLayerMask.value) == 0)
            {
                this.FireDeadlineCollideMessage(false);
            }
        }

        private void FireDeadlineCollideMessage(bool isCollided)
        {
            this._fruitDeadlineCollideMessagePublisher.Publish(new FruitDeadlineCollideMessage
            {
                FruitEntityId = this.gameObject.GetInstanceID(),
                IsCollided = isCollided
            });
        }

        private void TryResetCenterOfMass()
        {
            if (this._hasResetCenterOfMass)
                return;

            Debug.Log($"[{LogTag}] Center of mass starting reset!");
            this._hasResetCenterOfMass = true;
            this.fruitBody.centerOfMass = this.fruitCollider.offset;
        }

        private void JumpABit()
        {
            float x = Random.value;
            float y = Random.value;
            Vector2 jumpDirection = new Vector2(x, y);
            this.fruitBody.AddForce(jumpDirection.normalized * this.upForce);
        }

        #region Check Fruit Execution

        private void OnColliderToOtherFruit(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & this.fruitLayerMask.value) == 0
                || !collision.collider.TryGetComponent(out FruitItem otherFruitItem))
                return;

            if (this.IsMaxFruitLevel())
            {
                Debug.Log($"[{LogTag}] Reach the max fruit level!");
                return;
            }

            this.TryMergeFruit(otherFruitItem);
        }

        private void TryMergeFruit(FruitItem otherFruitItem)
        {
            if (!this.IsSameFruit(otherFruitItem))
            {
                Debug.Log($"[{LogTag}] Not the same fruits!");
                return;
            }

            this.MergeFruitAsync(otherFruitItem).Forget();
        }

        private async UniTask MergeFruitAsync(FruitItem otherFruitItem)
        {
            if (!otherFruitItem.IsFirstCollider)
                this.IsFirstCollider = true;

            if (!this.IsFirstCollider)
                return;

            this.SetFruitColliderActive(false);
            otherFruitItem.SetFruitColliderActive(false);

            int nextFruitID = this.FruitID + 1;
            Vector2 averagePosition = (this.transform.position + otherFruitItem.transform.position) / 2f;
            FruitItemParam fruitItemParam = new FruitItemParam
            {
                FruitID = nextFruitID,
                Position = averagePosition,
            };

            FruitItem upgradedFruit = this._fruitItemFactory.Create(fruitItemParam);
            this.AddScore(upgradedFruit);
            upgradedFruit.TryResetCenterOfMass();
            upgradedFruit.Drop();
            upgradedFruit.JumpABit();

            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, this._cancellationToken);
            this.ReleaseFruit(this);
            this.ReleaseFruit(otherFruitItem);
        }

        private bool IsSameFruit(FruitItem fruitItem)
        {
            bool isSameFruit = fruitItem.FruitID == this.FruitID;
            return isSameFruit;
        }

        private bool IsMaxFruitLevel()
        {
            bool isMaxFruitLevel = this.FruitID >= this._maxFruitLevel;
            return isMaxFruitLevel;
        }

        private void ReleaseFruit(FruitItem fruitItem)
        {
            this._fruitReleasePublisher.Publish(new FruitReleaseMessage
            {
                FruitInstanceID = this.gameObject.GetInstanceID(),
            });

            if (this._fruitEffect)
            {
                GameObjectPoolManager.SpawnInstance(this._fruitEffect, 
                    this.transform.position, Quaternion.identity,
                    this._fruitItemFactory.FruitItemParent);
            }

            GameObjectPoolManager.Despawn(fruitItem.gameObject);
        }

        private void AddScore(FruitItem fruitItem)
        {
            this.AddFruitScore(fruitItem);
            this.AddSpawnedFruitToMemory(fruitItem);
        }

        private void AddSpawnedFruitToMemory(FruitItem fruitItem)
        {
            this._fruitSpawnPublisher.Publish(new FruitSpawnMessage
            {
                FruitItem = fruitItem,
            });
        }

        private void AddFruitScore(FruitItem fruitItem)
        {
            this._addFruitScorePublisher.Publish(new AddFruitScoreMessage
            {
                FruitScore = fruitItem.FruitScore,
            });
        }

        #endregion

        public void ApplyFruitConfig(FruitConfig fruitConfig)
        {
            this.FruitID = fruitConfig.fruitId;
            this.FruitScore = fruitConfig.fruitScore;
            this.fruitBody.mass = fruitConfig.fruitMass;
            this.fruitCollider.sharedMaterial = fruitConfig.fruitPhysicsMaterial;
            this._fruitEffect = fruitConfig.fruitParticles;
        }

        public void SetMaxFruitLevel(int level) => this._maxFruitLevel = level;

        public void SetFruitPhysicsActive(bool isActive)
        {
            RigidbodyType2D bodyType = isActive ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            this.fruitBody.bodyType = bodyType;
        }

        public void StopFruitPhysics()
        {
            this.fruitBody.linearVelocity = Vector2.zero;
        }

        public void SetFruitColliderActive(bool isActive) => this.fruitCollider.enabled = isActive;

        public void Drop()
        {
            this._isDropped = true;
            this.SetFruitColliderActive(true);
            this.SetFruitPhysicsActive(true);
            this.AddSpawnedFruitToMemory(this);
            this.SetDropRayEnable(false);
        }

        public float GetSafeDistanceBetweenCenterToTankEdge()
        {
            float radius = this.fruitCollider.radius;
            return radius;
        }

        public void SetDropRayEnable(bool enable)
        {
            this.fruitDropRay.SetDropRayEnabled(enable);
        }

        public void InitFruitFactory(FruitItemFactory fruitItemFactory)
        {
            this._fruitItemFactory = fruitItemFactory;
        }

        public void ForceBreakFruit()
        {
            this.AddFruitScore(this);
            this.ReleaseFruit(this);
        }

        private void SetTintedFruitEnable(bool enable)
        {
            // TODO: Play another animation
        }

        private void OnDisable()
        {
            this._isDropped = false;
            this.IsFirstCollider = false;
            this._hasResetCenterOfMass = false;
            this.ApplyFruitConfig(this.defaultFruitConfig);
            this.SetDropRayEnable(false);
            UpdateServiceManager.DeregisterUpdateHandler(this);
        }

        private void OnDestroy()
        {
            this._disposable?.Dispose();
        }
    }
}
