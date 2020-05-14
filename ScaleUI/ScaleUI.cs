using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.AccessControl;

namespace ScaleUI {
    public class ScaleUI : MonoBehaviour {
        private bool isInitialized;
        private bool isLinePositionsCached;
        private uint numTransportLines;
        private float thumbnailbarY = 0f;

        private UIView uiView;
        private UIComponent thumbnailBar;
        private UIComponent fullscreenContainer;
        private UIComponent infomenu;
        private UIComponent infomenuContainer;
        private UIComponent disasterWarnPanel;
        private UIComponent tsCloseButton;
        private UnityEngine.Object unlockingPanel;
        
        // Used to fix all UIComponents breaking
        private UIComponent[] allUIComponents;
        private Dictionary<int, Vector3> allUIComponentsCache;
        private UIComponent tsBar;
        private Vector3 tsBarCachedPos;
        
        // Used to fix transport  line breaking
        private Dictionary<String, Vector3> ltChildPositionsCache;

        public void Update() {
            if (!isInitialized || ModConfig.Instance.isApplyBtn) {
                isInitialized = true;
                ChangeScale(ModConfig.Instance.scale);
                ModConfig.Instance.ConfigUpdated = false;
                ModConfig.Instance.isApplyBtn = false;
            }

            if (ModConfig.Instance.isResetBtn) {
                SetDefaultScale();
                ModConfig.Instance.isResetBtn = false;
            }

            uint currNumLines = Singleton<TransportManager>.instance.m_lines.ItemCount();
            if (currNumLines > 2) {
                // If we started with no transport lines, we have to refresh the scale
                // to adjust the sizing of the first line's components, then cache the positions
                // of its components to re-use for alignment later
                if (!isLinePositionsCached) {
                    float oldScale = ModConfig.Instance.scale;
                    ChangeScale(ModConfig.Instance.scale + 0.0001f); // Re-aligns new transport line components
                    ChangeScale(oldScale);
                }
            }
            FixLinesOverview();
            FixAllComponentPositions();
        }

        public void Start() {
            try {
                uiView = UIView.GetAView();
                thumbnailBar = uiView.FindUIComponent("ThumbnailBar");
                fullscreenContainer = GameObject.Find("FullScreenContainer").GetComponent<UIComponent>();
                infomenu = GameObject.Find("InfoMenu").GetComponent<UIComponent>();
                infomenuContainer = GameObject.Find("InfoViewsContainer").GetComponent<UIComponent>();
                disasterWarnPanel = GameObject.Find("WarningPhasePanel").GetComponent<UIComponent>();
                tsCloseButton = GameObject.Find("TSCloseButton").GetComponent<UIComponent>();
                unlockingPanel = GameObject.FindObjectOfType(typeof(UnlockingPanel));
                ltChildPositionsCache = new Dictionary<string, Vector3>();
                
                
                allUIComponents = GameObject.FindObjectsOfType<UIComponent>();
                tsBar = GameObject.Find("TSBar").GetComponent<UIComponent>();
                allUIComponentsCache = new Dictionary<int, Vector3>();

                UIComponent tsContainer = GameObject.Find("TSContainer").GetComponent<UIComponent>();
                tsContainer.eventClicked += new MouseEventHandler(HideCloseButton);
                
                FixEverything();
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }

        public void ChangeScale(float scale) {
            uiView.scale = scale;
            CacheLinePositions();
            CacheAllUIComponents();
            FixEverything();
        }

        private void SetDefaultScale() {
            uiView.scale = 1f;
            CacheLinePositions();
            CacheAllUIComponents();
            FixEverything();
        }

        private void FixEverything() {
            try {
                FixCamera();
                FixPauseBorder();
                FixInfoMenu();
                FixInfoViewsContainer();
                FixDisasterDetection();
                FixPoliciesPanel();
                FixUnlockingPanel();
                tsCloseButton.position = new Vector3(-1000.0f, -1000.0f);
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }

        private void FixCamera() {
            if (uiView.scale < 1.0f) {
                MakeCameraFullscreen.Initialize();
            } else {
                if (MakeCameraFullscreen.cameraControllerRedirected) {
                    MakeCameraFullscreen.Deinitialize();
                }
            }
        }

        private void FixPauseBorder() {
            if (thumbnailbarY == 0f) {
                thumbnailbarY = thumbnailBar.relativePosition.y;
            }
            float diffHeight = thumbnailBar.relativePosition.y - thumbnailbarY;
            thumbnailbarY = thumbnailBar.relativePosition.y;

            fullscreenContainer.height += diffHeight;
            fullscreenContainer.relativePosition = new Vector2(0, 0);
        }

        private void FixInfoMenu() {
            infomenu.transformPosition = new Vector2(fullscreenContainer.GetBounds().min.x, fullscreenContainer.GetBounds().max.y);
            infomenu.relativePosition += new Vector3(70.0f, 6.0f);
        }

        private void FixInfoViewsContainer() {
            infomenuContainer.pivot = UIPivotPoint.TopCenter;
            infomenuContainer.transformPosition = new Vector3(infomenu.GetBounds().center.x, infomenu.GetBounds().min.y);
            infomenuContainer.relativePosition += new Vector3(-6.0f, 6.0f);
        }

        private void FixDisasterDetection() {
            const float OFFSET_X = 40.0f;
            const float OFFSET_Y = 3.0f;
           
            try {
                disasterWarnPanel.transformPosition = new Vector2(fullscreenContainer.GetBounds().min.x, fullscreenContainer.GetBounds().max.y);
                disasterWarnPanel.relativePosition += new Vector3(OFFSET_X, OFFSET_Y); // won't stick without doing it twice
                disasterWarnPanel.relativePosition += new Vector3(OFFSET_X, OFFSET_Y);
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }

        private void FixPoliciesPanel() {
            ToolsModifierControl.policiesPanel.component.height = fullscreenContainer.height - 65.0f;
        }

        private void FixUnlockingPanel() {
            ReflectionUtils.WritePrivate<UnlockingPanel>(unlockingPanel, "m_StartPosition", new UnityEngine.Vector3(-1f, 1f));
        }

        // If a new line was added, update the position of all its UIComponent
        // children  to the known good positions we saved earlier in our dict
        private void FixLinesOverview() {
            uint currNumLines = Singleton<TransportManager>.instance.m_lines.ItemCount();
            if (currNumLines > numTransportLines) {
                UIPanel[] uiPanels = GameObject.FindObjectsOfType<UIPanel>();
                for (int i = 0; i < uiPanels.Length; i++) {
                    var p = uiPanels[i].GetComponent<UIPanel>();
                    if (p.name == "LineTemplate(Clone)") {
                        UIComponent[] children = p.GetComponentsInChildren<UIComponent>();
                        foreach (var c in children) {
                            if (ltChildPositionsCache.TryGetValue(c.name, out Vector3 cachedPos)) {
                                c.position = cachedPos;
                            }
                        }
                    }
                }
            }
            numTransportLines = currNumLines;
        }

        // Very rarely the mod breaks the position of all UIComponents.
        // We cached their positions so we can restore them if this happens.
        // We check for the break by comparing the cached TSBar position with the
        // current one since if it is broken, likely everything else is as well.
        private void FixAllComponentPositions() {
            if (!tsBar.position.Equals(tsBarCachedPos)) {
                //DebugMsg("FIXALL | tsBar: " + tsBar.position.ToString() + " | tsBar_cached: " + tsBarCachedPos.ToString());
                foreach (var c in allUIComponents) {
                    if (c != null) { // For some reason, FindObjectsOfType() returns a handful of null results
                        int iid = c.GetInstanceID();
                        if (allUIComponentsCache.TryGetValue(iid, out Vector3 cachedPos)) {
                            c.position = cachedPos;
                        }
                    }
                }
                FixEverything(); // Have to apply fixes again
            }
        }
        
        // The close button on the TSBar sometimes stubbornly refuses to
        // disappear, so we hide it each time the bottom bar is clicked. 
        private void HideCloseButton(UIComponent component, UIMouseEventParameter eventParam) {
            UIComponent tsBar = GameObject.Find("TSBar").GetComponent<UIComponent>();
            tsBar.position = new Vector3(50f, 50f);
            tsCloseButton.position = new Vector3(-1000.0f, -1000.0f);
        }
        
        // Find an example transport line UIPanel with known good spacing 
        // and save the position of all its children .
        private void CacheLinePositions() {
            try {
                numTransportLines = Singleton<TransportManager>.instance.m_lines.ItemCount();
                UIComponent[] ltExampleChildren =
                    GameObject.Find("LineTemplate(Clone)").GetComponent<UIComponent>().GetComponentsInChildren<UIComponent>();
                foreach (var c in ltExampleChildren) {
                    String s = c.name;
                    if (!ltChildPositionsCache.ContainsKey(s)) {
                        ltChildPositionsCache.Add(s, new Vector3(c.position.x, c.position.y, c.position.z));
                    }
                }
                isLinePositionsCached = true;
            } catch (Exception ex) { 
            }
        }
        
        private void CacheAllUIComponents() {
            foreach (var c in allUIComponents) {
                if (c != null) { // For some reason, FindObjectsOfType() returns a handful of null results
                    allUIComponentsCache[c.GetInstanceID()] = new Vector3(c.position.x, c.position.y, c.position.z);
                } 
            }
            tsBarCachedPos = new Vector3(tsBar.position.x, tsBar.position.y, tsBar.position.z);
        }

       private void LogAllComponents() {
            var components = UnityEngine.Object.FindObjectsOfType<UIComponent>();
            foreach (var c in components) {
                Debug.Log("Comp: " + c.ToString());
            }
        } 

        private void DebugMsg(String s) {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, s);
        }
    }
}

