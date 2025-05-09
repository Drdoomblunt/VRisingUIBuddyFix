using Il2CppInterop.Runtime.Injection;
using UIBuddy.UI.Classes;
using UnityEngine;

namespace UIBuddy.Classes.Behaviors;

public class CoreUpdateBehavior : MonoBehaviour
{
    private GameObject _obj;

    public void Setup()
    {
        ClassInjector.RegisterTypeInIl2Cpp<CoreUpdateBehavior>();
        _obj = new GameObject();
        DontDestroyOnLoad(_obj);
        _obj.hideFlags = HideFlags.HideAndDontSave;
        _obj.AddComponent<CoreUpdateBehavior>();
    }

    public void Dispose()
    {
        if (_obj)
            Destroy(_obj);
    }


    protected void Update()
    {
        CoroutineUtility.TickRoutines();

        if(!Plugin.IsInitialized) return;

        InputFieldRef.UpdateInstances();
        UIBehaviourModel.UpdateInstances();

        InputManager.Update();
        PanelManager.Instance?.Update();
    }
}