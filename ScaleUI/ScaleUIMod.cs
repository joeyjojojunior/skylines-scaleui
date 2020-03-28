using ICities;

namespace ScaleUI {
    public class ScaleUIMod : IUserMod {
        public string Name {
            get {
                return "ScaleUI2";
            }
        }

        public string Description {
            get {
                return "Adds slider in options to scale the complete UI.";
            }
        }
        private const float MIN_SCALE = 0.3f;
        private const float MAX_SCALE = 1.5f;
        private const float INCR_SCALE = 0.05f;

        public void OnSettingsUI(UIHelperBase helper) {
            UIHelperBase group;
            float selectedValue;

            group = helper.AddGroup(Name);
            selectedValue = ModConfig.Instance.scale;
            group.AddSlider("Scale", MIN_SCALE, MAX_SCALE, INCR_SCALE, selectedValue, sel => {
                ModConfig.Instance.scale = sel;
                ModConfig.Instance.Save();
            });

            group.AddButton("Apply", () => {
                ModConfig.Instance.isApplyBtn = true;
            });

            group.AddButton("Reset", () => {
                ModConfig.Instance.isResetBtn = true;
            });
        }
    }
}