using System;
using System.Collections.Generic;
using System.Text;
using EventApp;


#if __ANDROID__
[assembly: Xamarin.Forms.Dependency(typeof(AndroidKeyboardHelper))]
namespace EventApp
{
    public interface IKeyboardHelper
    {
        void HideKeyboard();
        void ShowKeyboard();
    }
}
#endif

