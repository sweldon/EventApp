using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using EventApp.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Plugin.InAppBilling;
using System.Threading.Tasks;
using System.Linq;
# if __ANDROID__
using Plugin.InAppBilling.Abstractions;
# endif
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
                    var verify = DependencyService.Get<IInAppBillingVerifyPurchase>();
                    var purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase, "apppayload", verify);
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

        public async void RestorePurchases(object sender, EventArgs e)
        {
            RestoreButton.Text = "Processing...";
            var productId = "holidailypremium";
            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync(ItemType.InAppPurchase);

                if (!connected)
                {
                    //Couldn't connect
                }

                //check purchases
                var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);

                //check for null just incase
                if (purchases?.Any(p => p.ProductId == productId) ?? false)
                {
                    await DisplayAlert("Success!", "Your purchases have been " +
                        "successfully restored", "Thanks!");
                }
                else
                {
                    //no purchases found
                    await DisplayAlert("No Restoration Needed", "Your AppleID is " +
                        "synced with your purchases. You're all set!", "OK");
                }
                RestoreButton.Text = "Restore Purchases";
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                //Billing Exception handle this based on the type
                await DisplayAlert("Error", "Could not complete your restoration " +
                    "request at this time, please try again", "OK");
                RestoreButton.Text = "Restore Purchases";
            }
            catch (Exception ex)
            {
                //Something has gone wrong
                RestoreButton.Text = "Restore Purchases";
            }
            finally
            {
                await billing.DisconnectAsync();
                RestoreButton.Text = "Restore Purchases";
            }

        }


        protected override void OnAppearing()
        {

            base.OnAppearing();

        }

    }
}
