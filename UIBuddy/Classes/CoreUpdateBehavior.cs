using Il2CppInterop.Runtime.Injection;
using UIBuddy.UI.Classes;
using UnityEngine;

namespace UIBuddy.Classes;

public class CoreUpdateBehavior : MonoBehaviour
{
    private GameObject _obj;

    public void Setup()
    {
        ClassInjector.RegisterTypeInIl2Cpp<CoreUpdateBehavior>();
        _obj = new GameObject("UIBuddyCoreUpdateBehavior");
        DontDestroyOnLoad(_obj);
        _obj.hideFlags = HideFlags.HideAndDontSave;
        _obj.AddComponent<CoreUpdateBehavior>();
    }

    protected void Update()
    {
        CoroutineUtility.TickRoutines();
        InputFieldRef.UpdateInstances();

        InputManager.Update();
        PanelManager.Instance?.Update();
    }

    public void Dispose()
    {
        if (_obj)
            Destroy(_obj);
    }
}