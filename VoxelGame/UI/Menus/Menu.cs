using System.Collections.Generic;

namespace VoxelGame.UI.Menus
{
    /// <summary>
    /// Base for all menu
    /// </summary>
    public class Menu
    {
        /// <summary>
        /// List of currently opened menus
        /// </summary>
        private static List<Menu> _openMenus = new List<Menu>();

        /// <summary>
        /// Whether the menu is opened or not
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Render all open menus
        /// </summary>
        public static void RenderGUI()
        {
            for (int i = 0; i < _openMenus.Count; i++)
            {
                if (_openMenus[i] != null)
                    _openMenus[i].OnGUI();
            }
        }

        /// <summary>
        /// Called during GUI render pass
        /// </summary>
        public virtual void OnGUI()
        {

        }

        /// <summary>
        /// Open the menu
        /// </summary>
        public virtual void Show()
        {
            if (!_openMenus.Contains(this))
            {
                _openMenus.Add(this);
                IsOpen = true;
            }
        }

        /// <summary>
        /// Close the menu
        /// </summary>
        public virtual void Close()
        {
            if (_openMenus.Contains(this))
            {
                _openMenus.Remove(this);
                IsOpen = false;
            }
        }
    }
}
