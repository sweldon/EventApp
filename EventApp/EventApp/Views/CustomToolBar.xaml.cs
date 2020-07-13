using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
#if __IOS__
    using UIKit;
#elif __ANDROID__
    using Android.Content;
#endif
namespace EventApp.Views
{

    public partial class CustomToolBar : ContentView
    {

        public static readonly BindableProperty TitleTextProperty = BindableProperty.Create(
                                                              propertyName: "Title",
                                                              returnType: typeof(string),
                                                              declaringType: typeof(CustomToolBar),
                                                              defaultValue: string.Empty,
                                                              defaultBindingMode: BindingMode.TwoWay,
                                                              propertyChanged: TitleTextPropertyChanged);

        public string Title
        {
            get
            {
                string val = base.GetValue(TitleTextProperty).ToString();
                if (string.IsNullOrEmpty(val))
                {
                    //return Utils.GetDay();
                }
                return val;
            }
            set { base.SetValue(TitleTextProperty, value); }
        }

        private static void TitleTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomToolBar)bindable;
            control.HeaderLabel.Text = newValue.ToString();
        }

        public void UpdateToolbar(bool IsRoot)
        {
            if (IsRoot)
            {
                HeaderImage.IsVisible = true;
                HeaderLabel.IsVisible = false;
                HeaderImage.FadeTo(1, 500);
                HeaderLabel.FadeTo(0, 500);
            }
            else
            {
                HeaderImage.IsVisible = false;
                HeaderLabel.IsVisible = true;
                HeaderImage.FadeTo(0, 500);
                HeaderLabel.FadeTo(1, 500);
            }
        }

        public CustomToolBar()
        {
            InitializeComponent();
            BindingContext = this;
            this.Title = Title;
            MessagingCenter.Subscribe<HolidayDetailPage, bool>(this, "UpdateToolbar", (sender, IsRoot) =>
            {
                UpdateToolbar(IsRoot);
            });
            MessagingCenter.Subscribe<HolidaysPage, bool>(this, "UpdateToolbar", (sender, IsRoot) =>
            {
                UpdateToolbar(IsRoot);
            });

            //MessagingCenter.Subscribe<SearchPage, bool>(this, "UpdateToolbar", (sender, IsRoot) =>
            //{
            //    UpdateToolbar(IsRoot);
            //});

            //MessagingCenter.Subscribe<UserPage, bool>(this, "UpdateToolbar", (sender, IsRoot) =>
            //{
            //    UpdateToolbar(IsRoot);
            //});


            //MessagingCenter.Subscribe<AddHoliday, bool>(this, "UpdateToolbar", (sender, IsRoot) =>
            //{
            //    UpdateToolbar(IsRoot);
            //});


        }

        async void OpenNotifications(object sender, EventArgs e)
        {
            BellBtn.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new NotificationsPage()));
            await Task.Delay(2000);
            BellBtn.IsEnabled = true;
        }
    }
}
