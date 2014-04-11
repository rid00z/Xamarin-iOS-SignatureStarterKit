using System;
using MonoTouch.UIKit;
using MonoTouch.QuickLook;
using System.IO;
using iTextSharp.text.pdf;

namespace XamariniOSSignatureStarterKit
{
    public class SignViewController : UIViewController 
    {
        public SignViewController ()
        {
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            var signButton = new UIBarButtonItem ("Sign PDF", UIBarButtonItemStyle.Plain, (o,e) => ShowPDFForSigning ());
            NavigationItem.RightBarButtonItem = signButton;
        }

        public async void ShowPDFForSigning()
        {
            var currentFilePath = "Salesinvoice.pdf";

            QLPreviewItemBundle prevItem = new QLPreviewItemBundle ("Salesinvoice.pdf", currentFilePath);
            QLPreviewController previewController = new QLPreviewController ();
            previewController.DataSource = new PreviewControllerDS (prevItem);

            NavigationController.PushViewController (previewController, true);

            //this adds a button to the QLPreviewController, but it has to wait until after it's been loaded 
            //I'm not sure if there's a better way to do this.
            await System.Threading.Tasks.Task.Run( () =>
                {
                    System.Threading.Thread.Sleep( 500 );
                    for (int i = 0; i < 10; i++)
                    {
                        System.Threading.Thread.Sleep( 500 );
                        InvokeOnMainThread( () =>
                            {
                                if (previewController.NavigationItem.RightBarButtonItems.Length == 1)
                                {
                                    var signButton = new UIBarButtonItem( UIBarButtonSystemItem.Compose, (o, e ) =>
                                        {
                                            SignPDF();
                                        } );

                                    previewController.NavigationItem.RightBarButtonItems = 
                                        new UIBarButtonItem[] { signButton, previewController.NavigationItem.RightBarButtonItems[0] };                              
                                }
                            } );
                    }
                } );
        }

        void SignPDF()
        {
            var signViewController = new SignatureViewController("Michael Ridland");
            signViewController.SignatureCompleted += SignatureWasCompleted;

            NavigationController.PopViewControllerAnimated( false );
            NavigationController.PushViewController( signViewController, true );
        }

        void SignatureWasCompleted(byte[] signatureImage, string name)
        {
            NavigationController.PopViewControllerAnimated( false );

            var dirPath = Path.Combine (System.Environment.GetFolderPath (Environment.SpecialFolder.Personal), "SignedPDF");

            if (!Directory.Exists (dirPath))
                Directory.CreateDirectory (dirPath);

            string fileName = string.Format("result-{0}.pdf", Guid.NewGuid().ToString());
            var signedFilePath = Path.Combine (dirPath, fileName);

            if (File.Exists(signedFilePath))
                File.Delete(signedFilePath);

            using (Stream inputPdfStream = new FileStream("Salesinvoice.pdf", FileMode.Open, FileAccess.Read, FileShare.Read))              
            using (Stream outputPdfStream = new FileStream(signedFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                var pdfContentByte = stamper.GetOverContent(1);

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(signatureImage);
                image.SetAbsolutePosition(90, 90);
                var width = 200F;
                image.ScaleToFit(width, (float)(width * (200.0 / 480.0)));
                pdfContentByte.AddImage(image);
                stamper.Close();
            }

            //Display signature after saved
            QLPreviewItemFileSystem prevItem = new QLPreviewItemFileSystem (fileName, signedFilePath);
            QLPreviewController previewController = new QLPreviewController ();
            previewController.DataSource = new PreviewControllerDS (prevItem);
            NavigationController.SetNavigationBarHidden( false, true );
            NavigationController.PushViewController (previewController, true);
        }
    }
}

