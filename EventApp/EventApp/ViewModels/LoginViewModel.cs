using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace EventApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            Title = "Login or register to continue";
        }

        public ICommand OpenWebCommand { get; }
    }
}