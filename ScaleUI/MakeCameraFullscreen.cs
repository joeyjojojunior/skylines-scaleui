/*
The MIT License (MIT)

Copyright (c) 2015 Alexander Dzhoganov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using ColossalFramework;
using ColossalFramework.UI;
using System.Reflection;
using UnityEngine;

namespace ScaleUI {
    public class MakeCameraFullscreen {
        private static Rect fullRect = new Rect(0, 0, 1, 1);
        private static RedirectCallsState cameraControllerRedirect;
        private static bool initialized = false;
        public static bool cameraControllerRedirected = false;
        private static bool cachedFreeCamera = false;
        private static CameraController cameraController;
        private static Camera camera;
        private static FieldInfo cachedFreeCameraField = typeof(CameraController).GetField("m_cachedFreeCamera", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Initialize() {
            if (cameraControllerRedirected) {
                return;
            }
            cameraController = GameObject.FindObjectOfType<CameraController>();
            if (cameraController != null) {
                MethodInfo m1 = typeof(CameraController).GetMethod("UpdateFreeCamera", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo m2 = typeof(MakeCameraFullscreen).GetMethod("UpdateFreeCamera", BindingFlags.Instance | BindingFlags.NonPublic);

                cameraControllerRedirect = RedirectionHelper.RedirectCalls(m1, m2);

                cameraControllerRedirected = true;
            }
        }

        public static void Deinitialize() {
            if (cameraControllerRedirected) {
                RedirectionHelper.RevertRedirect(typeof(CameraController).GetMethod("UpdateFreeCamera",
                                                                                    BindingFlags.Instance | BindingFlags.NonPublic), cameraControllerRedirect);
            }
            cameraControllerRedirected = false;
            initialized = false;
            camera = null;
            cachedFreeCamera = false;
        }

        private void UpdateFreeCamera() {
            if (!initialized) {
                camera = cameraController.GetComponent<Camera>();
                cachedFreeCamera = (bool)cachedFreeCameraField.GetValue(cameraController);
                initialized = true;
            }

            // We have either entered or exited the free camera, so we 
            // must remove or restore UI elements accordingly. 
            if (cameraController.m_freeCamera != cachedFreeCamera) {
                cachedFreeCameraField.SetValue(cameraController, cameraController.m_freeCamera);
                cachedFreeCamera = cameraController.m_freeCamera;
                UIView.Show(!cameraController.m_freeCamera);
                Singleton<NotificationManager>.instance.NotificationsVisible = !cameraController.m_freeCamera;
                Singleton<GameAreaManager>.instance.BordersVisible = !cameraController.m_freeCamera;
                Singleton<DistrictManager>.instance.NamesVisible = !cameraController.m_freeCamera;
                Singleton<PropManager>.instance.MarkersVisible = !cameraController.m_freeCamera;
                Singleton<GuideManager>.instance.TutorialDisabled = cameraController.m_freeCamera;
            }

            // Ideally we wouldn't have to do this every tick, but not doing so
            // will break other camera mods, e.g. FPSCamera. It doesn't seem
            // to affect performance in any way.
            camera.rect = fullRect;
        }
    }
}
 