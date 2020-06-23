
using System.Diagnostics;
using EventApp;
using EventApp.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorRenderer))]
namespace EventApp.iOS
{
    class CustomEditorRenderer : EditorRenderer
    {
        public CustomEditorRenderer()
        {
        }


        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
        }



    }
}

//[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorRenderer))]
//namespace EventApp.iOS
//{
//    class CustomEditorRenderer : Editor
//    {


//        private void OnTextChanged(object sender, TextChangedEventArgs e)
//        {
//            AutoSize = EditorAutoSizeOption.Disabled;
//        }
//    }
//}