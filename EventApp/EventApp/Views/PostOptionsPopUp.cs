using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;


namespace EventApp.Views
{
    public partial class PostOptionsPopUp : Rg.Plugins.Popup.Pages.PopupPage
    {
        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }
        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }
        public string devicePushId
        {
            get { return Settings.DevicePushId; }
        }
        private dynamic LinkedEntity;
        private ContentView ContainingElement;
        private List<PostOption> options = new List<PostOption>();
        public PostOptionsPopUp(Post post = null, Comment comment = null, ContentView container =null)
        {
            InitializeComponent();

            if(post != null)
            {
                LinkedEntity = post;
            }
            else if(comment != null)
            {
                LinkedEntity = comment;
            }

            ContainingElement = container;

            if (bool.Parse(LinkedEntity.ShowEdit))
                options.Add(new PostOption() { Name = "Edit", Icon = "Submit_Menu_Icon.png" });

            if (bool.Parse(LinkedEntity.ShowDelete))
                options.Add(new PostOption() { Name = "Delete", Icon = "delete.png" });

            if (LinkedEntity.ShowReport)
                options.Add(new PostOption() { Name = "Report", Icon = "Flag.png" });

            if(options.Count == 0)
                options.Add(new PostOption() { Name = "No Options Available", Icon = "alert.png" });
            OptionList.ItemsSource = options;
        }


        protected override async void OnAppearing()
        {

            base.OnAppearing();
            //OptionList.ItemsSource = options;

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }


        public async void OptionSelected(object sender, SelectionChangedEventArgs args)
        {
            //((CollectionView)sender).SelectedItem = null;
            //if (args.CurrentSelection == null)
            //{
            //    return;
            //}
            var option = ((CollectionView)sender).SelectedItem as PostOption;
            try
            {
                await Navigation.PopPopupAsync();

                if (!isLoggedIn)
                {
                    App.promptLogin(Navigation);
                }
                else
                {
                    Debug.WriteLine($"{option.Name}");
                    if (option.Name == "Edit")
                    {
                        //MessagingCenter.Send(this, "EditPost", LinkedEntity);
                        Object[] data = { LinkedEntity, ContainingElement };
                        MessagingCenter.Send(this, "EditPost", data);
                    }
                    else if (option.Name == "Delete")
                    {
                        MessagingCenter.Send(this, "DeletePost", LinkedEntity);
                    }else if (option.Name == "Report")
                    {
                        MessagingCenter.Send(this, "ReportPost", LinkedEntity);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }

        }

    }
}