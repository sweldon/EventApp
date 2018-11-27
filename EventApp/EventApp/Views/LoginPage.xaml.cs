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
        HttpClient client = new HttpClient();
        string ec2Instance = "http://ec2-18-205-119-102.compute-1.amazonaws.com:5555";
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
                var response = await client.PostAsync(ec2Instance + "/Login", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.status;

                if (status == 200)
                {
                    Application.Current.Properties["IsLoggedIn"] = Boolean.TrueString;
                    //await RootPage.Detail.Navigation.PushAsync(new ItemsPage());
                    var menuPage = new MenuPage();
                    NavigationPage = new NavigationPage(new ItemsPage());
                    var rootPage = new MainPage(); 
                    rootPage.Master = menuPage; 
                    rootPage.Detail = NavigationPage; 
                    Application.Current.MainPage = rootPage; 
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
                var response = await client.PostAsync(ec2Instance + "/CreateUser", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;

                if (status == 200)
                {
                    Application.Current.Properties["IsLoggedIn"] = Boolean.TrueString;
                    //await RootPage.Detail.Navigation.PushAsync(new ItemsPage());
                    var menuPage = new MenuPage();
                    NavigationPage = new NavigationPage(new ItemsPage());
                    var rootPage = new MainPage();
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
    }
}