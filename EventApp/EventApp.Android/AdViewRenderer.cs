﻿using Xamarin.Forms;
using Android.Gms.Ads;
using EventApp;
using Xamarin.Forms.Platform.Android;
using Android.Widget;

[assembly: ExportRenderer(typeof(AdControlView), typeof(AdViewRenderer))]
namespace EventApp
{

    public class AdViewRenderer : ViewRenderer<AdControlView, AdView>
    {
        //string adUnitId = "ca-app-pub-1507507245083019/3929247084";
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
        AdSize adSize = AdSize.SmartBanner;
        AdView adView;


        AdView CreateAdView()
        {
            if(adView != null)  
                return adView;
            

            adView = new AdView(Forms.Context);

            adView.AdSize = adSize;
            adView.AdUnitId = adUnitId;
            var adParams = new LinearLayout.LayoutParams(
                LayoutParams.WrapContent, LayoutParams.WrapContent);

            adView.LayoutParameters = adParams;
            adView.LoadAd(new AdRequest.Builder().AddTestDevice("8284655B05EAAE250CC1222024A7BF05").Build());

            return adView;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AdControlView> e)
        {
            base.OnElementChanged(e);

            if(Control == null)
            {
                CreateAdView();
                SetNativeControl(adView);
            }
        }

    }
}