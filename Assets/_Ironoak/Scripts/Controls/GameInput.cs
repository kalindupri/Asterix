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
    ///
    /// Bindings
    ///   Move     A/D, arrows, left stick, d-pad
    ///   Jump     Space, W, Up, Z, gamepad A
    ///   Fire     X, J, left mouse, gamepad X / right trigger   (sling: hold to aim, release to throw)
    ///   Power    C, K, E, right mouse, gamepad Y / left trigger (use the Torc on a sap knot)
    ///   Aim      mouse cursor, right stick, or Up/Down while aiming
    ///   Pause    Esc, P, gamepad Start
    /// </summary>
    public static class GameInput
    {
        const float StickDeadzone = 0.5f;
        const float MouseIdleSeconds = 3f;

        static int _vertical, _prevVertical;
        static bool _prevFireHeld;
        static Vector2 _lastMouse;
        static float _lastMouseTime = -100f;

        public static float Horizontal { get; private set; }
        public static int Vertical => _vertical;
        public static bool JumpPressed { get; private set; }
        public static bool JumpHeld { get; private set; }
        public static bool FirePressed { get; private set; }
        public static bool FireHeld { get; private set; }
        public static bool FireReleased { get; private set; }
        public static bool PowerPressed { get; private set; }
        public static bool ConfirmPressed { get; private set; }
        public static bool PausePressed { get; private set; }
        public static bool BackPressed { get; private set; }
        public static Vector2 AimStick { get; private set; }
        public static Vector2 MouseScreen { get; private set; }
        /// <summary>True while the mouse has been used recently, so aiming follows the cursor.</summary>
        public static bool MouseActive => Time.unscaledTime - _lastMouseTime < MouseIdleSeconds;
        public static bool UsingGamepad { get; private set; }
        public static bool MenuUp => _vertical > 0 && _prevVertical <= 0;
        public static bool MenuDown => _vertical < 0 && _prevVertical >= 0;

        // ---- Automated playtest hooks (used only by DevPlaytest) ----
        public static bool TestMode;
        public static float TestHorizontal;
        public static bool TestJumpHeld;
        public static bool TestFireHeld;
        static bool _testPowerQueued, _prevTestJump, _prevTestFire;

        public static void TestPressPower() => _testPowerQueued = true;

        public static void Tick()
        {
            _prevVertical = _vertical;
            float h = 0f, v = 0f;
            bool jumpDown = false, jumpHeld = false, confirm = false, pause = false, back = false;
            bool fireHeld = false, powerDown = false, mouseButton = false, keyboardUsed = false, padUsed = false;
            Vector2 aim = Vector2.zero, mouse = MouseScreen;

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
                fireHeld |= k.xKey.isPressed || k.jKey.isPressed;
                powerDown |= k.cKey.wasPressedThisFrame || k.kKey.wasPressedThisFrame || k.eKey.wasPressedThisFrame;
                confirm |= k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.zKey.wasPressedThisFrame;
                pause |= k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame;
                back |= k.escapeKey.wasPressedThisFrame || k.xKey.wasPressedThisFrame || k.backspaceKey.wasPressedThisFrame;
                keyboardUsed = k.anyKey.isPressed;
            }

            var m = Mouse.current;
            if (m != null)
            {
                mouse = m.position.ReadValue();
                fireHeld |= m.leftButton.isPressed;
                powerDown |= m.rightButton.wasPressedThisFrame;
                mouseButton = m.leftButton.isPressed || m.rightButton.wasPressedThisFrame;
            }

            var g = Gamepad.current;
            if (g != null)
            {
                Vector2 s = g.leftStick.ReadValue() + g.dpad.ReadValue();
                if (Mathf.Abs(s.x) > StickDeadzone) h += Mathf.Sign(s.x);
                if (Mathf.Abs(s.y) > StickDeadzone) v += Mathf.Sign(s.y);
                aim = g.rightStick.ReadValue();
                jumpDown |= g.buttonSouth.wasPressedThisFrame;
                jumpHeld |= g.buttonSouth.isPressed;
                fireHeld |= g.buttonWest.isPressed || g.rightTrigger.isPressed;
                powerDown |= g.buttonNorth.wasPressedThisFrame || g.leftTrigger.wasPressedThisFrame;
                confirm |= g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame;
                pause |= g.startButton.wasPressedThisFrame;
                back |= g.buttonEast.wasPressedThisFrame;
                padUsed = s.sqrMagnitude > 0.25f || aim.sqrMagnitude > 0.25f || g.buttonSouth.isPressed || g.buttonWest.isPressed || g.buttonNorth.isPressed;
            }
#else
            if (Key(KeyCode.A) || Key(KeyCode.LeftArrow)) h -= 1f;
            if (Key(KeyCode.D) || Key(KeyCode.RightArrow)) h += 1f;
            if (Key(KeyCode.W) || Key(KeyCode.UpArrow)) v += 1f;
            if (Key(KeyCode.S) || Key(KeyCode.DownArrow)) v -= 1f;
            keyboardUsed = h != 0f || v != 0f;

            float ax = Axis("Horizontal"), ay = Axis("Vertical");
            if (Mathf.Abs(ax) > StickDeadzone) h += Mathf.Sign(ax);
            if (Mathf.Abs(ay) > StickDeadzone) v += Mathf.Sign(ay);

            jumpDown = Down(KeyCode.Space) || Down(KeyCode.Z) || Down(KeyCode.W) || Down(KeyCode.UpArrow) || Down(KeyCode.JoystickButton0);
            jumpHeld = Key(KeyCode.Space) || Key(KeyCode.Z) || Key(KeyCode.W) || Key(KeyCode.UpArrow) || Key(KeyCode.JoystickButton0);
            fireHeld = Key(KeyCode.X) || Key(KeyCode.J) || Key(KeyCode.Mouse0) || Key(KeyCode.JoystickButton2);
            powerDown = Down(KeyCode.C) || Down(KeyCode.K) || Down(KeyCode.E) || Down(KeyCode.Mouse1) || Down(KeyCode.JoystickButton3);
            confirm = Down(KeyCode.Return) || Down(KeyCode.KeypadEnter) || Down(KeyCode.Space) || Down(KeyCode.Z) || Down(KeyCode.JoystickButton0) || Down(KeyCode.JoystickButton7);
            pause = Down(KeyCode.Escape) || Down(KeyCode.P) || Down(KeyCode.JoystickButton7);
            back = Down(KeyCode.Escape) || Down(KeyCode.X) || Down(KeyCode.Backspace) || Down(KeyCode.JoystickButton1);
            mouse = UnityEngine.Input.mousePosition;
            mouseButton = Key(KeyCode.Mouse0) || Down(KeyCode.Mouse1);
            padUsed = Key(KeyCode.JoystickButton0) || Key(KeyCode.JoystickButton2) || Key(KeyCode.JoystickButton3) ||
                      (!keyboardUsed && (Mathf.Abs(ax) > StickDeadzone || Mathf.Abs(ay) > StickDeadzone));
#endif

            if ((mouse - _lastMouse).sqrMagnitude > 4f || mouseButton) _lastMouseTime = Time.unscaledTime;
            _lastMouse = mouse;
            if (padUsed) { UsingGamepad = true; _lastMouseTime = -100f; }
            else if (keyboardUsed || mouseButton) UsingGamepad = false;

            Horizontal = Mathf.Clamp(h, -1f, 1f);
            _vertical = v > 0f ? 1 : v < 0f ? -1 : 0;
            JumpPressed = jumpDown;
            JumpHeld = jumpHeld;
            FirePressed = fireHeld && !_prevFireHeld;
            FireReleased = !fireHeld && _prevFireHeld;
            FireHeld = fireHeld;
            _prevFireHeld = fireHeld;
            PowerPressed = powerDown;
            ConfirmPressed = confirm;
            PausePressed = pause;
            BackPressed = back;
            AimStick = aim;
            MouseScreen = mouse;

            if (TestMode)
            {
                Horizontal = TestHorizontal;
                JumpPressed = TestJumpHeld && !_prevTestJump;
                JumpHeld = TestJumpHeld;
                _prevTestJump = TestJumpHeld;
                FirePressed = TestFireHeld && !_prevTestFire;
                FireReleased = !TestFireHeld && _prevTestFire;
                FireHeld = TestFireHeld;
                _prevTestFire = TestFireHeld;
                PowerPressed = _testPowerQueued;
                _testPowerQueued = false;
                _lastMouseTime = -100f;
            }
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
