using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ScaleUI {
    public class ScaleUI : MonoBehaviour {
        private bool isInitialized;
        private bool isLinePositionsCached;
        private uint numTransportLines;
        private float thumbnailbarY = 0f;
        private float lastScale;

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
        private Vector3 tsBarCachedPos = Vector3.zero;
        
        // Used to fix transport line panel breaking
        private Dictionary<String, Vector3> ltChildPositionCache;
        
        // Used to fix mod button position breaking
        private Dictionary<UIComponent, Vector3> modPosCache;

        
        public void Update() {
            if (!isInitialized) InitModCache(); 

            if (!isInitialized || ModConfig.Instance.isApplyBtn) {
                ChangeScale(ModConfig.Instance.scale);
                ModConfig.Instance.isApplyBtn = false;
                lastScale = ModConfig.Instance.scale;
                isInitialized = true;
            }

            if (ModConfig.Instance.isResetBtn) {
                ChangeScale(1f);
                ModConfig.Instance.isResetBtn = false;
            }

            if (lastScale == ModConfig.Instance.scale) CacheModPositions();

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
                allUIComponents = GameObject.FindObjectsOfType<UIComponent>();
                allUIComponentsCache = new Dictionary<int, Vector3>();
                tsBar = GameObject.Find("TSBar").GetComponent<UIComponent>();
                ltChildPositionCache = new Dictionary<string, Vector3>();
                modPosCache = new Dictionary<UIComponent, Vector3>();
                lastScale = ModConfig.Instance.scale;
                UIComponent tsContainer = GameObject.Find("TSContainer").GetComponent<UIComponent>();
                tsContainer.eventClicked += new MouseEventHandler(HideCloseButton);
                FixEverything();
            } catch (Exception ex) {
                DebugMsg("ScaleUI: " + ex.ToString());
            }
        }

        public void ChangeScale(float scale) {
            DebugMsg("scale: " + scale.ToString());
            uiView.scale = scale;
            CacheLinePositions();
            CacheAllUIComponents();
            FixEverything();
            FixModPositions();
            lastScale = scale;
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
                DebugMsg("ScaleUI: " + ex.ToString());
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
            if (thumbnailbarY == 0f) 
                thumbnailbarY = thumbnailBar.relativePosition.y;

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
            if (disasterWarnPanel != null) {
                disasterWarnPanel.transformPosition = new Vector2(fullscreenContainer.GetBounds().min.x, fullscreenContainer.GetBounds().max.y);
                disasterWarnPanel.relativePosition += new Vector3(OFFSET_X, OFFSET_Y); // won't stick without doing it twice
                disasterWarnPanel.relativePosition += new Vector3(OFFSET_X, OFFSET_Y);
            }
        }

        private void FixPoliciesPanel() {
            ToolsModifierControl.policiesPanel.component.height = fullscreenContainer.height - 65.0f;
        }

        private void FixUnlockingPanel() {
            ReflectionUtils.WritePrivate<UnlockingPanel>(unlockingPanel, "m_StartPosition", new UnityEngine.Vector3(-1f, 1f));
        }
        
        private void HideCloseButton(UIComponent component, UIMouseEventParameter eventParam) {
            tsCloseButton.position = new Vector3(-1000.0f, -1000.0f);
        }
    
        private void InitModCache() {
            UIComponent[] mods = {
                    GameObject.Find("MoveIt").GetComponent<UIComponent>(),
                    GameObject.Find("MainMenuButton").GetComponent<UIComponent>() // TMPE
            };

            foreach (var c in mods) 
                if (c != null && !modPosCache.ContainsKey(c)) 
                    modPosCache.Add(c, c.absolutePosition * ModConfig.Instance.scale);
        }
        
        private void CacheModPositions() {
            foreach (var k in modPosCache.Keys.ToList()) 
                modPosCache[k] = k.absolutePosition * ModConfig.Instance.scale;
        }
        private void FixModPositions() {
            foreach (var e in modPosCache) 
                if (e.Key != null)
                    e.Key.absolutePosition = e.Value / ModConfig.Instance.scale;
        }

        // Find an example transport line UIPanel with known good spacing 
        // and save the position of all its children .
        private void CacheLinePositions() {
            try {
                numTransportLines = Singleton<TransportManager>.instance.m_lines.ItemCount();
                if (numTransportLines > 2) {
                    UIComponent[] lineTemplateChildren = 
                        GameObject.Find("LineTemplate(Clone)").GetComponent<UIComponent>().GetComponentsInChildren<UIComponent>();
                    foreach (var c in lineTemplateChildren) 
                        ltChildPositionCache[c.name] = new Vector3(c.position.x, c.position.y, c.position.z);
                    isLinePositionsCached = true;
                }
            } catch (Exception ex) {
                DebugMsg("ScaleUI: " + ex.ToString());
            }
        }

        // If a new line was added, update the position of all its UIComponent
        // children  to the known good positions we saved earlier in our dict
        private void FixLinesOverview() {
            uint currNumLines = Singleton<TransportManager>.instance.m_lines.ItemCount();

            // If we started with no transport lines, we have to refresh the scale
            // to adjust the sizing of the first line's components, then cache the positions
            // of its components to re-use for alignment later
            if (!isLinePositionsCached && currNumLines > 2) {
                float oldScale = ModConfig.Instance.scale;
                ChangeScale(ModConfig.Instance.scale + 0.1f); // Re-aligns new transport line components
                ChangeScale(oldScale);
            }

            if (currNumLines > numTransportLines) 
                foreach (var p in GameObject.FindObjectsOfType<UIPanel>()) 
                    if (p.name == "LineTemplate(Clone)") 
                        foreach (var c in p.GetComponentsInChildren<UIComponent>()) 
                            if (ltChildPositionCache.TryGetValue(c.name, out Vector3 cachedPos)) 
                                c.position = cachedPos;
            numTransportLines = currNumLines;
        }

        private void CacheAllUIComponents() {
            try {
                foreach (var c in allUIComponents) 
                    if (c != null) // For some reason, FindObjectsOfType() returns a handful of null results
                        allUIComponentsCache[c.GetInstanceID()] = new Vector3(c.position.x, c.position.y, c.position.z);
                tsBarCachedPos = new Vector3(tsBar.position.x, tsBar.position.y, tsBar.position.z);
            } catch (Exception ex) {
                DebugMsg("ScaleUI: " + ex.ToString());
            }
        }

        // Very rarely the mod breaks the position of all UIComponents.
        // We cached their positions so we can restore them if this happens.
        // We check for the break by comparing the cached TSBar position with the
        // current one since if it is broken, likely everything else is as well.
        private void FixAllComponentPositions() {
            try {
                if (!tsBar.position.Equals(tsBarCachedPos)) {
                    Debug.Log("FIXALL | tsBar: " + tsBar.position.ToString() + " | tsBar_cached: " + tsBarCachedPos.ToString());
                    DebugMsg("FIXALL | tsBar: " + tsBar.position.ToString() + " | tsBar_cached: " + tsBarCachedPos.ToString());
                    foreach (var c in allUIComponents) 
                        if (c != null)  // For some reason, FindObjectsOfType() returns a handful of null results
                            if (allUIComponentsCache.TryGetValue(c.GetInstanceID(), out Vector3 cachedPos)) 
                                c.position = cachedPos;
                    FixEverything(); // Have to apply fixes again
                }
            } catch (Exception ex) {
                DebugMsg("ScaleUI: " + ex.ToString());
            }
        }

        private void DebugMsg(String s) {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, s);
        }
    }
}

