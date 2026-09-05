using System;
using System.Collections;
using System.Threading;
using _FruitMerge.Scripts.Gameplay.Factory.FruitFactory;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitTheme;
using _FruitMerge.Scripts.Gameplay.GameManagement;
using Cysharp.Threading.Tasks;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using DracoRuan.CoreSystems.PlayerLoopSystem.UpdateServices;
using GlobalScripts.Audios;
using UnityEngine;
using Random = UnityEngine.Random;
using MessagePipe;
using ServiceLocators.Core;
using Spine.Unity;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    public class FruitItem : MonoBehaviour, IUpdateHandler
    {
        private const string LogTag = "FruitItem";

        [SerializeField] private FruitDropRay fruitDropRay;
        [SerializeField] private FruitDropRay fruitFadeRay;
        [SerializeField] private SkeletonAnimation fruitSkeletonRenderer;
        [SerializeField] private LayerMask fruitLayerMask;
        [SerializeField] private LayerMask barrierLayerMask;
        [SerializeField] private SkeletonAnimation mergeEffect;
        [SerializeField] private AudioSource fruitSound;
        [SerializeField] private AudioClip[] mergeSounds;
        [SerializeField] private bool check;

        [Header("Fruit Physics")] 
        [SerializeField] private Rigidbody2D fruitBody;
        [SerializeField] private Collider2D fruitCollider;
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
        private FruitSpawner _fruitSpawner;
        private FruitItemFactory _fruitItemFactory;
        private FruitThemeConfig _fruitThemeConfig;
        private CancellationToken _cancellationToken;
        private readonly YieldInstruction _animationLoopWaitTimePhase1 = new WaitForSeconds(5f);
        private readonly YieldInstruction _animationLoopWaitTimePhase2 = new WaitForSeconds(1f);
        private readonly YieldInstruction _mergeAnimationWaitTime = new WaitForSeconds(0.5f);
        private Coroutine _loopAnimationCoroutine;

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
            this._fruitSpawner = ServiceLocator.ForSceneOf(this).Get<FruitSpawner>();
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
            
        }

        public void Tick(float deltaTime)
        {
            this.fruitDropRay.Tick(deltaTime);
            this.fruitFadeRay.Tick(deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            this.TryResetCenterOfMass();
            this.OnColliderToOtherFruit(other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (this._isDropped && ((1 << other.gameObject.layer) & this.barrierLayerMask.value) != 0)
            {
                this.FireDeadlineCollideMessage(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (this._isDropped && ((1 << other.gameObject.layer) & this.barrierLayerMask.value) != 0)
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
            FruitItem fruitPrefab = this._fruitThemeConfig.GetFruitById(nextFruitID);
            FruitItemParam fruitItemParam = new FruitItemParam
            {
                FruitID = nextFruitID,
                Position = averagePosition,
                Prefab = fruitPrefab,
            };

            FruitItem upgradedFruit = this._fruitItemFactory.Create(fruitItemParam);
            upgradedFruit.InitFruitThemeConfig(this._fruitThemeConfig);
            this.AddScore(upgradedFruit);
            upgradedFruit.TryResetCenterOfMass();
            upgradedFruit.Drop();
            upgradedFruit.JumpABit();
            upgradedFruit.PlayMergeAnimation();
            this.PlayMergeSound();

            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, this._cancellationToken);
            this._fruitSpawner.UpdateFruitBarView();
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

            if (this.mergeEffect)
            {
                var effect = GameObjectPoolManager.SpawnInstance(this.mergeEffect, 
                    this.transform.position, Quaternion.identity,
                    this._fruitItemFactory.FruitItemParent);
                effect.AnimationState.ClearTracks();
                effect.AnimationState.SetAnimation(0,"animation", false);
            }

            GameObjectPoolManager.Despawn(fruitItem.gameObject);
        }

        public void ReleaseImmediately()
        {
            this._fruitReleasePublisher.Publish(new FruitReleaseMessage
            {
                FruitInstanceID = this.gameObject.GetInstanceID(),
            });
            
            GameObjectPoolManager.Despawn(this.gameObject);
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

        private void PlayMergeSound()
        {
            int rand = Random.Range(0, this.mergeSounds.Length);
            AudioClip mergeClip = this.mergeSounds[rand];
            AudioManager.Instance.PlayFloatingAudio(mergeClip);
        }

        #endregion

        public void ApplyFruitConfig(FruitConfig fruitConfig)
        {
            this.FruitID = fruitConfig.fruitId;
            this.FruitScore = fruitConfig.fruitScore;
            this.fruitBody.mass = fruitConfig.fruitMass;
            this.fruitCollider.sharedMaterial = fruitConfig.fruitPhysicsMaterial;
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
            this.CancelLoopAnimation();
            this.SetFruitColliderActive(true);
            this.SetFruitPhysicsActive(true);
            this.AddSpawnedFruitToMemory(this);
            this.SetDropRayEnable(false);
            this.SetFadeRayEnable(false);
        }

        public float GetSafeDistanceBetweenCenterToTankEdge()
        {
            return this.fruitSafeDistance;
        }

        public void SetDropRayEnable(bool enable)
        {
            this.fruitDropRay.SetDropRayEnabled(enable);
            this.fruitDropRay.gameObject.SetActive(enable);
        }
        
        public void SetFadeRayEnable(bool enable)
        {
            this.fruitFadeRay.SetDropRayEnabled(enable);
            this.fruitFadeRay.gameObject.SetActive(enable);
        }

        public void InitFruitFactory(FruitItemFactory fruitItemFactory)
        {
            this._fruitItemFactory = fruitItemFactory;
        }

        public void InitFruitThemeConfig(FruitThemeConfig fruitThemeConfig)
        {
            this._fruitThemeConfig = fruitThemeConfig;
        }

        public void ForceBreakFruit()
        {
            this.AddFruitScore(this);
            this.ReleaseFruit(this);
        }

        public void PlayStartAnimation()
        {
            bool shouldPlayWaitAnimation = Random.value <= 0.5f;
            this.fruitSkeletonRenderer.AnimationState.ClearTracks();

            if (shouldPlayWaitAnimation)
            {
                this._loopAnimationCoroutine = StartCoroutine(this.PlayLoopSmileAnimation());
            }
            else
            {
                string emotion = "idle";
                this.fruitSkeletonRenderer.AnimationState.SetAnimation(0, emotion, false);
            }
        }

        private IEnumerator PlayLoopSmileAnimation()
        {
            while (true)
            {
                this.fruitSkeletonRenderer.AnimationState.SetAnimation(0, "idle", false);
                yield return this._animationLoopWaitTimePhase1;
                this.fruitSkeletonRenderer.AnimationState.SetAnimation(0, "smile", false);
                yield return this._animationLoopWaitTimePhase2;
            }
        }

        private void CancelLoopAnimation()
        {
            if (this._loopAnimationCoroutine != null)
                StopCoroutine(this._loopAnimationCoroutine);
        }

        private void PlayMergeAnimation()
        {
            StartCoroutine(this.PlayMergeAnimationWithDelay());
        }

        private IEnumerator PlayMergeAnimationWithDelay()
        {
            this.fruitSkeletonRenderer.AnimationState.SetAnimation(0, "smile", false);
            yield return this._mergeAnimationWaitTime;
            this.fruitSkeletonRenderer.AnimationState.SetAnimation(0, "idle", false);
        }

        private void OnDisable()
        {
            this._isDropped = false;
            this.IsFirstCollider = false;
            this._hasResetCenterOfMass = false;
            this.ApplyFruitConfig(this.defaultFruitConfig);
            this.SetDropRayEnable(false);
            this.SetFadeRayEnable(false);
            UpdateServiceManager.DeregisterUpdateHandler(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.check)
            {
                this.check = false;
                if (!this.fruitSound)
                    this.fruitSound = GetComponent<AudioSource>();
            }
        }
#endif

        private void OnDestroy()
        {
            this._disposable?.Dispose();
        }
    }
}
