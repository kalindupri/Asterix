using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Ironoak.Controls
{
    /// <summary>
    /// Single polling point for all player input. Ticked once per frame by IronoakGame
    /// before any other Update runs. Works with both the new Input System and the
    /// legacy Input Manager, so the project compiles whichever backend is enabled.
    /// </summary>
    public static class GameInput
    {
        const float StickDeadzone = 0.5f;

        static int _vertical, _prevVertical;

        public static float Horizontal { get; private set; }
        public static bool JumpPressed { get; private set; }
        public static bool JumpHeld { get; private set; }
        public static bool ConfirmPressed { get; private set; }
        public static bool PausePressed { get; private set; }
        public static bool BackPressed { get; private set; }
        public static bool MenuUp => _vertical > 0 && _prevVertical <= 0;
        public static bool MenuDown => _vertical < 0 && _prevVertical >= 0;

        public static void Tick()
        {
            _prevVertical = _vertical;
            float h = 0f, v = 0f;
            bool jumpDown = false, jumpHeld = false, confirm = false, pause = false, back = false;

#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.aKey.isPressed || k.leftArrowKey.isPressed) h -= 1f;
                if (k.dKey.isPressed || k.rightArrowKey.isPressed) h += 1f;
                if (k.wKey.isPressed || k.upArrowKey.isPressed) v += 1f;
                if (k.sKey.isPressed || k.downArrowKey.isPressed) v -= 1f;
                jumpDown |= k.spaceKey.wasPressedThisFrame || k.zKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame;
                jumpHeld |= k.spaceKey.isPressed || k.zKey.isPressed || k.wKey.isPressed || k.upArrowKey.isPressed;
                confirm |= k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.zKey.wasPressedThisFrame;
                pause |= k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame;
                back |= k.escapeKey.wasPressedThisFrame || k.xKey.wasPressedThisFrame || k.backspaceKey.wasPressedThisFrame;
            }

            var g = Gamepad.current;
            if (g != null)
            {
                Vector2 s = g.leftStick.ReadValue() + g.dpad.ReadValue();
                if (Mathf.Abs(s.x) > StickDeadzone) h += Mathf.Sign(s.x);
                if (Mathf.Abs(s.y) > StickDeadzone) v += Mathf.Sign(s.y);
                jumpDown |= g.buttonSouth.wasPressedThisFrame;
                jumpHeld |= g.buttonSouth.isPressed;
                confirm |= g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame;
                pause |= g.startButton.wasPressedThisFrame;
                back |= g.buttonEast.wasPressedThisFrame;
            }
#else
            if (Key(KeyCode.A) || Key(KeyCode.LeftArrow)) h -= 1f;
            if (Key(KeyCode.D) || Key(KeyCode.RightArrow)) h += 1f;
            if (Key(KeyCode.W) || Key(KeyCode.UpArrow)) v += 1f;
            if (Key(KeyCode.S) || Key(KeyCode.DownArrow)) v -= 1f;

            float ax = Axis("Horizontal"), ay = Axis("Vertical");
            if (Mathf.Abs(ax) > StickDeadzone) h += Mathf.Sign(ax);
            if (Mathf.Abs(ay) > StickDeadzone) v += Mathf.Sign(ay);

            jumpDown = Down(KeyCode.Space) || Down(KeyCode.Z) || Down(KeyCode.W) || Down(KeyCode.UpArrow) || Down(KeyCode.JoystickButton0);
            jumpHeld = Key(KeyCode.Space) || Key(KeyCode.Z) || Key(KeyCode.W) || Key(KeyCode.UpArrow) || Key(KeyCode.JoystickButton0);
            confirm = Down(KeyCode.Return) || Down(KeyCode.KeypadEnter) || Down(KeyCode.Space) || Down(KeyCode.Z) || Down(KeyCode.JoystickButton0) || Down(KeyCode.JoystickButton7);
            pause = Down(KeyCode.Escape) || Down(KeyCode.P) || Down(KeyCode.JoystickButton7);
            back = Down(KeyCode.Escape) || Down(KeyCode.X) || Down(KeyCode.Backspace) || Down(KeyCode.JoystickButton1);
#endif

            Horizontal = Mathf.Clamp(h, -1f, 1f);
            _vertical = v > 0f ? 1 : v < 0f ? -1 : 0;
            JumpPressed = jumpDown;
            JumpHeld = jumpHeld;
            ConfirmPressed = confirm;
            PausePressed = pause;
            BackPressed = back;
        }

#if !ENABLE_INPUT_SYSTEM
        static bool Key(KeyCode c) => UnityEngine.Input.GetKey(c);
        static bool Down(KeyCode c) => UnityEngine.Input.GetKeyDown(c);

        static float Axis(string name)
        {
            try { return UnityEngine.Input.GetAxisRaw(name); }
            catch (System.ArgumentException) { return 0f; }
        }
#endif
    }
}
