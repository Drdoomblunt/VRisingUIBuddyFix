using UIBuddy.Classes;
using UIBuddy.UI.Panel;
using UnityEngine;

namespace UIBuddy.UI.Classes
{
    public interface IUIElementDrag
    {
        IGenericPanel Panel { get; }
        bool IsActive { get; }
        bool WasDragging { get; set; }
        void Update(MouseState.ButtonState state, Vector3 rawMousePos);
    }
}
