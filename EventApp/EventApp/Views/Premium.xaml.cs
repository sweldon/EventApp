using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using EventApp.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Premium : ContentPage
    {

        public Holiday OpenedHoliday { get; set; }
        public string CommentTitle { get; set; }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
            set
            {
                if (Settings.CurrentUser == value)
                    return;
                Settings.CurrentUser = value;
                OnPropertyChanged();
            }
        }

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
            set
            {
                if (Settings.IsLoggedIn == value)
                    return;
                Settings.IsLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public bool isPremium
        {
            get { return Settings.IsPremium; }
            set
            {
                if (Settings.IsPremium == value)
                    return;
                Settings.IsPremium = value;
                OnPropertyChanged();
            }
        }

        public Premium()
        {
            InitializeComponent();

            BindingContext = this;
        }


        async void MakePurchase(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            PurchaseButton.Text = "Connecting to AppStore...";
            
            if (!isLoggedIn)
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                PurchaseButton.Text = "Purchase";
                this.IsEnabled = true;
            }
            else if (isPremium)
            {
                await DisplayAlert("Thank you!", "You have already purchased premium", "Oh right!");
                PurchaseButton.Text = "Purchase";
                this.IsEnabled = true;
            }
            else
            {
                PurchaseButton.Text = "Connected...";
                var billing = CrossInAppBilling.Current;
                try
                {
                    //var productId = "android.test.purchased";
                    var productId = "holidailypremium";
                    
                    var connected = await billing.ConnectAsync(ItemType.InAppPurchase);

                    //Undo Test Purchase
                    //var consumedItem = await billing.ConsumePurchaseAsync(productId,
                    //"inapp:com.divinity.holidailyapp:android.test.purchased");
                    //if (consumedItem != null)
                    //{
                    //    await DisplayAlert("Premium Refunded", "Premium is undone", "OK");
                    //    isPremium = false;
                    //    this.IsEnabled = true;
                    //    return;
                    //}

                    if (!connected)
                    {
                        await DisplayAlert("Uh oh!", "We couldn't connect to the store", "Try again");
                        //Couldn't connect to billing, could be offline, alert user
                        PurchaseButton.Text = "Purchase";
                        this.IsEnabled = true;
                        return;
                    }

                    //try to purchase item
                    var purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase, "apppayload");
                    if (purchase == null)
                    {
                        await DisplayAlert("Error!", "Something went wrong. You have not been charged for anything.", "Try again");
                        PurchaseButton.Text = "Purchase";
                        this.IsEnabled = true;
                    }
                    else
                    {
                        //Purchased, save this information
                        var id = purchase.Id;
                        var token = purchase.PurchaseToken;
                        var state = purchase.State.ToString();

                        var values = new Dictionary<string, string>{
                           { "username", currentUser },
                           { "id", id },
                           { "token", token },
                           { "state", state }
                        };

                        var content = new FormUrlEncodedContent(values);
                        var response = await App.globalClient.PostAsync(App.HolidailyHost + "/user/", content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                        int status = responseJSON.status;
                        if (status == 200)
                        {
                            isPremium = true;
                            await DisplayAlert("Success!", "The transaction was successful. Thank you very much for your support", "You're welcome!");
                            PurchaseButton.Text = "Purchase";
                            this.IsEnabled = true;
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Something went wrong. If you do not see your premium rewards, please contact holidailyapp@gmail.com and we will resolve it ASAP.", "Try again");
                            PurchaseButton.Text = "Purchase";
                            this.IsEnabled = true;
                        }
                        
                    }

                }
                catch (Exception ex)
                {
                    //Something else has gone wrong, log it
                    PurchaseButton.Text = "Purchase";
                    this.IsEnabled = true;
                    await DisplayAlert("Error!", "Unable to connect to the store, please try again later.", "OK");

                }
                finally
                {
                    PurchaseButton.Text = "Purchase";
                    this.IsEnabled = true;
                    await billing.DisconnectAsync();
                }
            }

        }

        protected override void OnAppearing()
        {

            base.OnAppearing();

        }

    }
}
