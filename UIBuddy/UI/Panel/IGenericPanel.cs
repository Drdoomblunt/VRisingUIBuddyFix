using UnityEngine;

namespace UIBuddy.UI.Panel
{
    public interface IGenericPanel
    {
        GameObject RootObject { get; }
        Vector2 ReferenceResolution { get; set; }
        float GetOwnerScaleFactor();
        void EnsureValidPosition();
    }
}
