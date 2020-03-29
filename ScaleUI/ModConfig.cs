using System.IO;

namespace ScaleUI {
    [ConfigurationPath("ScaleUIConfig.xml")]
    public class ModConfig {
        public bool ConfigUpdated { get; set; }
        public float scale { get; set; } = 1.0f;
        public bool isApplyBtn { get; set; } = false;
        public bool isResetBtn { get; set; } = false;

        private static ModConfig instance;

        public static ModConfig Instance {
            get {
                if (instance == null) {
                    instance = Configuration<ModConfig>.Load();
                    
                    if (!File.Exists(Configuration<ModConfig>.GetConfigPath())) 
                        instance.Save();
                }
                return instance;
            }
        }

        public void Save() {
            Configuration<ModConfig>.Save();
            ConfigUpdated = true;
        }
    }
}
