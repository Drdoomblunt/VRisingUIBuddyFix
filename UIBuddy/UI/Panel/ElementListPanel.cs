using UnityEngine;

namespace UIBuddy.UI.Panel
{
    public class ElementListPanel: GenericPanelBase
    {
        public ElementListPanel(GameObject parent) 
            : base(parent, nameof(ElementListPanel))
        {
            ConstructUI();
        }

        protected override void ConstructUI()
        {

        }
    }
}
