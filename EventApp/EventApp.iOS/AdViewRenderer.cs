using CoreGraphics;
using Google.MobileAds;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using EventApp;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

[assembly: ExportRenderer(typeof(AdControlView), typeof(AdViewRenderer))]
namespace EventApp
{
    public class AdViewRenderer : ViewRenderer<AdControlView, BannerView>
    {
		protected override void OnElementChanged(ElementChangedEventArgs<AdControlView> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
			{
                try {
					SetNativeControl(CreateBannerView());
                }
                catch{
					Debug.WriteLine("Ad not ready yet");
				}			
			}
		}
		
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == nameof(BannerView.AdUnitID))
				Control.AdUnitID = Element.AdUnitId;
		}

		private BannerView CreateBannerView()
		{

			var bannerView = new BannerView(size: AdSizeCons.SmartBannerPortrait,
                                           origin: new CGPoint(0, UIScreen.MainScreen.Bounds.Size.Height - AdSizeCons.Banner.Size.Height))
            {
                AdUnitID = Element.AdUnitId,
                RootViewController = GetVisibleViewController()
            };

			bannerView.LoadRequest(GetRequest());

			Request GetRequest()
			{
				var request = Request.GetDefaultRequest();
				return request;
			}
			
			return bannerView;
		}
			
		private UIViewController GetVisibleViewController()
		{
			var windows = UIApplication.SharedApplication.Windows;
			foreach (var window in windows)
			{
				if (window.RootViewController != null)
				{
					return window.RootViewController;
				}				
			}
			return null;
		}
	}
}