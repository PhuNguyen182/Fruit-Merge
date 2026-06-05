using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace _FruitMerge.Scripts.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        
        private GameInputSystem _solitaireInputPlayer;
        private List<RaycastResult> _results = new();
        private PointerEventData _eventDataCurrentPosition;

        public bool IsPointerDown { get; private set; }
        public bool IsPointerUp { get; private set; }
        public bool IsPointerClicked { get; private set; }
        public bool IsInputActive { get; set; } = true;
        public Vector2 PointerDelta { get; private set; }

        public Vector2 ScreenPointerPosition { get; private set; }
        public Vector2 ViewportPointerPosition { get; private set; }
        public Vector2 WorldPointerPosition { get; private set; }
        
        public event Action OnPointerDown; 
        public event Action OnPointerUp; 

        private void Awake()
        {
            this._solitaireInputPlayer = new GameInputSystem();
            this.SetupCameraForInput();
            this.RegisterInputActions();
        }

        #region Inpit Registration

        private void RegisterInputActions()
        {
            this.RegisterInputMovement();
            this.RegisterInputClick();
            this.RegisterInputPointerDelta();
        }

        private void RegisterInputMovement()
        {
            this._solitaireInputPlayer.Player.Position.started += this.UpdatePointerPosition;
            this._solitaireInputPlayer.Player.Position.performed += this.UpdatePointerPosition;
            this._solitaireInputPlayer.Player.Position.canceled += this.UpdatePointerPosition;
        }

        private void RegisterInputClick()
        {
            this._solitaireInputPlayer.Player.Press.started += this.UpdatePointerClicked;
            this._solitaireInputPlayer.Player.Press.performed += this.UpdatePointerClicked;
            this._solitaireInputPlayer.Player.Press.canceled += this.UpdatePointerClicked;
        }

        private void RegisterInputPointerDelta()
        {
            this._solitaireInputPlayer.Player.Delta.started += this.UpdatePointerDelta;
            this._solitaireInputPlayer.Player.Delta.performed += this.UpdatePointerDelta;
            this._solitaireInputPlayer.Player.Delta.canceled += this.UpdatePointerDelta;
        }

        private void UnregisterInputActions()
        {
            this.UnregisterInputMovement();
            this.UnregisterInputClick();
            this.UnregisterInputPointerDelta();
        }

        #endregion

        #region Input Unregistration

        private void UnregisterInputMovement()
        {
            this._solitaireInputPlayer.Player.Position.started -= this.UpdatePointerPosition;
            this._solitaireInputPlayer.Player.Position.performed -= this.UpdatePointerPosition;
            this._solitaireInputPlayer.Player.Position.canceled -= this.UpdatePointerPosition;
        }

        private void UnregisterInputClick()
        {
            this._solitaireInputPlayer.Player.Press.started -= this.UpdatePointerClicked;
            this._solitaireInputPlayer.Player.Press.performed -= this.UpdatePointerClicked;
            this._solitaireInputPlayer.Player.Press.canceled -= this.UpdatePointerClicked;
        }
        
        private void UnregisterInputPointerDelta()
        {
            this._solitaireInputPlayer.Player.Delta.started -= this.UpdatePointerDelta;
            this._solitaireInputPlayer.Player.Delta.performed -= this.UpdatePointerDelta;
            this._solitaireInputPlayer.Player.Delta.canceled -= this.UpdatePointerDelta;
        }

        #endregion

        #region Input Update

        private void UpdatePointerPosition(InputAction.CallbackContext context)
        {
            this.SetupCameraForInput();
            this.ScreenPointerPosition = this.IsInputActive ? context.ReadValue<Vector2>() : Vector2.zero;
            if (!this.inputCamera) 
                return;
            
            this.ViewportPointerPosition = this.inputCamera.ScreenToViewportPoint(this.ScreenPointerPosition);
            this.WorldPointerPosition = this.inputCamera.ScreenToWorldPoint(this.ScreenPointerPosition);
        }

        private void UpdatePointerClicked(InputAction.CallbackContext context)
        {
            this.IsPointerClicked = this.IsInputActive && context.ReadValueAsButton();
        }

        private void UpdatePointerDelta(InputAction.CallbackContext context)
        {
            this.PointerDelta = context.ReadValue<Vector2>();
        }

        #endregion

        #region Input UI

        public bool IsPointerOverlapUI()
        {
#if UNITY_EDITOR
            bool result = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IOS
            bool result = this.IsPointerOverUIObject(); 
#endif
            return result;
        }

        private bool IsPointerOverUIObject()
        {
            if (!EventSystem.current)
                return false;

            this._results.Clear();
            this._eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(this.ScreenPointerPosition.x, this.ScreenPointerPosition.y)
            };
            
            EventSystem.current.RaycastAll(this._eventDataCurrentPosition, this._results);
            bool result = this._results.Count > 0; 
            return result;
        }

        #endregion

        private void OnEnable()
        {
            this._solitaireInputPlayer.Enable();
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            this.UpdatePointerDownState();
            this.UpdatePointerUpState();
        }

        private void UpdatePointerDownState()
        {
            this.IsPointerDown = this.IsInputActive && this._solitaireInputPlayer.Player.Press.WasPressedThisFrame();
            if (this.IsPointerDown)
                this.OnPointerDown?.Invoke();
        }

        private void UpdatePointerUpState()
        {
            this.IsPointerUp = this.IsInputActive && this._solitaireInputPlayer.Player.Press.WasReleasedThisFrame();
            if (this.IsPointerUp)
                this.OnPointerUp?.Invoke();
        }

        private void SetupCameraForInput()
        {
            if (!this.inputCamera)
                this.inputCamera = Camera.main;
        }

        private void OnDisable()
        {
            this._solitaireInputPlayer.Disable();
            EnhancedTouchSupport.Disable();
        }

        private void OnDestroy()
        {
            this.UnregisterInputActions();
            this._solitaireInputPlayer.Dispose();
        }
    }
}
