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
            Utils.BuildNavigation();
        }

        protected override void OnAppearing()
        {

            base.OnAppearing();
            AgreeButton.IsVisible = !eulaAccepted;
            string EulaText = @"
Holidaily End­ User License Agreement ('Agreement')

Last updated: ​4/17/20

This Holidaily End User License Agreement (the 'Agreement') is made between Crater Applications ('Crater', 'Company', 'we', 'us', 'our') and you, our customer ('you', 'your') only. This Agreement governs your use of Holidaily and our online social media services ('Services').

1. Acceptance: Please read this End­ User License Agreement ('Agreement') carefully before clicking the 'I Agree' button, downloading or using ​Holidaily ('Application'). By clicking the 'I Agree' button, downloading or using the Application, you are agreeing to be bound by the terms and conditions of this Agreement. If you do not agree to the terms of this Agreement, do not click on the 'I Agree' button and do not download or use the Application.

2. Scope of License: Crater Applications grants you a revocable, non­exclusive, non­transferable, limited license to download, install and use the Application solely for your personal, non­commercial purposes strictly in accordance with the terms of this Agreement. 

3. Restrictions on Use: You agree not to, and you will not permit others to: (a) license, sell, rent, lease, assign, distribute, transmit, host, outsource, disclose or otherwise commercially exploit the Application or make the Application available to any third party; (b) copy or use the Application for any other purposes except for personal, non-commercial purpose; (c) modify, decrypt, reverse compile or reverse engineer the Application; (d) violate any applicable laws, rules or regulations in connection with Your access or use of the Application; (e) use the Application for creating a product, service or software that is, directly or indirectly, competitivce with or in any way a substitute for any services, product or software offered by Company 

4. Modifications to Application: Crater Applications reserves the right to modify, suspend or discontinue, temporarily or permanently, the Application or any service to which it connects, with or without notice and without liability to you.

5. Term and Termination: This Agreement shall remain in effect until terminated by you or ​Crater Applications. Crater Applications may, in its sole discretion, at any time and for any or no reason, suspend or terminate this Agreement with or without prior notice. This Agreement will terminate immediately, without prior notice from ​Crater Applications​, in the event that you fail to comply with any provision of this Agreement. You may also terminate this Agreement by deleting the Application and all copies thereof from your mobile device or from your desktop. Upon termination of this Agreement, you shall cease all use of the Application and delete all copies of the Application from your mobile device or from your desktop. 

6. Severability: If any provision of this Agreement is held to be unenforceable or invalid, such provision will be changed and interpreted to accomplish the objectives of such provision to the greatest extent possible under applicable law and the remaining provisions will continue in full force and effect. 

7. Code of Conduct: In using our services, you may not: Use an offensive screen name, post offensive content, impersonate any person or organization, harass or stalk any persons, collect personal information on others, engage in unlawful activity, access anothers account without permission, harm or exploit minors, cause or encourage others to do any of the above. Failure to comply with the Code of Conduct will result in immediate termination of this Agreement.

8. Limitation of Liability: OUR COMPANY OR ITS AFFILIATES WILL UNDER NO CIRCUMSTANCES BE RESPONSIBLE OR LIABLE FOR ANY INDIRECT, INCIDENTAL, CONSEQUENTIAL, EXEMPLARY DAMAGES OR THE LIKE ARRISING OUT OF OR IN CONNECTION WITH YOUR ACCESS OR USE OF OR INABILITY TO ACCESS OR USE THE APPLICATION. WE ARE NOT RESPONSIBLE OR LIABLE FOR ANY USER GENERATED CONTENT. ALTHOUGH WE PROVIDE A GUIDELINE TO WHAT CAN BE POSTED ABOVE, WE DO NOT CONTROL AND ARE NOT RESPONSIBLE FOR WHAT USERS OF THE APPLICATION POST ON OUR APPLICATION AND ARE NOT RESPONSIBLE FOR ANY OFFENSIVE, INAPPROPRIATE, OBSCENE, UNLAWFUL OR ILLEGAL USER GENERATED CONTENT YOU MIGHT ENCOUNTER ON OUR APPLICATION. WITHOUT LIMITING THE GENERATLITY OF THE FOREGOING, COMPANY'S AGGREGATE LIABILITY TO YOU SHALL NOT EXCEED THE AMOUNT OF FIFTY DOLLARS ($50.00). THE FOREGOING LIMITATIONS WILL APPLY EVEN IF THE ABOVE STATED REMEDY FAILS OF ITS ESSENTIAL PURPOSE.

9. Amendments to this Agreement: Crater Applications reserves the right, at its sole discretion, to modify or replace this Agreement at any time. If a revision is material we will provide at least ​30 days' notice prior to any new terms taking effect. What constitutes a material change will be determined at oursole discretion.

10. Contact Information: If you have any questions about this Agreement, please contact us at: holidailyapp@gmail.com

Crater Applications
PO Box 217
Berlin, CT 06037
";

            EulaLabel.Text = EulaText;

        }

    }
}
