using System;
using _FruitMerge.Scripts.Input;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameTask
{
    public class BoosterControllerTask : IDisposable
    {
        public HammerBoosterTask HammerBoosterTask { get; }

        public BoosterControllerTask(InputController inputController, GameObject hammerBoosterPrefab)
        {
            this.HammerBoosterTask = new HammerBoosterTask(inputController, hammerBoosterPrefab);
        }

        public void Dispose()
        {
            this.HammerBoosterTask.Dispose();
        }
    }
}