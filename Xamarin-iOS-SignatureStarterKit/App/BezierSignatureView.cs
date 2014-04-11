using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace XamariniOSSignatureStarterKit
{
    [Register]
    public class BezierSignatureView : UIView
    {       
        public bool IsSigning { get; set; }
        private PointF _touchLocation;
        private PointF _prevTouchLocation;
        private CGPath _drawPath;
        private UIPanGestureRecognizer _panner = null;

        [Export ("BezierSignatureViewPan")]
        protected void pan(UIPanGestureRecognizer sender)
        {
            if (IsSigning) {
                _touchLocation = sender.LocationInView (this);
                PointF mid = BezierSignatureView.MidPoint (_touchLocation, _prevTouchLocation);

                switch (sender.State) {
                case UIGestureRecognizerState.Began:
                    { 
                        _drawPath.MoveToPoint (_touchLocation);
                        break; 
                    }
                case UIGestureRecognizerState.Changed:
                    {
                        _drawPath.AddQuadCurveToPoint (_prevTouchLocation.X, _prevTouchLocation.Y, mid.X, mid.Y);                   
                        IsSigning = true;
                        break; 
                    }
                default:
                    {
                        break; }

                }
                _prevTouchLocation = _touchLocation;
                SetNeedsDisplay ();
            }
        }

        public void Clear ()
        {
            _drawPath.Dispose ();
            _drawPath = new CGPath ();
            SetNeedsDisplay ();         
        }

        public BezierSignatureView (RectangleF frame, bool isSigning = true) : base(frame)
        {
            IsSigning = isSigning;
            _drawPath = new CGPath ();
            BackgroundColor = UIColor.White;
            MultipleTouchEnabled = false;

            _panner = new UIPanGestureRecognizer (this, new MonoTouch.ObjCRuntime.Selector("BezierSignatureViewPan"));
            _panner.MaximumNumberOfTouches = _panner.MinimumNumberOfTouches = 1;
            AddGestureRecognizer (_panner);
        }

        public UIImage GetDrawingImage()
        {
            UIImage returnImg = null;

            UIGraphics.BeginImageContext (this.Bounds.Size);
            using (CGContext context = UIGraphics.GetCurrentContext()) 
            {
                context.SetStrokeColor (UIColor.Black.CGColor);
                context.SetLineWidth (5f);
                context.AddPath (this._drawPath);
                context.StrokePath ();
                returnImg = UIGraphics.GetImageFromCurrentImageContext ();
            }
            UIGraphics.EndImageContext ();
            return returnImg;
        }

        public override void Draw (RectangleF rect)
        {
            using (CGContext context = UIGraphics.GetCurrentContext()) 
            {
                context.SetStrokeColor (UIColor.Black.CGColor);
                context.SetLineWidth (5f);
                context.AddPath (this._drawPath);
                context.StrokePath ();
            }
        }

        public static PointF MidPoint(PointF p0, PointF p1) {
            return new PointF() {
                X = (float)((p0.X + p1.X) / 2.0),
                Y = (float)((p0.Y + p1.Y) / 2.0)
            };
        }

    }
}

