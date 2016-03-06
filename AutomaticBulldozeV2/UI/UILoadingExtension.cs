using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace AutomaticBulldozeV2.UI
{
    public class UILoadingExtension : LoadingExtensionBase
    {
        private LoadMode _mode;
        private UIAutoBulldozerPanel _autoBulldozerPanel;

        private void InitWindows()
        {
            var bulldozerBar = UIView.Find("BulldozerBar");
            _autoBulldozerPanel = bulldozerBar.AddUIComponent<UIAutoBulldozerPanel>();
            _autoBulldozerPanel.relativePosition = new Vector3(10.0f, -20.0f);
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
                return;
            _mode = mode;

            InitWindows();
        }

        public override void OnLevelUnloading()
        {
            if (_mode != LoadMode.LoadGame && _mode != LoadMode.NewGame)
                return;

            if (_autoBulldozerPanel != null)
                Object.Destroy(_autoBulldozerPanel);
        }
    }
}
