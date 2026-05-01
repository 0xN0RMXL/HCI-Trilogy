using UnityEngine;
using UnityEngine.InputSystem;
using HCITrilogy.Lockdown.Core;

namespace HCITrilogy.Lockdown.Player
{
    /// <summary>
    /// CharacterController-based first-person controller.
    ///
    /// IMPORTANT: pitch is applied to the camera pivot (cameraPivot.localEulerAngles.x),
    /// NOT to the controller. Yaw is applied via transform.Rotate(0, lookX, 0).
    /// Rotating the controller on pitch causes physics jitter.
    ///
    /// HCI: Yaw on root keeps the collider upright (predictable physics);
    /// pitch on a child decouples look from move. Standard FPS mapping.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float runSpeed  = 5.0f;
        [SerializeField] private float gravity   = -18f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float lookSensitivity = 0.18f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private float headBobAmplitude = 0.04f;
        [SerializeField] private float headBobFrequency = 9f;

        public bool ControlEnabled = true;

        private InputAction _move, _look, _jump, _sprint;
        private Vector3 _velocity;
        private float _pitch;
        private Vector3 _camHomeLocal;
        private float _bobT;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            if (cameraPivot != null) _camHomeLocal = cameraPivot.localPosition;
        }

        private void OnEnable()
        {
            if (inputActions == null) return;
            var map = inputActions.FindActionMap("Player", false);
            if (map == null) return;
            _move   = map.FindAction("Move", false);
            _look   = map.FindAction("Look", false);
            _jump   = map.FindAction("Jump", false);
            _sprint = map.FindAction("Sprint", false);
            map.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!ControlEnabled || PauseController.Instance != null && PauseController.Instance.IsPaused)
                return;
            DoLook();
            DoMove();
        }

        private void DoLook()
        {
            if (_look == null || cameraPivot == null) return;
            var settings = ServiceLocator.Get<SettingsManager>();
            float sensX = settings != null ? settings.SensitivityX : 1f;
            float sensY = settings != null ? settings.SensitivityY : 1f;
            int   inv   = settings != null && settings.InvertY ? -1 : 1;
            Vector2 d = _look.ReadValue<Vector2>();
            transform.Rotate(0f, d.x * lookSensitivity * sensX, 0f);
            _pitch = Mathf.Clamp(_pitch - d.y * lookSensitivity * sensY * inv, -maxPitch, maxPitch);
            cameraPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        private void DoMove()
        {
            if (_move == null) return;
            Vector2 in2 = _move.ReadValue<Vector2>();
            if (in2.sqrMagnitude > 1f) in2.Normalize();
            bool sprinting = _sprint != null && _sprint.IsPressed();
            float speed = sprinting ? runSpeed : walkSpeed;
            Vector3 dir = transform.right * in2.x + transform.forward * in2.y;
            Vector3 horizontal = dir * speed;

            if (controller.isGrounded)
            {
                if (_velocity.y < 0f) _velocity.y = -2f;
                if (_jump != null && _jump.WasPressedThisFrame())
                    _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            _velocity.y += gravity * Time.deltaTime;

            controller.Move((horizontal + new Vector3(0, _velocity.y, 0)) * Time.deltaTime);
            HeadBob(in2.magnitude > 0.1f, sprinting);
        }

        private void HeadBob(bool moving, bool sprinting)
        {
            if (cameraPivot == null) return;
            var settings = ServiceLocator.Get<SettingsManager>();
            if (settings != null && !settings.HeadBob)
            {
                cameraPivot.localPosition = _camHomeLocal;
                return;
            }
            if (!moving)
            {
                _bobT = 0;
                cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, _camHomeLocal, 10f * Time.deltaTime);
                return;
            }
            float freq = headBobFrequency * (sprinting ? 1.4f : 1f);
            float amp  = headBobAmplitude * (sprinting ? 1.5f : 1f);
            _bobT += Time.deltaTime * freq;
            float y = Mathf.Sin(_bobT) * amp;
            float x = Mathf.Cos(_bobT * 0.5f) * amp * 0.4f;
            cameraPivot.localPosition = _camHomeLocal + new Vector3(x, y, 0);
        }
    }
}
