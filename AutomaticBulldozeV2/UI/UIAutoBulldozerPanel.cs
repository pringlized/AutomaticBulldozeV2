using AutomaticBulldozeV2.UI.Localization;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace AutomaticBulldozeV2.UI
{
    public class UIAutoBulldozerPanel : UIPanel
    {
        private UIButton _demolishAbandonedButton;
        private UIButton _demolishBurnedButton;

        public static readonly SavedBool DemolishAbandoned = new SavedBool("ModDemolishAbandoned", Settings.gameSettingsFile, true, true);
        public static readonly SavedBool DemolishBurned = new SavedBool("ModDemolishBurned", Settings.gameSettingsFile, true, true);

        private static void InitButton(UIButton button)
        {
            var sprite = "SubBarButtonBase";
            var spriteHov = sprite + "Hovered";
            button.normalBgSprite = spriteHov;
            button.disabledBgSprite = spriteHov;
            button.hoveredBgSprite = spriteHov;
            button.focusedBgSprite = spriteHov;
            button.pressedBgSprite = sprite + "Pressed";
            button.textColor = new Color32(255, 255, 255, 255);
        }

        private void UpdateCheckButton(UIButton button, bool isActive)
        {
            var inactiveColor = new Color32(64, 64, 64, 255);
            var activeColor = new Color32(255, 64, 64, 255);
            var textColor = new Color32(255, 255, 255, 255);
            var textColorDis = new Color32(128, 128, 128, 255);

            if (isActive)
            {
                button.color = activeColor;
                button.focusedColor = activeColor;
                button.hoveredColor = activeColor;
                button.pressedColor = activeColor;
                button.textColor = textColor;
            }
            else
            {
                button.color = inactiveColor;
                button.focusedColor = inactiveColor;
                button.hoveredColor = inactiveColor;
                button.pressedColor = inactiveColor;
                button.textColor = textColorDis;
            }

            button.Unfocus();
        }

        private void SetLocales()
        {
            var bWidth = LocalizationManager.GetButtonWidth();
            _demolishAbandonedButton.text = "Switch.DemolishAbandoned".Translate();
            _demolishAbandonedButton.width = bWidth;
            _demolishBurnedButton.text = "Switch.DemolishBurned".Translate();
            _demolishBurnedButton.width = bWidth;
        }

        public override void Start()
        {
            // configure panel
            this.height = 50;
            this.autoLayout = true;
            this.autoLayoutDirection = LayoutDirection.Horizontal;
            this.autoLayoutPadding = new RectOffset(0, 10, 0, 0);
            this.autoLayoutStart = LayoutStart.TopLeft;

            _demolishAbandonedButton = this.AddUIComponent<UIButton>();
            _demolishAbandonedButton.width = 200;
            _demolishAbandonedButton.height = 50;
            InitButton(_demolishAbandonedButton);
            _demolishAbandonedButton.eventClick += (component, param) =>
            {
                DemolishAbandoned.value = !DemolishAbandoned.value;
                UpdateCheckButton(_demolishAbandonedButton, DemolishAbandoned.value);
            };

            _demolishBurnedButton = this.AddUIComponent<UIButton>();
            
            _demolishBurnedButton.width = 200;
            _demolishBurnedButton.height = 50;
            InitButton(_demolishBurnedButton);
            _demolishBurnedButton.eventClick += (component, param) =>
            {
                DemolishBurned.value = !DemolishBurned.value;
                UpdateCheckButton(_demolishBurnedButton, DemolishBurned.value);
            };

            UpdateCheckButton(_demolishAbandonedButton, DemolishAbandoned.value);
            UpdateCheckButton(_demolishBurnedButton, DemolishBurned.value);

            SetLocales();
            LocalizationManager.Instance.eventLocaleChanged += language => SetLocales();
            
            base.Start();
        }
    }
}
