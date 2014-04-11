using System;
using MonoTouch.UIKit;
using System.Runtime.InteropServices;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace XamariniOSSignatureStarterKit
{
	public class SignatureViewController : UIViewController
	{
		readonly BezierSignatureView _sigView 
			= new BezierSignatureView(new RectangleF(0,0,1,1), true);
		readonly UIToolbar _toolBar = new UIToolbar();
		readonly UILabel _nameDateLabel = new UILabel();
		string _name;
		public Action<byte[], string> SignatureCompleted;

		readonly UIImage TextImage = UIImage.FromBundle ("Assets/Images/text.png");
		readonly UIImage EraserImage = UIImage.FromBundle ("Assets/Images/erase.png");
		readonly UIImage DoneImage = UIImage.FromBundle ("Assets/Images/checkmark.png");

		public SignatureViewController(string name)
		{
			_name = name;

			if (Helper.IsIOS7)
			{
				AutomaticallyAdjustsScrollViewInsets = false;
				EdgesForExtendedLayout = UIRectEdge.None;
			}
		}

		public override void ViewDidLoad()
		{
			View.BackgroundColor = UIColor.White;

			_sigView.Layer.BorderColor = UIColor.Black.CGColor;
			_sigView.Layer.BorderWidth = 1;
			Add( _sigView );

			_nameDateLabel.TextAlignment = UITextAlignment.Center;
			Add( _nameDateLabel );
			SetNameDate();

			Add( _toolBar );
			var toolBarButtons = new UIBarButtonItem[] {
				new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
				new UIBarButtonItem (Helper.CreateToolbarTextImageIcon( "Confirm", ConfirmClickedHandler, this.DoneImage, UIColor.Black, 100)),
				new UIBarButtonItem (Helper.CreateToolbarTextImageIcon( "Change Name", ChangeNameHandler, this.TextImage, UIColor.Black, 130)),
				new UIBarButtonItem (Helper.CreateToolbarTextImageIcon( "Clear", ClearHandler, this.EraserImage, UIColor.Black, 100)),
				new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace)
			};
			_toolBar.SetItems (toolBarButtons, true);

			base.ViewDidLoad();
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
			UIView.BeginAnimations( "Position" );
			UIView.SetAnimationDuration(duration);
			LayoutViews(Helper.ScreenBoundsDependOnOrientation(toInterfaceOrientation));
			UIView.CommitAnimations();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			NavigationController.SetNavigationBarHidden( true, true );
			LayoutViews(Helper.ScreenBoundsDependOnOrientation());
		}

		void LayoutViews(RectangleF bounds)
		{
			if (Helper.IsPhone)
			{
				float heightIphone = bounds.Height - 64;
				float widthIPhone = (float)(heightIphone * (200.0 / 480.0));

				int x = (int)((bounds.Width - widthIPhone) / 2);
				int y = (int)((bounds.Height - heightIphone) / 2);

				_sigView.Frame = new RectangleF( x, y, widthIPhone, heightIphone );

				_nameDateLabel.Layer.AnchorPoint = new PointF( 0, 0 );
				_nameDateLabel.Frame = new RectangleF( x-30, y+heightIphone, heightIphone, 30 );
				_nameDateLabel.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeRotation((float) -(Math.PI / 2));

				_toolBar.Layer.AnchorPoint = new PointF( 0, 0 );
				_toolBar.Frame = new RectangleF( bounds.Width-30, bounds.Height, bounds.Height, 30 );
				_toolBar.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeRotation((float) -(Math.PI / 2));
			}
			else
			{
				const float width = 480;
				const float height = 200;

				int x = (int)((bounds.Width - width) / 2);
				int y = (int)((bounds.Height - height) / 2);

				_sigView.Frame = new RectangleF( x, y, width, height );

				_nameDateLabel.Frame = new RectangleF( x, y - 30, width, 30 );

				_toolBar.Frame = new RectangleF( x, y + height, width, 30 );
			}
		}

		void SetNameDate()
		{
			_nameDateLabel.Text = string.Format("Signed by {0} on {1:d} ", _name, DateTime.Now);
		}

		void ClearHandler (object sender, EventArgs e)
		{
			_sigView.Clear();
		}

		void ChangeNameHandler (object sender, EventArgs e)
		{
			var alert = new UIAlertView ();
			alert.Canceled += (object s, EventArgs e2) => {}; // we crash if we don't assign Cancel delegate!
			alert.Title = "Please enter name.";

			alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
			alert.Clicked += (object s, UIButtonEventArgs e2) => {
				if (alert.GetTextField(0).Text != _name)
				{
					_name = alert.GetTextField(0).Text;
					SetNameDate();
				}
			};
			alert.AddButton("Ok");
			alert.Show ();
		}

		void ConfirmClickedHandler (object sender, EventArgs e)
		{
			var image = _sigView.GetDrawingImage();

			if (Helper.IsPhone)
				image = ImageRotatedByDegrees( image, 90F );

			var data = image.AsPNG();

			byte[] bytes = new byte[data.Length];
			Marshal.Copy (data.Bytes, bytes, 0, (int)data.Length);

			if (SignatureCompleted != null)
				SignatureCompleted( bytes, _name );
		}

		static float radians (float degrees) 
		{
			return (float)(degrees * Math.PI/180);
		}

		UIImage ImageRotatedByDegrees(UIImage oldImage, float degrees)
		{
			// calculate the size of the rotated view's containing box for our drawing space
			UIView rotatedViewBox = new UIView( new RectangleF( 0, 0, oldImage.Size.Width, oldImage.Size.Height ) );

			CGAffineTransform t = CGAffineTransform.MakeRotation((float)(degrees * Math.PI / 180));
			rotatedViewBox.Transform = t;
			SizeF rotatedSize = rotatedViewBox.Frame.Size;

			// Create the bitmap context
			UIGraphics.BeginImageContext(rotatedSize);
			CGContext bitmap = UIGraphics.GetCurrentContext();

			// Move the origin to the middle of the image so we will rotate and scale around the center.
			bitmap.TranslateCTM(rotatedSize.Width/2, rotatedSize.Height/2);

			//   // Rotate the image context
			bitmap.RotateCTM((float)(degrees * Math.PI / 180));

			// Now, draw the rotated/scaled image into the context
			bitmap.ScaleCTM( 1.0F, -1.0F );
			bitmap.DrawImage(new RectangleF(-oldImage.Size.Width / 2, -oldImage.Size.Height / 2, oldImage.Size.Width, oldImage.Size.Height), oldImage.CGImage);

			UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return newImage;
		}

	}
}

