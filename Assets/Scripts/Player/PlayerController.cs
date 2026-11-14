using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private Transform visual;
        [SerializeField] private Animator animator;
        [SerializeField] private float moveSpeed = 5.6f;
        [SerializeField] private float acceleration = 45f;
        [SerializeField] private float turnSpeed = 900f;

        private Rigidbody2D body;
        private Vector2 input;
        private Vector2 aimDirection = Vector2.up;
        private float aimHold;
        private bool controlsEnabled = true;

        public Vector2 Input => input;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void SetJoystick(VirtualJoystick stick)
        {
            joystick = stick;
        }

        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;
            if (!enabled)
            {
                input = Vector2.zero;
                body.velocity = Vector2.zero;
            }
        }

        public void SetAim(Vector2 direction)
        {
            aimDirection = direction;
            aimHold = 0.3f;
        }

        private void Update()
        {
            input = controlsEnabled ? ReadInput() : Vector2.zero;
            animator.SetFloat(SpeedHash, input.magnitude);

            aimHold -= Time.deltaTime;
            var facing = aimHold > 0f ? aimDirection : input;
            if (facing.sqrMagnitude > 0.01f)
            {
                var angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
                var target = Quaternion.Euler(0f, 0f, angle);
                visual.rotation = Quaternion.RotateTowards(visual.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            var desired = input * moveSpeed;
            body.velocity = Vector2.MoveTowards(body.velocity, desired, acceleration * Time.fixedDeltaTime);
            if (!ArenaGenerator.Bounds.Contains(body.position))
            {
                body.position = ArenaGenerator.ClampInside(body.position, 0.4f);
            }
        }

        private Vector2 ReadInput()
        {
            var direction = joystick != null ? joystick.Direction : Vector2.zero;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));
            }
            return Vector2.ClampMagnitude(direction, 1f);
        }
    }
}
