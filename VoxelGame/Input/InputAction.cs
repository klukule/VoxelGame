using Newtonsoft.Json;
using System;
using OpenTK.Input;

namespace VoxelGame
{
    /// <summary>
    /// Input action
    /// </summary>
    public class InputAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        public string Name;

        /// <summary>
        /// Primary action binding
        /// </summary>
        public Binding Primary;

        /// <summary>
        /// Called when key bound to this action is pressed
        /// </summary>
        [JsonIgnore] public Action KeyDown;

        /// <summary>
        /// Called when key bound to this action is released
        /// </summary>
        [JsonIgnore] public Action KeyUp;

        /// <summary>
        /// Create new action of given name with given binding
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="primary">Primary binding</param>
        public InputAction(string name, Binding primary)
        {
            Name = name;
            Primary = primary;
        }

        /// <summary>
        /// Empty constructor - used for deserialization
        /// </summary>
        public InputAction() { }

    }

    /// <summary>
    /// The key or mouse button binding
    /// </summary>
    public class Binding
    {
        /// <summary>
        /// The key - null if not bound to key
        /// </summary>
        public Key? KeyButton;

        /// <summary>
        /// The mouse button - null if not bound to mouse button
        /// </summary>
        public MouseButton? MouseButton;

        /// <summary>
        /// Create key binding
        /// </summary>
        /// <param name="input">The key</param>
        public Binding(Key input)
        {
            KeyButton = input;
            MouseButton = null;
        }

        /// <summary>
        /// Create mouse binding
        /// </summary>
        /// <param name="input">The mouse button</param>
        public Binding(MouseButton input)
        {
            KeyButton = null;
            MouseButton = input;
        }

        /// <summary>
        /// Deserialization only constructor
        /// </summary>
        public Binding() { }
    }
}
