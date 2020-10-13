using Android.Content;
using EventApp;
using EventApp.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorRenderer))]
namespace EventApp.Droid
{
    class CustomEditorRenderer : EditorRenderer
    {
        public CustomEditorRenderer(Context context) : base(context)
        {
        }


        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
            // Remove underline in text box
            Control.SetBackgroundColor(Android.Graphics.Color.Transparent);
            // Stop scaling after a certain point
            Control.SetMaxHeight(250);
            
        }

    }
}