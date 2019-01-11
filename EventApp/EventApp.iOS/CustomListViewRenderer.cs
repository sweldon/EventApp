using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using CustomListViewNamespace;
using CustomListViewDemo.iOS;

[assembly: ExportRenderer(typeof(CustomListView), typeof(CustomListViewRenderer))]
namespace CustomListViewDemo.iOS
{
    public class CustomListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.ShowsVerticalScrollIndicator = false;

            }
        }
    }
}