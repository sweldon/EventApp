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

        public async void updateBell()
        {

            if (notifCount > 0)
            {
                if(notifCount > 99)
                {
 
                    BellBadge.Text = "!";
                }
                else
                {
                    BellBadge.Text = notifCount.ToString();
                }
                BadgeWrapper.IsVisible = true;
            }
            else
            {
                BadgeWrapper.IsVisible = false;
            }

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
            updateBell();
        }

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

        }


        async void OpenNotifications(object sender, EventArgs e)
        {
            BellBtn.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new NotificationsPage()));
            await Task.Delay(2000);
            BellBtn.IsEnabled = true;

            // Reset bell
            BadgeWrapper.IsVisible = false;
            notifCount = 0;
        }
    }
}
