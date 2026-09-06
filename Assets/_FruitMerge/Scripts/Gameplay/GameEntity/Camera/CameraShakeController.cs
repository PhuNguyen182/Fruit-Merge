using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using Unity.Cinemachine;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.Camera
{
    public class CameraShakeController : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseListener cinemachineImpulseListener;
        [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;

        [Header("Test Only")] 
        [SerializeField] private bool check;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float amplitude = 3f;
        [SerializeField] private float frequency = 3f;
        [SerializeField] private Vector3 impulseVelocity = Vector3.one;

        public void VibrateCamera(CameraVibrateMessage message)
        {
            this.cinemachineImpulseListener.ReactionSettings.AmplitudeGain = message.Amplitude;
            this.cinemachineImpulseListener.ReactionSettings.FrequencyGain = message.Frequency;
            this.cinemachineImpulseListener.ReactionSettings.Duration = message.Duration;
            this.cinemachineImpulseSource.GenerateImpulseAtPositionWithVelocity(message.SourcePosition, message.ImpulseVelocity);
        }

        private void Test()
        {
            this.cinemachineImpulseListener.ReactionSettings.AmplitudeGain = amplitude;
            this.cinemachineImpulseListener.ReactionSettings.FrequencyGain = frequency;
            this.cinemachineImpulseListener.ReactionSettings.Duration = duration;
            this.cinemachineImpulseSource.GenerateImpulseAtPositionWithVelocity(Vector3.zero, impulseVelocity);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (check)
            {
                check = false;
                Test();
            }    
        }
#endif
    }
}
