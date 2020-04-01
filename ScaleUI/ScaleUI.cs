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

                FixEverything();
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }

        private void FixLinesOverview() {
            uint curr_num_lines = Singleton<TransportManager>.instance.m_lines.ItemCount();
            if (curr_num_lines > num_transport_lines) {
                ChangeScale(ModConfig.Instance.scale + 0.0001f);
                ChangeScale(ModConfig.Instance.scale - 0.0001f);
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
                FixFullScreenContainer();
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
                if (CameraIsFullscreen()) {
                    return;
                }
                MakeCameraFullscreen.Initialize();
            } else {
                //scaleui redirected camera
                if (MakeCameraFullscreen.cameraControllerRedirected) {
                    MakeCameraFullscreen.Deinitialize();
                }
            }
        }

        private bool CameraIsFullscreen() {
            if (MakeCameraFullscreen.cameraControllerRedirected) {
                return true;
            }
            CameraController cameraController = GameObject.FindObjectOfType<CameraController>();
            if (cameraController != null) {

                Camera camera = cameraController.GetComponent<Camera>();
                if (camera != null) {

                    if (Mathf.Approximately(camera.rect.width, 1) && Mathf.Approximately(camera.rect.height, 1)) {
                        //already fullscreen
                        return true;
                    }
                }
            }
            return false;
        }

        private void FixFullScreenContainer() {
            //rescale the border around the window (when paused)
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
            //button top left
            infomenu.transformPosition = new Vector2(fullscreenContainer.GetBounds().min.x, fullscreenContainer.GetBounds().max.y);
            infomenu.relativePosition += new Vector3(70.0f, 6.0f);
        }
        private void FixInfoViewsContainer() {
            //container with info buttons
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
            //much too big and can't be repositioned easily, need to reduce the size
            PoliciesPanel policies = ToolsModifierControl.policiesPanel;

            List<int> li = new List<int>();
            li.Add(DistrictPolicies.CITYPLANNING_POLICY_COUNT);
            li.Add(DistrictPolicies.SERVICE_POLICY_COUNT);
            li.Add(DistrictPolicies.SPECIAL_POLICY_COUNT);
            li.Add(DistrictPolicies.TAXATION_POLICY_COUNT);
            li.Sort();
            li.Reverse();
            int maxPolicies = li[0];

            UIButton b = (UIButton)policies.Find("PolicyButton");
            float buttonheight = b.height;
            policies.component.height = maxPolicies * buttonheight + 200f;
        }

        private void FixUnlockingPanel() {
            //UnlockingPanel
            //position at top of screen so it's visible with scaled ui
            UnityEngine.Object obj = GameObject.FindObjectOfType(typeof(UnlockingPanel));
            ReflectionUtils.WritePrivate<UnlockingPanel>(obj, "m_StartPosition", new UnityEngine.Vector3(-1f, 1f));
        }
        
        // MouseEventHandler to hide button when a category panel is clicked
        private void HideCloseButton(UIComponent component, UIMouseEventParameter eventParam) {
            tsCloseButton.position = CLOSEBTN_HIDE_POS;
        }
        
        private void LogAllComponents() {
            var components = UnityEngine.Object.FindObjectsOfType<UIComponent>();
            foreach (var c in components) {
                Debug.Log("Comp: " + c.ToString());
            }
        }

        private void LogComponentChildren(UIComponent component) {
            foreach (var child in component.GetComponentsInChildren<UIComponent>()) {
                Debug.Log("Child of " + component.ToString() + ": " + child.ToString());
            }
        }

        private void LogComponentParents(UIComponent component) {
            foreach (var parent in component.GetComponentsInParent<UIComponent>()) {
                Debug.Log("Parent of " + component.ToString() + ": " + parent.ToString());
            }
        }
        private void DebugMsg(String s) {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, s);
        }

    }
}

