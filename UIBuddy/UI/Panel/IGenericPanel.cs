using UnityEngine;

namespace UIBuddy.UI.Panel
{
    public interface IGenericPanel
    {
        bool IsRootActive { get; }
        GameObject RootObject { get; }
        RectTransform RootRect { get; }
        string Name { get; }
        float GetOwnerScaleFactor();
        void EnsureValidPosition();
        void ShowPanelOutline(bool select);
        void SetActive(bool value);
        void SetRootActive(bool value);
        void Dispose();
        void Update();
    }
}
