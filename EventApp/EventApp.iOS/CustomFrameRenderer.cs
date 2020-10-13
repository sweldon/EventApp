using System;
using System.ComponentModel;
using System.Diagnostics;
using CoreGraphics;
using EventApp;
using EventApp.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomFrame), typeof(CustomFrameRenderer))]
namespace EventApp.iOS
{
    public class CustomFrameRenderer: FrameRenderer
    {
        public CustomFrameRenderer()
        {
        }



        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                base.OnElementPropertyChanged(sender, e);

                // If the Frame's position or size changes we need to reset the shadow
                //if (e.PropertyName == "X" || e.PropertyName == "Y" || e.PropertyName == "Width" || e.PropertyName == "Height" || e.PropertyName == "IsVisible")
                //{
                    SetFrameShadow();
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override void Draw(CGRect rect)
        {
            try
            {
                base.Draw(rect);

                SetFrameShadow();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void SetFrameShadow()
        {
            if (Element != null && Element.HasShadow)
            {
                Layer.MasksToBounds = true;
                Layer.BorderColor = Element.BorderColor.ToCGColor();

                var shadowRadius = 1f;
                Layer.ShadowRadius = shadowRadius;
                Layer.ShadowColor = UIColor.Gray.CGColor;
                Layer.ShadowOffset = new CGSize(0f, 1f);
                Layer.ShadowOpacity = 0.8f;
                Layer.MasksToBounds = false;
                Layer.BorderWidth = 0;
            }
        }

    }
}
