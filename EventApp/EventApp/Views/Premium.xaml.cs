using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        HttpClient client = new HttpClient();

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

        public Premium()
        {
            InitializeComponent();

            BindingContext = this;
        }


        async void MakePurchase(object sender, EventArgs e)
        {
            // await DisplayAlert("Soon!", "Premium isn't quite ready yet, but is coming soon. We will send you a notification when it is ready.", "I'll come back later!");
            try
            {
                var productId = "holidailypremium";

                var connected = await CrossInAppBilling.Current.ConnectAsync();

                if (!connected)
                {
                    await DisplayAlert("Uh oh!", "We couldn't connect to the store", "Try again");
                    //Couldn't connect to billing, could be offline, alert user
                    return;
                }

                //try to purchase item
                var purchase = await CrossInAppBilling.Current.PurchaseAsync(productId, ItemType.InAppPurchase, "apppayload");
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
                    var state = purchase.State;
                    await DisplayAlert("Success!", "The transaction was successful. Thank you very much for your support", "You're welcome!");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error!", ex.ToString(), "Try again");
            }
            finally
            {
                //Disconnect, it is okay if we never connected
                await CrossInAppBilling.Current.DisconnectAsync();
            }
        }

        protected override async void OnAppearing()
        {

            base.OnAppearing();

        }

    }
}
