using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScaleUI {
    public class ScaleUI : MonoBehaviour {
        bool isInitialized;
        float thumbnailbarY = 0f;
        uint num_transport_lines;
        UIComponent tsBar = GameObject.Find("TSBar").GetComponent<UIComponent>();
        UIComponent tsCloseButton; 

        void Update() {
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
            
            try {
                tsCloseButton = GameObject.Find("TSCloseButton").GetComponent<UIComponent>();

                if (tsCloseButton.isVisible) {
                    tsBar.RemoveUIComponent(tsCloseButton);
                }
            } catch (Exception ex) {}

            Array16<TransportLine> lines = Singleton<TransportManager>.instance.m_lines;
            uint curr_num_lines = lines.ItemCount();
            if (curr_num_lines > num_transport_lines) {
                ChangeScale(ModConfig.Instance.scale + 0.01f);
                ChangeScale(ModConfig.Instance.scale - 0.01f);
            } 
            num_transport_lines = curr_num_lines;
        }

        void Start() {
            try {
                num_transport_lines = Singleton<TransportManager>.instance.m_lines.ItemCount();
                FixEverything();
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }
        
        public void ChangeScale(float scale) {
            UIView.GetAView().scale = scale;
            FixEverything();
        }

        private void SetDefaultScale() {
            UIView.GetAView().scale = 1f;
            FixEverything();
        }

        private void FixEverything() {
            FixCamera();
            FixUIPositions();
        }

        private void FixCamera() {
            if (UIView.GetAView().scale < 1.0f) {
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

        private void FixUIPositions() {
            try {
                FixFullScreenContainer();
                FixInfoMenu();
                FixInfoViewsContainer();
                FixPoliciesPanel();
                FixUnlockingPanel();
                FixDisasterDetection();
                FixLinesOverview();
            } catch (Exception ex) {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "ScaleUI: " + ex.ToString());
            }
        }

        private void FixFullScreenContainer() {
            //rescale the border around the window (when paused)
            UIComponent uic;
            uic = UIView.GetAView().FindUIComponent("ThumbnailBar");
            if (thumbnailbarY == 0f) {
                thumbnailbarY = uic.relativePosition.y;
            }
            float diffHeight = uic.relativePosition.y - thumbnailbarY;
            thumbnailbarY = uic.relativePosition.y;

            uic = UIView.GetAView().FindUIComponent("FullScreenContainer");
            uic.height += diffHeight;
            uic.relativePosition = new Vector2(0, 0);
        }

        private void FixInfoMenu() {
            //button top left
            UIComponent fullscreenContainer = UIView.GetAView().FindUIComponent("FullScreenContainer");

            UIComponent infomenu = UIView.GetAView().FindUIComponent("InfoMenu");
            infomenu.transformPosition = new Vector2(fullscreenContainer.GetBounds().min.x, fullscreenContainer.GetBounds().max.y);
            infomenu.relativePosition += new Vector3(70.0f, 6.0f);
        }
        private void FixInfoViewsContainer() {
            //container with info buttons
            UIComponent infomenu = UIView.GetAView().FindUIComponent("InfoMenu");
            UIComponent infomenucontainer = UIView.GetAView().FindUIComponent("InfoViewsContainer");

            infomenucontainer.pivot = UIPivotPoint.TopCenter;
            infomenucontainer.transformPosition = new Vector3(infomenu.GetBounds().center.x, infomenu.GetBounds().min.y);
            infomenucontainer.relativePosition += new Vector3(-6.0f, 6.0f);
        }

        private void FixDisasterDetection() {
            UIComponent fullscreenContainer = UIView.GetAView().FindUIComponent("FullScreenContainer");

            const float OFFSET_X = 40.0f;
            const float OFFSET_Y = 3.0f;
           
            try {
                UIComponent disasterWarnPanel = UIView.GetAView().FindUIComponent("WarningPhasePanel");
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
        
        private void FixLinesOverview() {
            try {
                // Top Level Panel
                UIComponent LinesOverview = GameObject.Find("(Library) PublicTransportDetailPanel").GetComponent<UIComponent>();
                GameObject MonorailDetail = GameObject.Find("MonorailDetail");




                //LogComponentChildren(LinesOverview);
            } catch (Exception ex) {
                DebugMsg("Can't find: " + ex.ToString());
            }
        }

        private void FixCategoriesCloseButton() {
            tsBar.RemoveUIComponent(tsCloseButton);
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

