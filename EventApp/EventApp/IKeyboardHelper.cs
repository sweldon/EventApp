using System;
using System.Collections.Generic;
using System.Text;
using EventApp;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidKeyboardHelper))]
namespace EventApp
{
    public interface IKeyboardHelper
    {
        void HideKeyboard();
    }
}
