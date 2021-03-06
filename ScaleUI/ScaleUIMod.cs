using ICities;
using ColossalFramework.UI;
using System;

namespace ScaleUI {
    public class ScaleUIMod : IUserMod {
        public string Name => "ScaleUI ";
        public string Description => "Adds slider in options to scale the UI.";
        
        private const float MIN_SCALE = 0.4f;
        private const float MAX_SCALE = 1.4f;
        private const float INCR_SCALE = 0.05f;

        public void OnSettingsUI(UIHelperBase helper) {
            UIHelperBase group;
            float selectedValue;

            group = helper.AddGroup(Name);
            selectedValue = ModConfig.Instance.scale;
            
            UITextField scaleText = (UITextField)group.AddTextfield("Scale", selectedValue.ToString(), (t) => { });
            scaleText.readOnly = true;
            scaleText.horizontalAlignment = UIHorizontalAlignment.Center;
            scaleText.canFocus = false;
            scaleText.normalBgSprite = "";
            scaleText.hoveredBgSprite = "";

            UISlider scaleSlider = (UISlider) group.AddSlider(" ", MIN_SCALE, MAX_SCALE, INCR_SCALE, selectedValue, sel => {
                scaleText.text = sel.ToString();
                ModConfig.Instance.scale = sel;
                ModConfig.Instance.Save();
            });

            group.AddButton("Apply", () => {
                ModConfig.Instance.isApplyBtn = true;
            });

            group.AddButton("Reset", () => {
                ModConfig.Instance.scale = 1.0f;
                ModConfig.Instance.isResetBtn = true;
                scaleSlider.value = ModConfig.Instance.scale;
            });
        }
    }
}