using Xamarin.Forms;
using Android.Gms.Ads;
using EventApp;
using Xamarin.Forms.Platform.Android;
using Android.Widget;
using Android.Content;

[assembly: ExportRenderer(typeof(AdControlView), typeof(AdViewRenderer))]
namespace EventApp
{

    public class AdViewRenderer : ViewRenderer<AdControlView, AdView>
    {
        string adUnitId = "ca-app-pub-1517355594758692/4921551474";
        //string adUnitId = "ca-app-pub-3940256099942544/6300978111";
        AdSize adSize = AdSize.SmartBanner;
        AdView adView;


        public AdViewRenderer(Context context) : base(context)
        {
        }

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
    
            adView.LoadAd(new AdRequest.Builder().Build());
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