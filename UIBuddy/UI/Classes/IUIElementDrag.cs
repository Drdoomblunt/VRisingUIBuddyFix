using UIBuddy.Classes;
using UnityEngine;

namespace UIBuddy.UI.Classes
{
    public interface IUIElementDrag
    {
        bool IsActive { get; }
        bool WasDragging { get; set; }
        void Update(MouseState.ButtonState state, Vector3 rawMousePos);
    }
}
