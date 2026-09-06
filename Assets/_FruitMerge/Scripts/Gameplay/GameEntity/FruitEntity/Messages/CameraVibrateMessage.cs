using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages
{
    public struct CameraVibrateMessage
    {
        public Vector3 SourcePosition;
        public Vector3 ImpulseVelocity;
        public float Duration;
        public float Frequency;
        public float Amplitude;
    }
}