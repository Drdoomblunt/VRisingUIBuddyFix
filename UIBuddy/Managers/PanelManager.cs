using System;
using System.Collections.Generic;
using System.Linq;
using UIBuddy.UI;
using UIBuddy.UI.Classes;
using UIBuddy.UI.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.Managers;

public class PanelManager: IDisposable
{
    private Vector3 _previousMousePosition;
    private MouseState.ButtonState _previousMouseButtonState;
    private static readonly List<IUIElementDrag> DraggersList = new();
    public static bool WasAnyDragging;
    public static bool DraggerHandledThisFrame;
    protected virtual bool MouseInTargetDisplay => true;
    public static GameObject PoolHolder { get; private set; }

    public static PanelManager Instance { get; private set; }
    private static GameObject CanvasRoot { get; set; }
    private static CanvasScaler Scaler { get; set; }
    private static Canvas Canvas { get; set; }
    private static GameObject PanelHolder { get; set; }

    // Main control panel
    public static  MainControlPanel MainPanel { get; private set; }
    public static ElementListPanel ElementListPanel { get; private set; }
    private static bool _focusHandledThisFrame;

    protected virtual bool ShouldUpdateFocus =>
        MouseInTargetDisplay &&
        InputManager.Mouse.Button0 == MouseState.ButtonState.Down &&
        !WasAnyDragging;

    public PanelManager()
    {
        Instance = this;
        CreateRootCanvas();
        CreateElementListPanel();
        CreateMainPanel();
    }

    public static void ReloadElements()
    {
        AddDrag("BloodOrbParent", "Blood Orb HP", "ORB");
        //AddDetachedDrag("BottomBar(Clone)|Content|TooltipParent|BloodPoolTooltip", "Blood Orb Tooltip Anchor", "BOTA");

        AddDrag("JournalParent(Clone)", "Journal", "JOU");
        AddDrag("TargetInfoPanel(Clone)", "Target Info", "TGT");

        AddDrag("ClockParent3(Clone)|Content|Parent", "DayNight sphere", "SPHERE"); //daytime circle
        AddDrag("MiniMapParent(Clone)|Root|Panel", "Minimap", "MAP"); //minimap
        AddDrag("BackgroundBig", "Clock+Minimap Background", "Background"); //clock/minimap background

        AddDrag("BottomBar(Clone)", "Full Bottom Bar", "FBB"); //bottom bar
        AddDrag("BottomBar(Clone)|Content|Background|Background", "Bottom Bar Fade", "BBF"); //bottom bar fade
        AddDrag("BottomBar(Clone)|Content|Background|DarkFade", "Bottom Bar Background", "BBB"); //bottom bar bg
        AddDetachedDrag("BottomBar(Clone)|Content|Background|ActionBar", "Action Bar", "ACBAR"); //action bar
        AddDetachedDrag("BottomBar(Clone)|Content|Background|AbilityBar", "Ability Bar", "ABBAR"); //abilityBar bar

        AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry", "Action Bar Button 1", "ACB1"); //ab1
        AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry (1)", "Action Bar Button 2", "ACB2"); //ab2
        AddDrag("AbilityBarEntry_Primary", "AB primary", "ABP");

        AddDrag("Buffs", "Buffs", "BUFF");
        AddDrag("Debuffs", "Debuffs", "DBUFF");

        AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|HUDAlertParent(Clone)|Container", "(?)Right Alerts", "RA"); //right alerts
        AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|DangerTextParent(Clone)", "(!!)Bottom Danger", "DANGER"); //bottom danger text
        //var chatPanel = _pm.AddDetachedDrag("HUDChatParent|ChatWindow(Clone)|Content", "Chat Window", "CHAT"); //chat
        //chatPanel?.SetParameters(positionValidation: false, initialPosition: new Vector2(500,500));

        //AddDrag("Version_HUD", "Clan Members");
        //AddDrag("HUDClan", "Clan Members", "CLAN"); //clan
        // _pm.AddDrag("HUDTutorial"); //tutorial
        // _pm.AddDrag("HUDRecipeTrackerParent"); //recipe tracker

        /*_pm.AddDrag("SLS logo");
        _pm.AddDrag("NewsPanelParent");
        _pm.AddDrag("SideBar");
        _pm.AddDrag("LinksParentNode");*/
        // _pm.AddDrag(null);
    }

    private static void CreateElementListPanel()
    {
        ElementListPanel = new ElementListPanel(PanelHolder);
        ElementListPanel.Initialize();
        DraggersList.Add(ElementListPanel.Dragger);
        Plugin.Log.LogInfo("Elements list panel created and initialized successfully");
    }

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


    public void Update()
    {
        if(!MainPanel.IsRootActive)
            return;

        if (ConfigManager.SelectPanelsWithMouse && ShouldUpdateFocus)
            UpdateFocus();

        foreach (var panel in DraggersList.Select(a=> a.Panel))
        {
            panel.Update();
        }

        if (!DraggerHandledThisFrame)
            UpdateDraggers();

        DraggerHandledThisFrame = false;
        _focusHandledThisFrame = false;
    }

    protected virtual void UpdateFocus()
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

    protected virtual void UpdateDraggers()
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

                if (DraggerHandledThisFrame)
                    break;
            }
               
        }

        if (WasAnyDragging && state.HasFlag(MouseState.ButtonState.Up))
        {
            foreach (var instance in DraggersList)
                instance.WasDragging = false;
            WasAnyDragging = false;
        }
    }

    private static void AddDrag(string gameObjectName, string friendlyName, string shortName = null)
    {
        if(DraggersList.FirstOrDefault(a=> a.Panel.Name == friendlyName) != null)
            return;

        var element = new ElementPanel(gameObjectName, friendlyName, shortName);

        if (element.Initialize())
        {
            DraggersList.Add(element.Dragger);
            ElementListPanel.AddElement(element);
        }
    }


    private static DetachedPanel AddDetachedDrag(string name, string friendlyName, string shortName)
    {
        if (DraggersList.FirstOrDefault(a => a.Panel.Name == friendlyName) != null)
            return null;

        var element = new DetachedPanel(name, friendlyName, shortName, PanelHolder);

        if (element.Initialize())
        {
            DraggersList.Add(element.Dragger);
            ElementListPanel.AddElement(element);
            return element;
        }

        return null;
    }

    public void Dispose()
    {
        foreach (var panel in DraggersList.Select(a=> a.Panel))
            panel.Dispose();
        DraggersList.Clear();
        UnityEngine.Object.Destroy(CanvasRoot);
        UnityEngine.Object.Destroy(PoolHolder);
    }

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
            MainPanel.SelectedElementPanel = null;
            return;
        }

        // select panel
        var dragger = DraggersList.FirstOrDefault(d => d.Panel == panel);
        DraggersList.Remove(dragger);
        DraggersList.Insert(0, dragger);
        panel.ShowPanelOutline(true);
        MainPanel.SelectedElementPanel = panel as ElementPanel;
    }

    public static void SetPanelsActive(bool value)
    {
        foreach (var panel in GetAllPanels())
        {
            panel.SetActive(value);
        }
    }

    private static List<IGenericPanel> GetAllPanels()
    {
        return DraggersList.Where(drag => drag.Panel != MainPanel && drag.Panel != ElementListPanel)
            .Select(a => a.Panel).ToList();
    }
}