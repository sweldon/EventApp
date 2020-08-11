using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.Generic;
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

        public int notifCount
        {
            get { return Settings.NotificationCount; }
            set
            {
                if (Settings.NotificationCount == value)
                    return;
                Settings.NotificationCount = value;
                OnPropertyChanged();
            }
        }

        public bool isActive
        {
            get { return Settings.IsActive; }
            set
            {
                if (Settings.IsActive == value)
                    return;
                Settings.IsActive = value;
                OnPropertyChanged();
            }
        }

        private static void TitleTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomToolBar)bindable;
            control.HeaderLabel.Text = newValue.ToString();
        }

        public void updateBell()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                
                if (notifCount > 0)
                {

                    BadgeWrapper.FadeTo(1, 100);
                    BellBadge.FadeTo(1, 100);

                    if (notifCount > 99)
                    {

                        BellBadge.Text = "!";
                    }
                    else
                    {
                        BellBadge.Text = notifCount.ToString();
                    }
                }
                else
                {
                    BadgeWrapper.FadeTo(0, 100);
                    BellBadge.FadeTo(0, 100);
                }

                // Just in case some async shenanigans caused us to drop below 0
                if (notifCount < 0)
                    notifCount = 0;
            });

        }

        public async void UpdateToolbar(bool IsRoot)
        {
            if (IsRoot)
            {
                HeaderLabel.IsVisible = false;
                HeaderImage.IsVisible = true;
                await HeaderLabel.FadeTo(0, 500);
                await HeaderImage.FadeTo(1, 500);

            }
            else
            {
                HeaderImage.IsVisible = false;
                HeaderLabel.IsVisible = true;
                await HeaderImage.FadeTo(0, 500);
                await HeaderLabel.FadeTo(1, 500);

            }
            updateBell();
        }

        //private async void RefreshNotifications()
        //{
        //    await Task.Delay(3000);  
        //    if (!App.NotificationsRefreshed)
        //    {
        //        await Utils.GetUserNotificationCount();
        //    }
        //    else
        //    {
        //        await Application.Current.MainPage.DisplayAlert("test", "already refreshed", "test");
        //    }
        //}


        public CustomToolBar()
        {
            InitializeComponent();
            BindingContext = this;
            this.Title = Title;

            // Sync toolbar between pages
            MessagingCenter.Subscribe<Application, bool>(this, "UpdateToolbar", (sender, IsRoot) =>
            {
                UpdateToolbar(IsRoot);
            });

            MessagingCenter.Subscribe<Application, int>(this, "UpdateBellCount", (sender, count) =>
            {
                notifCount = count;
                updateBell();
            });

            //MessagingCenter.Subscribe<Application>(this, "RefreshNotifications", (sender) =>
            //{
            //    // If the app launch hasnt triggered notif refresh, do it again
            //    RefreshNotifications();
            //});

        }


        async void OpenNotifications(object sender, EventArgs e)
        {
            BellBtn.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new NotificationsPage()));
            await Task.Delay(2000);
            BellBtn.IsEnabled = true;

            // Reset bell
            await BadgeWrapper.FadeTo(0, 100);
            await BellBadge.FadeTo(0, 100);
            notifCount = 0;
        }
    }
}
