using UIBuddy.UI.ScrollView.ObjectPool;
using UnityEngine;

namespace UIBuddy.UI.ScrollView;

public interface ICell : IPooledObject
{
    bool Enabled { get; }

    RectTransform Rect { get; set; }

    void Enable();
    void Disable();
}