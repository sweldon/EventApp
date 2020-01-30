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

            if (!isLoggedIn)
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else if (isPremium)
            {
                await DisplayAlert("Thank you!", "You have already purchased premium", "Oh right!");
                PurchaseButton.Text = "Purchase";
                this.IsEnabled = true;
            }
            else
            {
                PurchaseButton.Text = "Loading...";
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

                    PurchaseButton.Text = "Purchase";
                    this.IsEnabled = true;

                    //try to purchase item
                    var purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase, "apppayload");
                    if (purchase == null)
                    {
                        await DisplayAlert("Error!", "Something went wrong. You have not been charged for anything.", "Try again");
                        //Not purchased, alert the user
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
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Something went wrong. If you do not see your premium rewards, please contact holidailyapp@gmail.com and we will resolve it ASAP.", "Try again");
                        }
                        
                    }

                }
                catch (InAppBillingPurchaseException purchaseEx)
                {
                    var message = string.Empty;
                    switch (purchaseEx.PurchaseError)
                    {
                        case PurchaseError.AppStoreUnavailable:
                            message = "Currently the app store seems to be unavailble. Try again later.";
                            break;
                        case PurchaseError.BillingUnavailable:
                            message = "Billing seems to be unavailable, please try again later.";
                            break;
                        case PurchaseError.PaymentInvalid:
                            message = "Payment seems to be invalid, please try again.";
                            break;
                        case PurchaseError.PaymentNotAllowed:
                            message = "Payment does not seem to be enabled/allowed, please try again.";
                            break;
                    }

                    //Decide if it is an error we care about
                    if (string.IsNullOrWhiteSpace(message))
                        return;
                                PurchaseButton.Text = "Purchase";
            this.IsEnabled = true;
                    //Display message to user
                }
                catch (Exception ex)
                {
                    //Something else has gone wrong, log it
                    PurchaseButton.Text = "Purchase";
                    this.IsEnabled = true;
                    Debug.WriteLine("Issue connecting: " + ex);
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
