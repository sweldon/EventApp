
//using System.Diagnostics;
//using EventApp;
//using EventApp.iOS;
//using Foundation;
//using UIKit;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.iOS;

//[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorRenderer))]
//namespace EventApp.iOS
//{
//    class CustomEditorRenderer : EditorRenderer
//    {
//        public CustomEditorRenderer()
//        {
//        }

//        private string Placeholder { get; set; }

//        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
//        {
//            base.OnElementChanged(e);
//            var element = this.Element as CustomEditor;

//            if (Control != null && element != null)
//            {
//                Placeholder = element.Placeholder;
//                Control.TextColor = UIColor.LightGray;
//                Control.Text = Placeholder;

     
//                Control.ShouldChangeText += (UITextView textView, NSRange range, string text) => {
//                    Debug.WriteLine($"current value: {textView.Text}");
//                    Debug.WriteLine($"change to: {text}");
//                    if (textView.Text == Placeholder && text == "")
//                    {
//                        return true;
//                    }
//                    else if (textView.Text == Placeholder)
//                    {
//                        textView.TextColor = UIColor.Black;
//                        textView.Text = "";
//                        return true;
//                        //textView.TextColor = UIColor.LightGray;
//                        //Control.SelectedTextRange = Control.GetTextRange(
//                        //fromPosition: Control.BeginningOfDocument,
//                        //toPosition: Control.BeginningOfDocument);

//                    }
//                    else if (textView.Text.Length == 1 && text == "")
//                    {
//                        textView.TextColor = UIColor.LightGray;
//                        textView.Text = Placeholder;
//                        Control.SelectedTextRange = Control.GetTextRange(fromPosition: Control.BeginningOfDocument, toPosition: Control.BeginningOfDocument);


//                    }
//                    else
//                    {
//                        return true;
//                    }

//                    return false;

//                };



//                Control.ShouldBeginEditing += (UITextView textView) =>
//                {

//                    Debug.WriteLine($"{textView.Text.Length}");


//                    if (textView.Text == Placeholder)
//                    {
//                        Debug.WriteLine($"setting to beginning");
                        
//                        Control.SelectedTextRange = Control.GetTextRange(fromPosition: Control.BeginningOfDocument, toPosition: Control.BeginningOfDocument);

//                    }





//                    return true;
//                };

                

//                // todo find out how to set placeholder if editing and lengh is 0 (nothing has been typed yet)

//                Control.ShouldEndEditing += (UITextView textView) =>
//                {
//                    if (textView.Text == "")
//                    {
//                        textView.Text = Placeholder;
//                        textView.TextColor = UIColor.LightGray; // Placeholder Color
//                    }

//                    return true;
//                };
//            }
//        }

//    }
//}
