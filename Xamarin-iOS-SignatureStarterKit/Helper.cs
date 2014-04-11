using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace XamariniOSSignatureStarterKit
{
    public class Helper
    {
        public Helper ()
        {
        }

        public static int iOSMajorVersion {
            get {
                return Convert.ToInt16 (UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
            }
        }

        public static bool IsIOS7 
        {
            get {
                return iOSMajorVersion > 6;
            }
        }

        public static UIView CreateToolbarTextImageIcon (string label, EventHandler action, UIImage iconImage, UIColor textColor, float width = 60)
        {
            float height = 30;
            float iconSize = 20;
            float iconMargin = 5;

            UILabel lb = new UILabel() {
                Font = UIFont.SystemFontOfSize(14),
                TextColor = textColor,
                Text = label,
                BackgroundColor = UIColor.Clear,
                TextAlignment = UITextAlignment.Left
            };
            lb.SizeToFit();
            int x = (int)(iconSize + (iconMargin * 2));
            int y = (int)((height - lb.Frame.Height) / 2);
            lb.Frame = new RectangleF (x, y, width-y, lb.Frame.Height);

            UIButton btn = new UIButton(UIButtonType.Custom);
            btn.Frame = new RectangleF(0, 0, width, height);
            if (action != null)
                btn.TouchUpInside += action;

            UIImageView iv = new UIImageView(iconImage);
            iv.Frame = new RectangleF(iconMargin, iconMargin, iconSize, iconSize);

            UIView container = new UIView(new RectangleF(0, 0, width, height));
            container.Add(iv);
            container.Add(btn);
            container.Add(lb);
            return container;
        }

        public static RectangleF ScreenBoundsDependOnOrientation (UIInterfaceOrientation? interfaceOrientation = null)
        {  
            RectangleF screenBounds = new RectangleF();

            if (!interfaceOrientation.HasValue)
                interfaceOrientation = UIApplication.SharedApplication.StatusBarOrientation;

            if (interfaceOrientation == UIInterfaceOrientation.Portrait || interfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
            {
                screenBounds.Size = new SizeF (UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            } else if (interfaceOrientation == UIInterfaceOrientation.LandscapeLeft || interfaceOrientation == UIInterfaceOrientation.LandscapeRight){
                screenBounds.Size = new SizeF (UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width);
            }
            return screenBounds ;
        }

        public static bool IsPhone {
            get {
                return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;
            }
        }
    }
}

