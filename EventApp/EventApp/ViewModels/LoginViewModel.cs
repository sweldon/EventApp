using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace EventApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            Title = "Get Involved!";
        }
        public ICommand OpenWebCommand { get; }
    }
}