using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace EventApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            Title = "Login or Register";
        }

        public ICommand OpenWebCommand { get; }
    }
}