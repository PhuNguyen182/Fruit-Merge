using System;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using MessagePipe;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class MessageBrokerManager
    {
        private readonly BuiltinContainerBuilder _builder;

        public MessageBrokerManager()
        {
            this._builder = new BuiltinContainerBuilder();
            this._builder.AddMessagePipe();

            this.AddMessageBrokers();
            IServiceProvider provider = this._builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);
        }

        private void AddMessageBrokers()
        {
            this._builder.AddMessageBroker<AddFruitScoreMessage>();
            this._builder.AddMessageBroker<FruitDeadlineCollideMessage>();
            this._builder.AddMessageBroker<FruitReleaseMessage>();
            this._builder.AddMessageBroker<FruitSpawnMessage>();
        }
    }
}
