using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EventApp.Models;
using EventApp.ViewModels;
using Plugin.Share;
using Plugin.Share.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
#if __IOS__
using UIKit;
#endif


namespace EventApp.Views
{
    public partial class PastHolidays : StackLayout
    {
        public ObservableCollection<Holiday> Holidays { get; set; }

        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(
                                                      propertyName: "IsVisible",
                                                      returnType: typeof(bool),
                                                      declaringType: typeof(PastHolidays),
                                                      defaultValue: false,
                                                      defaultBindingMode: BindingMode.TwoWay,
                                                      propertyChanged: IsVisiblePropertyChanged);

        public bool IsVisible
        {
            get
            {
                bool val = (bool)base.GetValue(IsVisibleProperty);
                return val;
            }
            set { base.SetValue(IsVisibleProperty, value); }
        }

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
 
        }

        public PastHolidays()
        {
            InitializeComponent();
            BindingContext = this;
            Holidays = new ObservableCollection<Holiday>();
            PastHolidaysList.ItemAppearing += (sender, e) =>
            {
               
                var holiday = e.Item as Holiday;
                if (Holidays.Last() == holiday)
                {
                    LoadMoreHolidays();
                }
              
            };

            // List highlighting
            PastHolidaysList.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                // don't do anything if we just de-selected the row.
                if (e.Item == null) return;

                // Optionally pause a bit to allow the preselect hint.
                Task.Delay(500);

                // Deselect the item.
                if (sender is ListView lv) lv.SelectedItem = null;


            };

            PastHolidaysList.ItemSelected += (object sender, SelectedItemChangedEventArgs args) => {

                ((ListView)sender).SelectedItem = null;
                if (args.SelectedItem == null)
                {
                    return;
                }

            };



            }

        async void LabelToHoliday(object sender, EventArgs args)
        {

            var item = (sender as Label).BindingContext as Holiday;

            if (!item.Active)
            {
                await Application.Current.MainPage.DisplayAlert("Holiday Inactive", "This holiday is currently not ready " +
                        "for viewing. We like to keep a close eye on our holidays " +
                        "and keep them up to date, so sometimes they are removed " +
                        "from the active queue. Try again later!", "OK");
                return;
            }

            string holidayId = item.Id;
            await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
            
        }

        async void ImageToHoliday(object sender, EventArgs args)
        {
            dynamic item;

            try
            {
                item = (sender as ContentView).BindingContext as Holiday;
            }
            catch
            {
                item = (sender as Image).BindingContext as Holiday;
            }

            if (!item.Active)
            {
                await Application.Current.MainPage.DisplayAlert("Holiday Inactive", "This holiday is currently not ready " +
                        "for viewing. We like to keep a close eye on our holidays " +
                        "and keep them up to date, so sometimes they are removed " +
                        "from the active queue. Try again later!", "OK");
                return;
            }

            string holidayId = item.Id;

            await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));

        }

        public void Share(object sender, EventArgs args)
        {
            dynamic holiday;
            try
            {
                holiday = (sender as Label).BindingContext as Holiday;
            }
            catch
            {
                holiday = (sender as Image).BindingContext as Holiday;
            }

            var holidayName = holiday.Name;
            var holidayLink = App.HolidailyHost + "/holiday?id=" + holiday.Id;
            string blurb = $"{holidayName}! {holiday.Blurb}\nCheck it out on Holidaily!";

            if (!CrossShare.IsSupported)
                return;

            CrossShare.Current.Share(new ShareMessage
            {
                Title = holidayName,
                Text = blurb,
                Url = holidayLink
            });
        }

        private static void IsVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (PastHolidays)bindable;
            if(control.Holidays.Count() == 0)
                control.LoadHolidays();

            if ((bool)newValue)
            {
                MessagingCenter.Subscribe<HolidayDetailPage, Object[]>(control,
                "UpdateCelebrateStatus", (sender, data) => {
                    control.UpdateCelebrateStatus((string)data[0], (bool)data[1], (string)data[2]);
                });
            }
            else
            {
                MessagingCenter.Unsubscribe<HolidayDetailPage, Object[]>(control, "UpdateCelebrateStatus");
            }

        }

        private int pastHolidaysPage = 0;
        protected void RefreshPastHolidays(object sender, EventArgs e)
        {
            pastHolidaysPage = 0;
            LoadHolidays();
            PastHolidaysList.EndRefresh();
   
        }

        private void UpdateCelebrateStatus(string holiday, bool upvote, string newVotes)
        {
            foreach (Holiday h in Holidays)
            {

                if (h.Name == holiday)
                {
                    if (upvote)
                    {
                        h.CelebrateStatus = "celebrate_active.png";
                        h.Votes = newVotes;
                    }
                    else
                    {
                        h.CelebrateStatus = "celebrate.png";
                        h.Votes = newVotes;
                    }
                    break;
                }
            }
        }

        private async void LoadHolidays()
        {

            try
            {
                Holidays = await Services.GlobalServices.GetHolidaysAsync(past: true);
                PastHolidaysList.ItemsSource = Holidays;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
        }

        public async void LoadMoreHolidays()
        {
            pastHolidaysPage += 1;
            var moreHolidays = await Services.GlobalServices.GetHolidaysAsync(page: pastHolidaysPage, past: true);
            foreach (Holiday h in moreHolidays)
            {
                Holidays.Add(h);
            }
        }

        async void OnCelebrateTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            #if __IOS__
                var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                haptic.Prepare();
                haptic.ImpactOccurred();
                haptic.Dispose();
            #endif

            #if __ANDROID__
                    var duration = TimeSpan.FromSeconds(.025);
                    Vibration.Vibrate(duration);
            #endif

            var view = (VisualElement)sender;
            var GridObject = (Grid)view.Parent;
            var CelebrateImage = (Image)GridObject.Children[4];
            var UpVoteImageFile = CelebrateImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;

            var holidayId = "none";
            var votesInt = -1;
            var CelebrateLabel = (Label)GridObject.Children[5];
            var holiday = new Holiday();
            try
            {
                holiday = (sender as Label).BindingContext as Holiday;
                votesInt = Int32.Parse((sender as Label).Text.Split(null)[0]);
                holidayId = holiday.Id;
            }
            catch
            {
                holiday = (sender as Image).BindingContext as Holiday;
                votesInt = Int32.Parse(CelebrateLabel.Text.Split(null)[0]);
                holidayId = holiday.Id;
            }

            var newVotes = votesInt;

            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                App.promptLogin(Navigation);
                this.IsEnabled = true;
            }
            else
            {
                if (UpVoteIcon == "celebrate_active.png")
                {
                    // Undo
                    newVotes -= 1;
                    holiday.CelebrateStatus = "celebrate.png";
                    holiday.Votes = newVotes.ToString();
                    await CelebrateImage.ScaleTo(2, 50);
                    await CelebrateImage.ScaleTo(1, 50);
                    await Services.GlobalServices.VoteHoliday(holidayId, "3");

                }
                else
                {
                    // Only allow if user hasnt already downvoted
                    newVotes += 1;
                    if (newVotes <= votesInt + 1 && newVotes >= votesInt - 1)
                    {
                        holiday.CelebrateStatus = "celebrate_active.png";
                        holiday.Votes = newVotes.ToString();
                        await CelebrateImage.ScaleTo(2, 50);
                        await CelebrateImage.ScaleTo(1, 50);
                        await Services.GlobalServices.VoteHoliday(holidayId, "1");

                    }
                    else
                    {
                        newVotes -= 2;
                        holiday.CelebrateStatus = "celebrate.png";
                        holiday.Votes = newVotes.ToString();
                        await CelebrateImage.ScaleTo(2, 50);
                        await CelebrateImage.ScaleTo(1, 50);
                        await Services.GlobalServices.VoteHoliday(holidayId, "5");

                    }

                }
            }

            this.IsEnabled = true;

        }

    }
}
