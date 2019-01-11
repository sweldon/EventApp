using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {

        public string isLoggedIn
        {
            get { return Settings.GeneralSettings; }
            set
            {
                if (Settings.GeneralSettings == value)
                    return;
                Settings.GeneralSettings = value;
                OnPropertyChanged();
            }
        }

        public string currentUser
        {
            get { return Settings.GeneralSettings; }
            set
            {
                if (Settings.GeneralSettings == value)
                    return;
                Settings.GeneralSettings = value;
                OnPropertyChanged();
            }
        }

        HttpClient client = new HttpClient();
        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";
        public NavigationPage NavigationPage { get; private set; }
        public LoginPage()
        {
            InitializeComponent();
        }

        public async Task LoginUser(object sender, EventArgs e)
        {
            string userName = NameEntry.Text.Trim();
            string pass = PassEntry.Text;

            if (!string.IsNullOrEmpty(userName))
            {

                var values = new Dictionary<string, string>{
                    { "username", userName },
                    { "password", pass }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ec2Instance + "/portal/login/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;

                if (status == 200)
                {
                    isLoggedIn = "yes";
                    //await RootPage.Detail.Navigation.PushAsync(new ItemsPage());
                    var menuPage = new MenuPage();
                    NavigationPage = new NavigationPage(new HolidaysPage());
                    var rootPage = new RootPage(); 
                    rootPage.Master = menuPage; 
                    rootPage.Detail = NavigationPage;
                    currentUser = userName;
                    await Navigation.PopModalAsync();
                    //Application.Current.MainPage = rootPage; 
                }
                else if (status == 400)
                {
                    await DisplayAlert("Error", "Your info doesn't look right...", "Try again");
                }
                else
                {
                    await DisplayAlert("Error", "Username or password incorrect", "Oops");
                }
            }
            else
            {
                await DisplayAlert("Error", "Enter some text first!", "Alrighty");
            }

        }
        public async Task RegisterUser(object sender, EventArgs e)
        {
            string userName = NameEntry.Text.Trim();
            string pass = PassEntry.Text;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pass) || userName.Contains(" "))
            {
                await DisplayAlert("Error!", "Username or password invalid", "Dang");
            }
            else
            {
                var values = new Dictionary<string, string>{
                   { "username", userName },
                   { "password", pass }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ec2Instance + "/portal/register/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;

                if (status == 200)
                {
                    isLoggedIn = "yes";
                    //await RootPage.Detail.Navigation.PushAsync(new ItemsPage());
                    var menuPage = new MenuPage();
                    NavigationPage = new NavigationPage(new HolidaysPage());
                    var rootPage = new RootPage();
                    rootPage.Master = menuPage;
                    rootPage.Detail = NavigationPage;
                    Application.Current.MainPage = rootPage;
                }
                else if (status == 1000)
                {
                    await DisplayAlert("Error!", "Username already exists.", "Dang");
                }
                else
                {
                    await DisplayAlert("Sorry!", "Something went wrong on our end", "Alrighty");
                }
            }

        }
        async void CancelLogin(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
    }
}