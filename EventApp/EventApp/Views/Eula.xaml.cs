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
using System.Threading.Tasks;
using System.Linq;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Eula : ContentPage
    {
        public NavigationPage NavigationPage { get; private set; }
        public bool eulaAccepted
        {
            get { return Settings.EulaAccepted; }
            set
            {
                if (Settings.EulaAccepted == value)
                    return;
                Settings.EulaAccepted = value;
                OnPropertyChanged();
            }
        }

        public Eula()
        {
            InitializeComponent();

            BindingContext = this;
        }

        async void Agree(object sender, EventArgs e)
        {
            eulaAccepted = true;
            var menuPage = new MenuPage(); // Build hamburger menu
            NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
            var rootPage = new RootPage(); // Root handles master detail navigation
            rootPage.Master = menuPage; // Menu
            rootPage.Detail = NavigationPage; // Content
            Application.Current.MainPage = rootPage; // Set root to built master detail
        }

        protected override void OnAppearing()
        {

            base.OnAppearing();

            string EulaText = @"
End­ User License Agreement ('Agreement')

Last updated: ​4/6/2020

Please read this End­ User License Agreement ('Agreement') carefully before clicking the 'I Agree' button, downloading or using ​Holidaily ('Application'). 

By clicking the 'I Agree' button, downloading or using the Application, you are agreeing to be boundby the terms and conditions of this Agreement. 

If you do not agree to the terms of this Agreement, do not click on the 'I Agree' button and do not download or use the Application. 

License

DVNT Applications grants you a revocable, non­exclusive, non­transferable, limited license to download, install and use the Application solely for your personal, non­commercial purposes strictly in accordance with the terms of this Agreement. 

Restrictions

You agree not to, and you will not permit others to:

a) license, sell, rent, lease, assign, distribute, transmit, host, outsource, disclose or otherwise commercially exploit the Application or make the Application available to any third party.
b) copy or use the Application for any other purposes except for personal, non-commercial purpose
c) modify, decrypt, reverse compile or reverse engineer the Application

Modifications to Application

DVNT Applications reserves the right to modify, suspend or discontinue, temporarily or permanently, the Application or any service to which it connects, with or without notice and without liability to you.

Term and Termination

This Agreement shall remain in effect until terminated by you or ​DVNT Applications. 

DVNT Applications may, in its sole discretion, at any time and for any or no reason, suspend or terminate this Agreement with or without prior notice.

This Agreement will terminate immediately, without prior notice from ​DVNT Applications​, in the event that you fail to comply with any provision of this Agreement. You may also terminate this Agreement by deleting the Application and all copies thereof from your mobile device or from your desktop. 

Upon termination of this Agreement, you shall cease all use of the Application and delete all copies of the Application from your mobile device or from your desktop. 

Severability

If any provision of this Agreement is held to be unenforceable or invalid, such provision will be changed and interpreted to accomplish the objectives of such provision to the greatest extent possible under applicable law and the remaining provisions will continue in full force and effect. 

Amendments to this Agreement

DVNT Applications reserves the right, at its sole discretion, to modify or replace this Agreement at any time. If a revision is material we will provide at least ​30 days' notice prior to any new terms taking effect. What constitutes a material change will be determined at oursole discretion.

Contact Information

If you have any questions about this Agreement, please contact us at holidailyapp@gmail.com";

            EulaLabel.Text = EulaText;

        }

    }
}
