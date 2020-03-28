using System;
using UnityEngine;
using ColossalFramework.UI;

namespace ScaleUI
{
    public class ScaleUIPanel : UIPanel, IScaleUI
    {
        public void Destroy ()
        {
            this.enabled = false;
        }
        public ScaleUIPanel ()
        {
            InitPanel ();
        }

        private void InitPanel ()
        {
            this.backgroundSprite = "";
            this.width = 300;
            this.height = 300;
            
            this.autoLayoutDirection = LayoutDirection.Vertical;
            this.autoLayoutStart = LayoutStart.TopLeft;
            this.autoLayoutPadding = new RectOffset (0, 0, 0, 0);
            this.autoLayout = true;
        }

        public void FixUI ()
        {
            //make scaling panel as big as it needs to be
            this.FitChildrenHorizontally ();
            this.FitChildrenVertically ();
            
            //position the panel below the menu button top right
            UIComponent uic = UIView.GetAView ().FindUIComponent ("Esc");
            float newX = uic.relativePosition.x + uic.width / 2 - this.width / 2;
            float newY = uic.relativePosition.y + uic.height + 10;
            this.relativePosition = new Vector3 (newX, newY);
        }
    }
}
