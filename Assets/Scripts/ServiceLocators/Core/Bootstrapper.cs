using ServiceLocators.Extensions;
using UnityEngine;

namespace ServiceLocators.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private ServiceLocator _container;

        internal ServiceLocator container =>
            this._container.OrNull() ?? (this._container = this.GetComponent<ServiceLocator>());

        private bool _hasBeenBootstrapped;

        private void Awake() => this.BootstrapOnDemand();

        public void BootstrapOnDemand()
        {
            if (this._hasBeenBootstrapped) 
                return;
            
            this._hasBeenBootstrapped = true;
            this.Bootstrap();
        }

        protected abstract void Bootstrap();
    }
}
