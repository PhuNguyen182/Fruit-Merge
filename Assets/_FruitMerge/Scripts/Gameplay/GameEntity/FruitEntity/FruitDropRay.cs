using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    public class FruitDropRay : MonoBehaviour, IUpdateHandler
    {
        [SerializeField] private LineRenderer dropRay;
        [SerializeField] private float dropRayLength = 15f;
        [SerializeField] private LayerMask castLayerMask;

        private RaycastHit2D _dropHit;
        private bool _isActive;
        
        public void Tick(float deltaTime)
        {
            this.DrawDropRay();
        }

        private void DrawDropRay()
        {
            if (!this._isActive)
                return;

            this._dropHit = Physics2D.Raycast(this.transform.position,
                Vector2.down, this.dropRayLength, this.castLayerMask);
            if (!this._dropHit)
            {
                ResetDropRay();
                return;
            }

            this.dropRay.SetPosition(0, this.transform.position);
            this.dropRay.SetPosition(1, this._dropHit.point);

            return;

            void ResetDropRay()
            {
                this.dropRay.SetPosition(0, Vector2.zero);
                this.dropRay.SetPosition(1, Vector2.zero);
            }
        }

        public void SetDropRayEnabled(bool enable)
        {
            this._isActive = enable;
            this.dropRay.enabled = enable;
        }
    }
}
