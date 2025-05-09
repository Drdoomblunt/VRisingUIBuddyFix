using System;
using ProjectM;
using UIBuddy.Utils;
using UnityEngine;

namespace UIBuddy.KeyBinds;

/// <summary>
/// A bound keybinding instance. You can use this to observe the current
/// value of the keybinding, and to check whether or not the keybinding
/// is currently pressed.
/// </summary>
public class Keybinding
{
    /// <summary>
    /// The description of the keybinding.
    /// </summary>
    public KeybindingDescription Description { get; }

    /// <summary>
    /// The current primary key bound to this keybinding. `None` if not bound.
    /// </summary>
    public KeyCode Primary => KeybindRegistrator.KeybindingValues[Description.Id].Primary;
    /// <summary>
    /// The current secondary key bound to this keybinding. `None` if not bound.
    /// </summary>
    public KeyCode Secondary => KeybindRegistrator.KeybindingValues[Description.Id].Secondary;

    /// <summary>
    /// Utility method for checking whether the keybinding is currently pressed using
    /// UnityEngine.Input.GetKeyDown. If you need more control, such as checking whether
    /// the key is being held, you can manually query the key state using the Primary
    /// and Secondary fields.
    /// </summary>
    public bool IsPressed => Input.GetKeyDown(Primary) || Input.GetKeyDown(Secondary);

    // Unique XXHash-based inputflag for identification.
    internal ButtonInputAction InputFlag { get; private set; }
    // Unique XXHash-based quarter-of-an-assetguid for identification.
    internal int AssetGuid { get; private set; }

    public Keybinding(KeybindingDescription description)
    {
        Description = description;

        ComputeInputFlag();
        ComputeAssetGuid();
    }

    internal event Action KeyPressed;
    public void OnKeyPressed()
    {
        KeyPressed?.Invoke();
    }

    // Stubborn V Rising internals expect us to have a unique InputFlag
    // for every input. We deterministically generate a random one here,
    // and ensure it is not already in use (by the game).
    private void ComputeInputFlag()
    {
        var hash = HashUtils.Hash64(Description.Id);
        var invalid = false;
        do
        {
            invalid = false;
            foreach (var entry in Enum.GetValues<ButtonInputAction>())
            {
                if (hash == (ulong)entry)
                {
                    invalid = true;
                    hash -= 1;
                }
            }
        } while (invalid);

        InputFlag = (ButtonInputAction)hash;
    }

    // Ditto, but for asset GUIDs.
    private void ComputeAssetGuid()
    {
        AssetGuid = (int)HashUtils.Hash32(Description.Id);
    }
}