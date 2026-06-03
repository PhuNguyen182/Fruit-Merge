using UnityEngine;

namespace ServiceLocators.Core
{
    [AddComponentMenu("ServiceLocator/ServiceLocator Scene")]
    [DefaultExecutionOrder(-5)]
    public class ServiceLocatorScene : Bootstrapper
    {
        protected override void Bootstrap()
        {
            this.container.ConfigureForScene();
        }
    }
}