using System.Collections;
using System.Collections.Generic;
using UIBuddy.Classes;
using UIBuddy.UI.ScrollView.Cells.Handlers;
using UIBuddy.UI.ScrollView.Cells;
using UIBuddy.UI.ScrollView;
using UnityEngine;
using UnityEngine.UI;
using UIBuddy.UI.Classes;

namespace UIBuddy.UI.Panel
{
    public class ElementListPanel: GenericPanelBase
    {
        private ScrollPool<ButtonCell> _scrollPool;
        private ButtonListHandler<ElementPanelData, ButtonCell> _scrollDataHandler;
        private readonly List<ElementPanelData> _dataList = new();
        private GameObject _titleBar;
        public UIElementDragEx Dragger { get; protected set; }

        public ElementListPanel(GameObject parent) 
            : base(parent, nameof(ElementListPanel))
        {
            ConstructUI();
        }

        protected override void ConstructUI()
        {
            SetActive(true);

            _titleBar = UIFactory.CreateUIObject($"TitleBar_{nameof(ElementListPanel)}", RootObject);
            Dragger = new UIElementDragEx(_titleBar, this);

            // Set size for the main panel
            RootRect.sizeDelta = new Vector2(200, 400);
            // Add background image
            var bgImage = RootObject.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.PanelBackground;

            var contentArea = UIFactory.CreateUIObject($"ContentArea_{nameof(ElementListPanel)}", RootObject);
            var contentRect = contentArea.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            // Position below the title bar
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, -35); // Height of the title bar

            _scrollDataHandler = new ButtonListHandler<ElementPanelData, ButtonCell>(_scrollPool, GetEntries, SetCell, ShouldDisplay, OnCellClicked);
            _scrollPool = UIFactory.CreateScrollPool<ButtonCell>(contentArea, $"ContentList_{nameof(ElementListPanel)}", out GameObject scrollObj,
                out _, new Color(0.03f, 0.03f, 0.03f, Theme.Opacity));
            _scrollPool.Initialize(_scrollDataHandler);
            /////PanelManager.AddScrollPool(_scrollPool);
            UIFactory.SetLayoutElement(scrollObj, flexibleHeight: 9999);



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

            UIFactory.CreateLabel(_titleBar, $"TitleLabel_{nameof(ElementListPanel)}", "UIBuddy Panel List", fontSize: 18);
        }

        private IEnumerator LateSetupCoroutine()
        {
            yield return null;

            // Create title bar (must be created first as we'll use it for the dragger)
            CreateTitleBar(RootObject);


            // Activate the UI
            RootObject.SetActive(true);
        }

        public void AddElement(IGenericPanel panel)
        {
            var data = new ElementPanelData { Panel = panel };
            _dataList.Add(data);
        }

        private void OnCellClicked(int dataIndex)
        {
            var famBox = _dataList[dataIndex];
           ///////
        }

        private bool ShouldDisplay(ElementPanelData data, string filter) => true;
        private List<ElementPanelData> GetEntries() => _dataList;

        private void SetCell(ButtonCell cell, int index)
        {
            if (index < 0 || index >= _dataList.Count)
            {
                cell.Disable();
                return;
            }
            cell.Button.ButtonText.text = _dataList[index].Panel.Name;
        }
    }

    internal class ElementPanelData
    {
        public IGenericPanel Panel { get; set; }
    }
}
