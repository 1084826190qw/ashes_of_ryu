using UnityEngine;
using UnityEngine.InputSystem;

namespace AshesOfRyu
{
    public class InputReader : MonoBehaviour
    {
        public static InputReader Instance { get; private set; }

        public Vector2 MoveAxis { get; private set; }
        public Vector2 RightStick { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public bool MouseActive { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool FirePressed { get; private set; }
        public bool DragonPressed { get; private set; }

        private bool prevAttack;
        private bool prevFire;
        private bool prevDragon;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            var ms = Mouse.current;
            var gp = Gamepad.current;

            Vector2 move = Vector2.zero;
            if (gp != null)
            {
                Vector2 stick = gp.leftStick.ReadValue();
                if (stick.magnitude > 0.2f) move = stick;
            }
            if (kb != null && move == Vector2.zero)
            {
                float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
                float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
                move = new Vector2(x, y);
            }
            MoveAxis = move;

            RightStick = gp != null ? gp.rightStick.ReadValue() : Vector2.zero;

            if (ms != null)
            {
                MousePosition = ms.position.ReadValue();
                MouseActive = ms.delta.ReadValue().magnitude > 0.1f;
            }

            bool attack = false, fire = false, dragon = false;
            if (ms != null) attack |= ms.leftButton.isPressed;
            if (kb != null)
            {
                attack |= kb.jKey.isPressed;
                fire |= kb.qKey.isPressed;
                dragon |= kb.rKey.isPressed;
            }
            if (gp != null)
            {
                attack |= gp.buttonWest.isPressed;
                fire |= gp.leftShoulder.isPressed;
                dragon |= gp.rightShoulder.isPressed;
            }

            AttackPressed = attack && !prevAttack;
            FirePressed = fire && !prevFire;
            DragonPressed = dragon && !prevDragon;

            prevAttack = attack;
            prevFire = fire;
            prevDragon = dragon;
        }
    }
}
