//using Xamarin.Forms;
//using Android.Gms.Ads;
//using EventApp;
//using Xamarin.Forms.Platform.Android;
//using Android.Widget;
//using Android.Content;
//using System.Collections.Generic;
//using System;
//using System.Diagnostics;
//[assembly: ExportRenderer(typeof(NativeAdControlView), typeof(NativeAdViewRenderer))]
//namespace EventApp
//{

//    public class NativeAdViewRenderer : ViewRenderer<NativeAdControlView, NativeExpressAdView>
//    {
//        // test ca-app-pub-3940256099942544/6300978111
//        string adUnitId = "ca-app-pub-3940256099942544/2247696110";
//        AdSize adSize = AdSize.MediumRectangle;
//        NativeExpressAdView adView;

//        public NativeAdViewRenderer(Context context) : base(context)
//        {
//        }

//        NativeExpressAdView CreateAdView()
//        {
//            if(adView != null)  
//                return adView;

//            adView = new NativeExpressAdView(Forms.Context);
//            adView.AdSize = adSize;
//            adView.AdUnitId = adUnitId;

           

//            var adParams = new LinearLayout.LayoutParams(
//               LayoutParams.WrapContent, LayoutParams.WrapContent);

//            adView.LayoutParameters = adParams;
//            int heightPixels = AdSize.MediumRectangle.GetHeightInPixels(this.Context);
//            adView.SetMinimumHeight(heightPixels);

//            adView.LoadAd(new AdRequest.Builder().Build());
//            return adView;
//        }

//        protected override void OnElementChanged(ElementChangedEventArgs<NativeAdControlView> e)
//        {
//            base.OnElementChanged(e);

//            if (Control == null)
//            {
//                CreateAdView();
//                SetNativeControl(adView);
//            }
//        }

//    }
//}