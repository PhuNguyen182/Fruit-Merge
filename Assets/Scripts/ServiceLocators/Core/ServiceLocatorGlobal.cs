using UnityEngine;

namespace ServiceLocators.Core
{
    [AddComponentMenu("ServiceLocator/ServiceLocator Global")]
    [DefaultExecutionOrder(-10)]
    public class ServiceLocatorGlobal : Bootstrapper
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap()
        {
            this.container.ConfigureAsGlobal(this.dontDestroyOnLoad);
        }
    }
}