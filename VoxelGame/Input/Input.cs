using OpenTK;
using OpenTK.Input;
using System;
using System.Linq;

namespace VoxelGame
{
    /// <summary>
    /// Input manager
    /// </summary>
    public static class Input
    {
        private static Vector2 _mouseDelta = new Vector2(0, 0); // Mouse delta
        private static MouseState _currentMouseState;           // Current frame mouse state
        private static MouseState _previousMouseState;          // Previous frame mouse state
        private static KeyboardKeyEventArgs _lastKeyDown;       // Last pressed key
        private static char _lastKeyPress;                      // Last typed character

        /// <summary>
        /// Last pressed key
        /// </summary>
        public static KeyboardKeyEventArgs LastKeyDown => _lastKeyDown;

        /// <summary>
        /// Last typed character
        /// </summary>
        public static char LastKeyPress => _lastKeyPress;

        /// <summary>
        /// Mouse delta
        /// </summary>
        public static Vector2 MouseDelta => _mouseDelta;

        /// <summary>
        /// Called when mouse button is clicked
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs> MouseDown;

        /// <summary>
        /// Called when mouse button is released
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs> MouseUp;

        static Input()
        {
            // Hook native window events

            Program.Window.KeyDown += (sender, args) =>
            {
                // Skip repeat events
                if (args.IsRepeat) return;

                // Invoke any actions which are bound to this key
                foreach (var input in Program.Settings.Input.Actions.Where(x => x.Primary.KeyButton == args.Key))
                    input.KeyDown?.Invoke();

                // Store last pressed key
                _lastKeyDown = args;
            };

            Program.Window.KeyUp += (sender, args) =>
            {
                // Invoke any actions which are bound to this key
                foreach (var input in Program.Settings.Input.Actions.Where(x => x.Primary.KeyButton == args.Key))
                    input.KeyUp?.Invoke();
            };

            Program.Window.MouseDown += (sender, args) =>
            {
                MouseDown?.Invoke(sender, args);

                // Invoke any actions which are bound to this key
                foreach (var input in Program.Settings.Input.Actions.Where(x => x.Primary.MouseButton == args.Button))
                    input.KeyDown?.Invoke();
            };

            Program.Window.MouseUp += (sender, args) =>
            {
                MouseUp?.Invoke(sender, args);

                // Invoke any actions which are bound to this key
                foreach (var input in Program.Settings.Input.Actions.Where(x => x.Primary.MouseButton == args.Button))
                    input.KeyUp?.Invoke();
            };

            Program.Window.KeyPress += (sender, args) => { _lastKeyPress = args.KeyChar; };
        }

        /// <summary>
        /// Get input action from settings by it's name
        /// </summary>
        /// <param name="input">THe action name</param>
        /// <returns>The action or null if not found</returns>
        public static InputAction GetAction(string input) => Program.Settings.Input.GetAction(input);

        /// <summary>
        /// Update mouse input
        /// </summary>
        public static void Update()
        {
            _currentMouseState = Mouse.GetState();
            if (_currentMouseState != _previousMouseState)
                _mouseDelta = new Vector2(_currentMouseState.X - _previousMouseState.X, _currentMouseState.Y - _previousMouseState.Y);
            else
                _mouseDelta = Vector2.Zero;

            _previousMouseState = _currentMouseState;
        }

        /// <summary>
        /// Reset per-frame variables
        /// </summary>
        public static void PostRenderUpdate()
        {
            _lastKeyPress = ' ';
            _lastKeyDown = null;
        }

    }
}
