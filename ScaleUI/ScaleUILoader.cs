using ICities;
using System;
using UnityEngine;

namespace ScaleUI {
    public class ScaleUILoader : LoadingExtensionBase {
        GameObject go;

        public override void OnLevelLoaded(LoadMode mode) {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame) {
                return;
            }

            go = GameObject.Find("ScaleUI");
            if (go == null) {
                go = new GameObject("ScaleUI");
                go.AddComponent<ScaleUI>();
            }
        }

        public override void OnLevelUnloading() {
            try {
                GameObject.Destroy(go);
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }
    }
}
