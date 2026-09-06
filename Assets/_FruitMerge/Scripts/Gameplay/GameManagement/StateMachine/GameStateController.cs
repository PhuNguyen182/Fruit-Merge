using _FruitMerge.Scripts.Gameplay.UI.GameUI;
using _FruitMerge.Scripts.Input;
using ServiceLocators.Core;
using Stateless;

namespace _FruitMerge.Scripts.Gameplay.GameManagement.StateMachine
{
    public class GameStateController
    {
        private readonly InputController _inputController;
        private readonly FruitDragController _fruitDragController;
        private readonly FruitMergeGameUI _fruitMergeGameUI;

        private StateMachine<GameState, StateTrigger> _gameStateMachine;

        public GameStateController(FruitDragController fruitDragController, InputController inputController)
        {
            this._inputController = inputController;
            this._fruitDragController = fruitDragController;
            this._fruitMergeGameUI = ServiceLocator.ForSceneOf(fruitDragController).Get<FruitMergeGameUI>();
            this.BuildGameStateMachine();
        }

        private void BuildGameStateMachine()
        {
            this._gameStateMachine = new StateMachine<GameState, StateTrigger>(GameState.Begin);
            this._gameStateMachine.Configure(GameState.Begin)
                .Permit(StateTrigger.PlayGame, GameState.Playing)
                .OnActivate(this.OnStateMachineActivate);

            this._gameStateMachine.Configure(GameState.Playing)
                .Permit(StateTrigger.EndGame, GameState.EndGame)
                .Permit(StateTrigger.QuitGame, GameState.Quited)
                .OnEntryFrom(StateTrigger.PlayGame, this.OnPlayGame)
                .OnEntryFrom(StateTrigger.ContinuePlayGame, this.OnContinuePlayGame);

            this._gameStateMachine.Configure(GameState.EndGame)
                .Permit(StateTrigger.ContinuePlayGame, GameState.Playing)
                .Permit(StateTrigger.QuitGame, GameState.Quited)
                .OnEntryFrom(StateTrigger.EndGame, this.OnEndGame);

            this._gameStateMachine.Configure(GameState.EndGame)
                .OnEntryFrom(StateTrigger.QuitGame, this.OnQuitGame);

            this._gameStateMachine.Activate();
        }

        #region State Machine Callbacks

        private void OnStateMachineActivate()
        {

        }

        private void OnPlayGame()
        {
            this._inputController.IsInputActive = true;
            this._fruitDragController.SetEndGameState(false);
        }

        private void OnContinuePlayGame()
        {
            this._fruitDragController.SetEndGameState(false);
        }

        private void OnEndGame()
        {
            this._inputController.IsInputActive = false;
            this._fruitDragController.SetEndGameState(true);
            this._fruitMergeGameUI.ShowLosePopup();
        }

        private void OnQuitGame()
        {
            this._fruitDragController.SetEndGameState(false);
        }

        #endregion

        #region State Machine Trigger Functions

        public void PlayGame()
        {
            if (this._gameStateMachine.CanFire(StateTrigger.PlayGame))
                this._gameStateMachine.Fire(StateTrigger.PlayGame);
        }

        public void EndGame()
        {
            if (this._gameStateMachine.CanFire(StateTrigger.EndGame))
                this._gameStateMachine.Fire(StateTrigger.EndGame);
        }

        public void ContinueGame()
        {
            if (this._gameStateMachine.CanFire(StateTrigger.ContinuePlayGame))
                this._gameStateMachine.Fire(StateTrigger.ContinuePlayGame);
        }

        public void QuitGame()
        {
            if (this._gameStateMachine.CanFire(StateTrigger.QuitGame))
                this._gameStateMachine.Fire(StateTrigger.QuitGame);
        }

        #endregion
    }
}
