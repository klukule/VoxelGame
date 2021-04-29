using OpenTK.Input;
using System.Collections.Generic;
using System.Linq;

namespace VoxelGame
{
    /// <summary>
    /// Input settings
    /// </summary>
    public class InputSettings
    {
        /// <summary>
        /// The lsit of input actions
        /// </summary>
        public List<InputAction> Actions { get; private set; } = new List<InputAction>();

        public InputSettings()
        {
            Actions.Add(new InputAction("Jump", new Binding(Key.Space)));
            Actions.Add(new InputAction("Sprint", new Binding(Key.ShiftLeft)));
            Actions.Add(new InputAction("Destroy Block", new Binding(MouseButton.Left)));
            Actions.Add(new InputAction("Interact", new Binding(MouseButton.Right)));
            Actions.Add(new InputAction("Inventory", new Binding(Key.Tab)));
            Actions.Add(new InputAction("Pause", new Binding(Key.Escape)));
        }

        /// <summary>
        /// Gets the input action of given name
        /// </summary>
        /// <param name="name">The action name</param>
        /// <returns>The action if found; otherwise null</returns>
        public InputAction GetAction(string name) => Actions.FirstOrDefault(x => x.Name == name);
    }
}
