using UnityEngine;
using UnityEngine.InputSystem;

namespace HCITrilogy.Lockdown.Audio
{
    /// <summary>
    /// Pure-distance-based footstep trigger. Plays a random clip from a list
    /// every step distance.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FootstepSystem : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private float stepDistance = 1.6f;

        private float _accum;

        private void Awake() { if (controller == null) controller = GetComponent<CharacterController>(); }

        private void Update()
        {
            if (!controller.isGrounded) return;
            Vector2 horiz = new(controller.velocity.x, controller.velocity.z);
            _accum += horiz.magnitude * Time.deltaTime;
            if (_accum >= stepDistance)
            {
                _accum = 0f;
                if (clips != null && clips.Length > 0 && source != null)
                {
                    var clip = clips[Random.Range(0, clips.Length)];
                    source.pitch = Random.Range(0.92f, 1.08f);
                    source.PlayOneShot(clip);
                }
            }
        }
    }
}
