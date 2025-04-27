using UnityEngine;

namespace UIBuddy.UI.Panel
{
    public interface IGenericPanel
    {
        bool IsActive { get; }

        GameObject RootObject { get; }
        Vector2 ReferenceResolution { get; set; }
        string Name { get; }
        float GetOwnerScaleFactor();
        void EnsureValidPosition();
        void SelectPanelAsCurrentlyActive(bool select);
        void SetActive(bool value);
        void SetActiveUnconditionally(bool value);
        void Dispose();
    }
}
