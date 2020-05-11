using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScaleUI {
    public class ScaleUI : MonoBehaviour {
        private bool isInitialized;
        private uint num_transport_lines;
        private float thumbnailbarY = 0f;
        private Vector2 CLOSEBTN_HIDE_POS = new Vector3(-1000f, -1000f);

        private UIView uiView;
        private UIComponent fullscreenContainer;
        private UIComponent infomenu;
        private UIComponent infomenuContainer;
        private UIComponent disasterWarnPanel;
        private UIComponent tsCloseButton;

        private List<UIComponent> lineTemplates;
        private UIComponent ltExample;
        private UIComponent[] ltExampleChildren;
        Dictionary<String, Vector3> ltChildPositions;

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
            FixLinesOverview();
        }

        public void Start() {
            try {
                num_transport_lines = Singleton<TransportManager>.instance.m_lines.ItemCount();
                uiView = UIView.GetAView();
                fullscreenContainer = GameObject.Find("FullScreenContainer").GetComponent<UIComponent>();
                infomenu = GameObject.Find("InfoMenu").GetComponent<UIComponent>();
                infomenuContainer = GameObject.Find("InfoViewsContainer").GetComponent<UIComponent>();
                disasterWarnPanel = GameObject.Find("WarningPhasePanel").GetComponent<UIComponent>();
                tsCloseButton = GameObject.Find("TSCloseButton").GetComponent<UIComponent>();

                UIComponent tsContainer = GameObject.Find("TSContainer").GetComponent<UIComponent>();
                tsContainer.eventClicked += new MouseEventHandler(HideCloseButton);

                ltExample = GameObject.Find("LineTemplate(Clone)").GetComponent<UIComponent>();
                ltExampleChildren = ltExample.GetComponentsInChildren<UIComponent>();

                ltChildPositions = new Dictionary<string, Vector3>();

                foreach (var c in ltExampleChildren) {
                    String s = c.name;
                    if (!ltChildPositions.ContainsKey(s)) {
                        ltChildPositions.Add(s, new Vector3(c.position.x, c.position.y, c.position.z));
                    }
                }
                /*
                foreach (var c in ltChildPositions) {
                    Debug.Log(c.Key + ", " + c.Value.ToString());
                }
                */
                

                // Get all current transport line panels
                //UIPanel[] uiPanels = GameObject.FindObjectsOfType<UIPanel>();
                //lineTemplates = new List<UIComponent>();
                //or (int i = 0; i < uiPanels.Length; i++) {
                //    if (uiPanels[i].GetComponent<UIPanel>().name == "LineTemplate(Clone)") {
                //        lineTemplates.Add(uiPanels[i].GetComponent<UIPanel>());
                //    }
                //}
                

                FixEverything();
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }

        private void FixLinesOverview() {
            uint curr_num_lines = Singleton<TransportManager>.instance.m_lines.ItemCount();
            if (curr_num_lines > num_transport_lines) {
                UIPanel[] uiPanels = GameObject.FindObjectsOfType<UIPanel>();
                for (int i = 0; i < uiPanels.Length; i++) {
                    var p = uiPanels[i].GetComponent<UIPanel>();
                    if (p.name == "LineTemplate(Clone)") {
                        // Update position of new line's UIPanel children to match our known good template
                        UIComponent[] children = p.GetComponentsInChildren<UIComponent>();
                        foreach (var c in children) {
                            Vector3 pos;
                            if (ltChildPositions.TryGetValue(c.name, out pos)) {
                                c.position = pos;
                            }
                        }
                    }
                }
            }
            num_transport_lines = curr_num_lines;
        }

        public void ChangeScale(float scale) {
            uiView.scale = scale;
            FixEverything();
        }

        private void SetDefaultScale() {
            uiView.scale = 1f;
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
            UIComponent uic;
            uic = uiView.FindUIComponent("ThumbnailBar");
            if (thumbnailbarY == 0f) {
                thumbnailbarY = uic.relativePosition.y;
            }
            float diffHeight = uic.relativePosition.y - thumbnailbarY;
            thumbnailbarY = uic.relativePosition.y;

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
            } catch (Exception ex) {}
        }

        private void FixPoliciesPanel() {
            ToolsModifierControl.policiesPanel.component.height = fullscreenContainer.height - 65.0f;
        }

        private void FixUnlockingPanel() {
            UnityEngine.Object obj = GameObject.FindObjectOfType(typeof(UnlockingPanel));
            ReflectionUtils.WritePrivate<UnlockingPanel>(obj, "m_StartPosition", new UnityEngine.Vector3(-1f, 1f));
        }
        
        private void HideCloseButton(UIComponent component, UIMouseEventParameter eventParam) {
            tsCloseButton.position = CLOSEBTN_HIDE_POS;
        }

        private void ltMouse(UIComponent component, UIMouseEventParameter eventParam) {
            /*
            DebugMsg("Mouse Before: " + component.transform.localScale.ToString());
            component.transform.localScale = new Vector3(0.90f, 1f, 1f);
            DebugMsg("Mouse After: " + component.transform.localScale.ToString());
            */

            //DebugMsg("ltMouse comp: " + component.ToString());
            //DebugMsg("ltMouse eventParam: " + eventParam.ToString());
            //DebugMsg("ltMouse eventParam source: " + eventParam.source.ToString());
            //DebugMsg("Comp width: " + component.width);
            //DebugMsg("Comp height: " + component.height);
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

