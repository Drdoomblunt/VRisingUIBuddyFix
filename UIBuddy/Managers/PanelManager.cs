using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Xml.Serialization;
using UIBuddy.Classes;
using UIBuddy.UI;
using UIBuddy.UI.Classes;
using UIBuddy.UI.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.Managers;

public class PanelManager: IDisposable
{
    private static Vector3 _previousMousePosition;
    private static MouseState.ButtonState _previousMouseButtonState;
    private static readonly List<IUIElementDrag> DraggersList = new();
    public static bool WasAnyDragging;
    public static bool DraggerHandledThisFrame;
    private static bool MouseInTargetDisplay => true;
    public static GameObject PoolHolder { get; private set; }

   // public static PanelManager Instance { get; private set; }
    private static GameObject CanvasRoot { get; set; }
    private static CanvasScaler Scaler { get; set; }
    private static Canvas Canvas { get; set; }
    private static GameObject PanelHolder { get; set; }

    // Main control panel
    public static  MainControlPanel MainPanel { get; private set; }
    public static ElementListPanel ElementListPanel { get; private set; }
    private static bool _focusHandledThisFrame;
    private static ScreenType _currentScreenType = ScreenType.MainMenu;

    private static volatile bool _isContentChanging;

    public static ScreenType CurrentScreenType
    {
        get => _currentScreenType;
        set
        {
            _currentScreenType = value; 
            Plugin.Log.LogWarning($"{nameof(CurrentScreenType)} changed to {CurrentScreenType}");
        }
    }

    /// <summary>
    /// Dirty hacks to fix some bar UI elements
    /// </summary>
    public static List<string> ButtonBarFixList =
    [
        "Action Bar",
        "Ability Bar"
    ];

    public static string RecipeTrackerFix = "Recipe Tracker";

    private static bool ShouldUpdateFocus =>
        MouseInTargetDisplay &&
        InputManager.Mouse.Button0 == MouseState.ButtonState.Down &&
        !WasAnyDragging;

    public PanelManager()
    {
        CreateRootCanvas();
        CreateElementListPanel();
        CreateMainPanel();
    }

    /// <summary>
    /// Reloads all UI elements based on the current screen type.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void ReloadElements()
    {
        _isContentChanging = true;
        try
        {
            var inheritAnchorParam = new PanelParameters
            {
                InheritAnchors = true
            };

            ClearAllElements(false);

            //IMPORTANT! GameObject.Find() can't find inactive objects so we need to
            //specify path to the object from the distinctly named parent that is always active

            switch (CurrentScreenType)
            {
                case ScreenType.MainMenu:
                {
                    AddDrag("MainMenu_V2(Clone)|SLS logo", "Logo", "Logo");
                    AddDrag("MainMenu_V2(Clone)|Content|NewsPanelParent", "News", "News");
                    AddDrag("MainMenu_V2(Clone)|Content|SideBar", "Main Menu", "Menu");
                    AddDrag("MainMenu_V2(Clone)|Content|SideBar|LinksParentNode", "(!!)Links", "Links",
                        prms: inheritAnchorParam);
                    AddDrag("CanvasUnscaledMainMenu|VersionParent|VersionString|background", "Version String", "V");
                }
                    break;
                case ScreenType.CharacterHUD:
                {
                    AddDrag("BottomBarCanvas|BottomBar(Clone)|Content|BloodOrbParent", "Blood Orb HP", "ORB");
                    //AddDetachedDrag("BottomBar(Clone)|Content|TooltipParent|BloodPoolTooltip", "Blood Orb Tooltip Anchor", "BOTA");

                    AddDrag("HUDCanvas(Clone)|JournalCanvas|JournalParent(Clone)", "Journal", "JOU");
                    AddDrag("HUDCanvas(Clone)|TargetInfoPanelCanvas|TargetInfoPanel(Clone)", "Target Info", "TGT");

                    AddDrag("ClockParent3(Clone)|Content|Parent", "DayNight sphere", "SPHERE"); //daytime circle
                    AddDrag("MiniMapParent(Clone)|Root|Panel", "Minimap", "MAP"); //minimap
                    AddDrag("ClockParent3(Clone)|Content|BackgroundBig", "Clock+Minimap Background", "Background"); //clock/minimap background

                    AddDrag("BottomBarCanvas|BottomBar(Clone)", "Full Bottom Bar", "FBB"); //bottom bar
                    AddDrag("BottomBar(Clone)|Content|Background|Background", "Bottom Bar Background",
                        "Bar Background"); //bottom bar fade
                    AddDrag("BottomBar(Clone)|Content|Background|DarkFade", "Bottom Bar Fade",
                        "Bar Fade"); //bottom bar bg


                    AddDrag("BottomBar(Clone)|Content|Background|ActionBar", "Action Bar", "ACBAR",
                        prms: inheritAnchorParam); //action bar

                    AddDrag("BottomBar(Clone)|Content|Background|AbilityBar", "Ability Bar", "ABBAR",
                        prms: inheritAnchorParam); //abilityBar bar

                    AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry", "Action Bar Button 1",
                        "ACB1"); //ab1
                    AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry (1)", "Action Bar Button 2",
                        "ACB2"); //ab2
                    AddDrag("BottomBar(Clone)|Content|Background|AbilityBar|AbilityBarEntry_Primary", "AB primary", "ABP");

                    AddDrag("BottomBar(Clone)|Content|BuffBar|Buffs", "Buffs", "BUFF");
                    AddDrag("BottomBar(Clone)|Content|BuffBar|Debuffs", "Debuffs", "DBUFF");

                    AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|HUDAlertParent(Clone)|Container", "(?)Right Alerts",
                        "RA"); //right alerts
                    AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|DangerTextParent(Clone)", "(?)Bottom Danger", "DANGER",
                        panelParentGameObjectName:
                        "HUDCanvas(Clone)|Canvas|HUDOther|DangerTextParent(Clone)|Alpha|Background",
                        prms: inheritAnchorParam); //bottom danger text
                    AddDrag("HUDCanvas(Clone)|Canvas|HUDTutorial|TutorialParent(Clone)", "(?)Tutorial", "Tutorial",
                        prms: inheritAnchorParam); //

                    var prms = new PanelParameters
                    {
                        PositionValidation = false,
                        InitialPosition = new Vector2(0, 0),
                    };
                    AddDetachedDrag("HUDChatParent|ChatWindow(Clone)|Content", "(!!)Chat Window", "CHAT", prms); //chat
                    
                    AddDrag("PlayerList_Overlay(Clone)|Version_HUD", "(!!)Clan Members", "CM", prms: inheritAnchorParam);
                    //AddDrag("HUDCanvas(Clone)|Canvas|HUDRecipeTrackerParent", "Recipe Tracker", "Recipe");
                    AddDrag("CanvasUnscaledMainMenu|VersionParent|VersionString|background", "Version String", "V");
                    AddDrag("ActionWheelReminder|Icon", "Wheel Reminder Icon", "WRI");
                    AddDrag("ActionWheelReminder|KeybindBackground", "Wheel Reminder Key", "WRK");
                    ////AddDrag("InventoryMenu(Clone)", "Inventory Window", "Inventory");
                    //AddDrag("WorkstationMenu(Clone)|MenuParent|CharacterInventorySubMenu(Clone)", "Craft - Inventory", "CraftInv");
                    //AddDrag("WorkstationMenu(Clone)|MenuParent|CharacterInventorySubMenu(Clone)", "Craft - Recipes", "CraftRec");
                    }
                    break;
                case ScreenType.EscapeMenu:

                    //AddDrag("FullscreenMenu(Clone)|Root|OuterLayout|InnerLayout|ViewParent|ViewParent (1)|EscapeMenu(Clone)|Menu|GameObject", "Bug Buttons", "BUG", prms: inheritAnchorParam);
                    //AddDrag("FullscreenMenu(Clone)|Root|OuterLayout|InnerLayout|ViewParent|ViewParent (1)|EscapeMenu(Clone)|Menu|ServerInfoLayout", "Server Info", "Server", prms: inheritAnchorParam);
                    AddDrag("FullscreenMenu(Clone)|Root|OuterLayout|InnerLayout|ViewParent|ViewParent (1)|EscapeMenu(Clone)|Menu|Logo", "Logo", "Logo");
                    //AddDrag("FullscreenMenu(Clone)|Root|OuterLayout|InnerLayout|ViewParent|ViewParent (1)|EscapeMenu(Clone)|Menu|ButtonCollection", "Buttons", "Buttons", prms: inheritAnchorParam);
                    
                    break;
                case ScreenType.None:
                    // No specific screen type, do nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CurrentScreenType), CurrentScreenType, null);
            }

            MainPanel.RootRect.SetAsLastSibling();
            ElementListPanel.RootRect.SetAsLastSibling();
            ElementListPanel.RefreshList();
        }
        finally
        {
            _isContentChanging = false;
        }
    }

    private static void CreateElementListPanel()
    {
        ElementListPanel = new ElementListPanel(PanelHolder);
        ElementListPanel.Initialize();
        DraggersList.Add(ElementListPanel.Dragger);
        Plugin.Log.LogInfo("Elements list panel created and initialized successfully");
    }

    /// <summary>
    /// Creates the main control panel.
    /// </summary>
    private static void CreateMainPanel()
    {
        try
        {
            // Create the main control panel
            MainPanel = new MainControlPanel(PanelHolder);
            MainPanel.Initialize();
            DraggersList.Add(MainPanel.Dragger);
            Plugin.Log.LogInfo("Main panel created and initialized successfully");

        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error creating main panel: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates the root canvas for the UI.
    /// </summary>
    private static void CreateRootCanvas()
    {
        CanvasRoot = new GameObject("Canvas");
        var canvasRootRect = CanvasRoot.AddComponent<RectTransform>();
        UnityEngine.Object.DontDestroyOnLoad(CanvasRoot);
        CanvasRoot.SetActive(false);
        CanvasRoot.hideFlags |= HideFlags.HideAndDontSave;
        CanvasRoot.layer = 5;
        CanvasRoot.transform.position = new Vector3(0f, 0f, 1f);

        Canvas = CanvasRoot.AddComponent<Canvas>();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas.referencePixelsPerUnit = 100;
        Canvas.sortingOrder = 30000;
        Canvas.overrideSorting = true;

        Scaler = CanvasRoot.AddComponent<CanvasScaler>();
        Scaler.referenceResolution = new Vector2(3840, 2160);
        Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        CanvasRoot.AddComponent<GraphicRaycaster>();
        canvasRootRect.anchorMin = Vector2.zero;
        canvasRootRect.anchorMax = Vector2.one;
        canvasRootRect.pivot = new Vector2(0.5f, 0.5f);

        PanelHolder = UIFactory.CreateUIObject("PanelHolder", CanvasRoot);
        var rect = PanelHolder.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        CanvasRoot.SetActive(true);

        PoolHolder = new GameObject("UIBuddy_PoolHolder");
        PoolHolder.transform.parent = CanvasRoot.transform;
        PoolHolder.SetActive(false);
    }


    public static void Update()
    {
        if(!MainPanel.IsRootActive || _isContentChanging)
            return;

        if (ConfigManager.SelectPanelsWithMouse && ShouldUpdateFocus)
            UpdateFocus();

        foreach (var panel in DraggersList.Select(a=> a.Panel))
        {
            panel.Update();
            if (_isContentChanging) return;
        }

        if (!DraggerHandledThisFrame)
            UpdateDraggers();

        DraggerHandledThisFrame = false;
        _focusHandledThisFrame = false;
    }

    protected static void UpdateFocus()
    {
        // If another UIBase has already handled a user's click for focus, don't update it for this UIBase.
        if (!_focusHandledThisFrame)
        {
            var mousePos = InputManager.Mouse.Position;
            var dragger = DraggersList
                .FirstOrDefault(a => a.Panel.IsRootActive &&
                                     a.Panel.RootRect.rect.Contains(a.Panel.RootRect.InverseTransformPoint(mousePos)));
                
            if (dragger?.Panel != null)
            {
                if (!IsControlPanel(dragger.Panel))
                {
                    SelectPanel(dragger.Panel);
                    ElementListPanel.UpdateSelectedEntry(dragger.Panel);
                    DraggersList.Remove(dragger);
                    DraggersList.Add(dragger);
                }
            }

            _focusHandledThisFrame = true;
        }
    }

    private static bool IsControlPanelMouseOver()
    {
        var mousePos = InputManager.Mouse.Position;
        var dragPos = MainPanel.RootRect.InverseTransformPoint(mousePos);
        bool inDragPos = MainPanel.RootRect.rect.Contains(dragPos);
        if (inDragPos)
            return true;
        dragPos = ElementListPanel.RootRect.InverseTransformPoint(mousePos);
        inDragPos = ElementListPanel.RootRect.rect.Contains(dragPos);
        if (inDragPos)
            return true;

        return false;
    }

    private static bool IsControlPanel(IGenericPanel panel)
    {
        return panel == MainPanel || panel == ElementListPanel;
    }

    private static void UpdateDraggers()
    {
        if (!MouseInTargetDisplay)
            return;

        var state = InputManager.Mouse.Button0;
        var mousePos = InputManager.Mouse.Position;

        // If the mouse hasn't changed, we don't need to do any more
        if (mousePos == _previousMousePosition && state == _previousMouseButtonState) 
            return;
        _previousMousePosition = mousePos;
        _previousMouseButtonState = state;

        if (!DraggerHandledThisFrame && MainPanel.IsRootActive)
        {
            //this is where we restrict panel selection for now
            foreach (var instance in DraggersList.Where(instance =>
                         instance.IsActive &&
                         (MainPanel.SelectedElementPanel != null &&
                           instance.Panel == MainPanel.SelectedElementPanel ||
                          IsControlPanel(instance.Panel))))
            {
                if (!instance.Panel.IsRootActive) continue;

                instance.Update(state, mousePos);

                if (DraggerHandledThisFrame || _isContentChanging)
                    break;
            }
               
        }

        if (WasAnyDragging && state.HasFlag(MouseState.ButtonState.Up))
        {
            WasAnyDragging = false;
            if(_isContentChanging) return;
            foreach (var instance in DraggersList)
                instance.WasDragging = false;
        }
    }

    /// <summary>
    /// Adds a dragger to the list of draggers.
    /// </summary>
    /// <param name="gameObjectName">Actual object we want to modify</param>
    /// <param name="friendlyName">Friendly name</param>
    /// <param name="shortName">Short name</param>
    /// <param name="panelParentGameObjectName">Optional object to use as panel root</param>
    /// <param name="prms">Optional parameters</param>
    private static void AddDrag(string gameObjectName, string friendlyName, string shortName = null, string panelParentGameObjectName = null, PanelParameters prms = null)
    {
        if(DraggersList.FirstOrDefault(a=> a.Panel.Name == friendlyName) != null)
            return;

        var element = new ElementPanel(gameObjectName, friendlyName, shortName, panelParentGameObjectName);

        if (prms != null)
            element.SetParameters(prms);

        if (element.Initialize())
        {
            DraggersList.Add(element.Dragger);
            ElementListPanel.AddElement(element);
        } 
        else element.Dispose();
    }

    /// <summary>
    /// Adds a panel with detached dragger
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="friendlyName">Friendly name</param>
    /// <param name="shortName"></param>
    /// <param name="prms"></param>
    /// <returns></returns>
    private static DetachedPanel AddDetachedDrag(string name, string friendlyName, string shortName, PanelParameters prms = null)
    {
        if (DraggersList.FirstOrDefault(a => a.Panel.Name == friendlyName) != null)
            return null;

        var element = new DetachedPanel(name, friendlyName, shortName, PanelHolder);

        if(prms != null)
            element.SetParameters(prms);

        if (element.Initialize())
        {
            DraggersList.Add(element.Dragger);
            ElementListPanel.AddElement(element);
            return element;
        }

        return null;
    }

    /// <summary>
    /// Clear all UI elements from the list by conditions
    /// </summary>
    /// <param name="clearControlPanels">Clear all objects even control panels</param>
    private static void ClearAllElements(bool clearControlPanels = true)
    {
        var elements = DraggersList.Where(a => clearControlPanels || !IsControlPanel(a.Panel)).ToArray();
        foreach (var panel in elements.Select(a => a.Panel))
            panel.Dispose();
        DraggersList.RemoveAll(a=> elements.Contains(a));

        if (!clearControlPanels)
            ElementListPanel.ClearList();
    }

    public void Dispose()
    {
        _isContentChanging = true;
        try
        {
            ClearAllElements();
            UnityEngine.Object.Destroy(CanvasRoot);
            UnityEngine.Object.Destroy(PoolHolder);
        }
        finally
        {
            _isContentChanging = false;
        }
    }

    /// <summary>
    /// Selects a panel and shows its outline
    /// </summary>
    /// <param name="panel">Panel</param>
    public static void SelectPanel(IGenericPanel panel)
    {
        if(panel == MainPanel || panel == ElementListPanel)
            return;

        // clear all panels outline
        foreach (var drag in DraggersList)
            drag.Panel.ShowPanelOutline(false);
            
        // do not select inactive panel
        if (!panel.IsRootActive)
        {
            MainPanel.DeselectCurrentPanel();
            return;
        }

        // select panel
        var dragger = DraggersList.FirstOrDefault(d => d.Panel == panel);
        DraggersList.Remove(dragger);
        DraggersList.Insert(0, dragger);
        panel.ShowPanelOutline(true);
        MainPanel.SelectedElementPanel = panel as ElementPanel;
    }

    /// <summary>
    /// Sets all panels active or inactive
    /// </summary>
    /// <param name="value">Is Active</param>
    public static void SetPanelsActive(bool value)
    {
        foreach (var panel in GetAllPanels())
        {
            panel.SetActive(value);
            if(value)
                panel.ShowPanelOutline(false);
        }

        MainPanel.SelectedElementPanel = null;
    }

    private static List<IGenericPanel> GetAllPanels()
    {
        return DraggersList.Where(drag => drag.Panel != MainPanel && drag.Panel != ElementListPanel)
            .Select(a => a.Panel).ToList();
    }
}