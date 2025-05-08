using System.Collections;
using System.Collections.Generic;
using UIBuddy.Classes;
using UIBuddy.UI.ScrollView.Cells.Handlers;
using UIBuddy.UI.ScrollView.Cells;
using UIBuddy.UI.ScrollView;
using UnityEngine;
using UnityEngine.UI;
using UIBuddy.UI.Classes;
using Unity.Entities.UniversalDelegates;

namespace UIBuddy.UI.Panel
{
    public class ElementListPanel: GenericPanelBase
    {
        private ScrollPool<CheckButtonCell> _scrollPool;
        private ButtonListHandler<ElementPanelData, CheckButtonCell> _scrollDataHandler;
        private readonly List<ElementPanelData> _dataList = new();
        private GameObject _titleBar;

        public ElementListPanel(GameObject parent) 
            : base(parent, nameof(ElementListPanel))
        {
        }

        protected override void ConstructUI()
        {
            SetActive(true);

            // Set size for the main panel
            RootRect.sizeDelta = new Vector2(400, 400);

            // Add background image
            var bgImage = RootObject.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.PanelBackground;

            // Create title bar
            _titleBar = UIFactory.CreateUIObject($"TitleBar_{nameof(ElementListPanel)}", RootObject);
            ConstructDrag(_titleBar);

            // Create content area that takes all space below the title bar
            var contentArea = UIFactory.CreateUIObject($"ContentArea_{nameof(ElementListPanel)}", RootObject);
            var contentRect = contentArea.GetComponent<RectTransform>();

            // This is critical for proper sizing:
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, -35); // Account for title bar height

            // Critical: Create the scroll pool with data handler
            _scrollDataHandler = new ButtonListHandler<ElementPanelData, CheckButtonCell>(null, GetEntries, SetCell, ShouldDisplay, OnCellClicked);

            // Create the scroll pool, outputting the scrollObj
            _scrollPool = UIFactory.CreateScrollPool<CheckButtonCell>(
                contentArea,
                $"ContentList_{nameof(ElementListPanel)}",
                out GameObject scrollObj,
                out _,
                new Color(0.03f, 0.03f, 0.03f, Theme.Opacity));

            // Set up the scroll object to fill the entire content area
            var scrollRect = scrollObj.GetComponent<RectTransform>();
            if (scrollRect != null)
            {
                scrollRect.anchorMin = Vector2.zero;
                scrollRect.anchorMax = Vector2.one;
                scrollRect.pivot = new Vector2(0.5f, 0.5f);
                scrollRect.anchoredPosition = Vector2.zero;
                scrollRect.sizeDelta = Vector2.zero; // This ensures it fills the parent
            }

            // Initialize the scroll pool
            _scrollDataHandler = new ButtonListHandler<ElementPanelData, CheckButtonCell>(_scrollPool, GetEntries, SetCell, ShouldDisplay, OnCellClicked);
            _scrollPool.Initialize(_scrollDataHandler);

            CoroutineUtility.StartCoroutine(LateSetupCoroutine());
        }

        private void CreateTitleBar(GameObject parent)
        {
            var titleRect = _titleBar.GetComponent<RectTransform>();

            // Position the title bar at the top of the panel
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 35); // Set height to 35
            titleRect.anchoredPosition = Vector2.zero;

            // Add background image
            var bgImage = _titleBar.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.SliderNormal;

            // Create a horizontal layout for the title label
            var titleLayout = UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(_titleBar,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 0,
                padTop: 0,
                padBottom: 0,
                padLeft: 10,
                padRight: 10,
                childAlignment: TextAnchor.MiddleCenter);

            UIFactory.CreateLabel(_titleBar, $"TitleLabel_{nameof(ElementListPanel)}", "UIBuddy Element List", fontSize: 18);
        }

        private IEnumerator LateSetupCoroutine()
        {
            yield return null;

            CreateTitleBar(RootObject);
            if(RootRect.anchoredPosition == Vector2.zero)
                RootRect.anchoredPosition = new Vector2(RootRect.anchoredPosition.x + 350, RootRect.anchoredPosition.y);
            // Activate the UI
            RootObject.SetActive(true);
        }

        public void AddElement(IGenericPanel panel)
        {
            var data = new ElementPanelData { Panel = panel };
            _dataList.Add(data);
            _scrollDataHandler.RefreshData();
            _scrollPool.Refresh(true);
        }

        public void RemoveElement(IGenericPanel panel)
        {
            var data = _dataList.Find(d => d.Panel == panel);
            if (data != null)
            {
                _dataList.Remove(data);
                _scrollDataHandler.RefreshData();
                _scrollPool.Refresh(true);
            }
        }

        private void OnCellClicked(int dataIndex)
        {
            var data = _dataList[dataIndex];
            PanelManager.SelectPanel(data.Panel);
            UpdateSelectedEntry(dataIndex);
        }

        private bool ShouldDisplay(ElementPanelData data, string filter) => true;
        private List<ElementPanelData> GetEntries() => _dataList;

        private void SetCell(CheckButtonCell cell, int index)
        {
            if (index < 0 || index >= _dataList.Count)
            {
                cell.Disable();
                return;
            }
            var data = _dataList[index];
            cell.Button.ButtonText.text = data.Panel.Name;
            cell.SetInitialToggleValue(data.Panel.IsRootActive);
            cell.OnToggleValueChanged += value =>
            {
                data.Panel.SetRootActive(value);
                if(value)
                    PanelManager.SelectPanel(data.Panel);
                UpdateSelectedEntry(value ? index : -1);
            };
        }

        public void UpdateSelectedEntry(int dataIndex)
        {
            _scrollPool.CellPool.ForEach(a =>
            {
                if(a.CurrentDataIndex == dataIndex && dataIndex != -1)
                    a.Button.ButtonText.color = Color.yellow;
                else a.Button.ButtonText.color = Theme.DefaultText;
            });
        }

        public void UpdateElement(ElementPanel panel)
        {
            _scrollDataHandler.RefreshData();
            _scrollPool.Refresh(true);
        }
    }

    internal class ElementPanelData
    {
        public IGenericPanel Panel { get; set; }
    }
}
