using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using EventApp.Models;
using EventApp.ViewModels;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace EventApp.Views
{
    public partial class UserPage : ContentPage
    {

        private User SelectedUser;
        List<Holiday> UserHolidays;
        List<Comment> UserComments;
        private string SubmittedUserName;
        public bool TapLock;
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
        public UserPage(User user=null, string userName=null)
        {
            InitializeComponent();
            SubmittedUserName = userName;
            if (user != null)
            {
                SelectedUser = user;
                // No need for slow load
                Title = $"{SelectedUser.UserName}'s Profile";
                ProfilePicture.Source = SelectedUser.Avatar == null ? "default_user_256.png" : SelectedUser.Avatar;
                UserName.Text = SelectedUser.UserName;
                Confetti.Text = SelectedUser.Confetti;
                Comments.Text = SelectedUser.Comments;
                Holidays.Text = SelectedUser.Approved;
                LastOnline.Text = SelectedUser.LastOnline;
            }
            TapLock = false;
            UserCommentList.ItemSelected += OnCommentSelected;
            UserHolidaysList.ItemSelected += OnHolidaySelected;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if(SelectedUser == null)
            {
                var values = new Dictionary<string, string>{
                    { "username", SubmittedUserName },
                    { "requesting_user",  currentUser }
                };
                dynamic response = await ApiHelpers.MakePostRequest(values, "user");
                dynamic results = response.results;
                SelectedUser = new User()
                {
                    UserName = results.username,
                    Confetti = results.confetti,
                    Submissions = results.holiday_submissions,
                    Approved = results.approved_holidays,
                    Comments = results.num_comments,
                    Premium = results.premium,
                    Avatar = results.profile_image,
                    LastOnline = results.last_online
                };
                // Little slower because of api call
                Title = $"{SelectedUser.UserName}'s Profile";
                ProfilePicture.Source = SelectedUser.Avatar == null ? "default_user_256.png" : SelectedUser.Avatar;
                UserName.Text = SelectedUser.UserName;
                Confetti.Text = SelectedUser.Confetti;
                Comments.Text = SelectedUser.Comments;
                Holidays.Text = SelectedUser.Approved;
                LastOnline.Text = SelectedUser.LastOnline;
            }

            UserCommentList.ItemsSource = await GetUserComments(SelectedUser.UserName);

        }

        async void OnCommentSelected(object sender, SelectedItemChangedEventArgs args)
        {

            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var comment = args.SelectedItem as Comment;
            if (!TapLock)
            {
                TapLock = true;
                try
                {
                    await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(comment.HolidayId, null, comment.Id)));
                }
                catch
                {
                    await DisplayAlert("Uh oh!", "We could no longer find that comment.", "Fine");
                }
                await Task.Delay(2000);
                TapLock = false;
            }
        }

        async void OnHolidaySelected(object sender, SelectedItemChangedEventArgs args)
        {

            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var holiday = args.SelectedItem as Holiday;
            if (!TapLock)
            {
                TapLock = true;
                try
                {
                    //Holiday holiday = await viewModel.HolidayStore.GetHolidayById(comment.HolidayId);
                    await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holiday.Id)));
                }
                catch
                {
                    await DisplayAlert("Uh oh!", "We could no longer find that comment.", "Fine");
                }
                await Task.Delay(2000);
                TapLock = false;
            }
        }

        public async Task<IEnumerable<Holiday>> GetUserHolidays(string UserName)
        {
            UserHolidays = new List<Holiday>();
            var values = new Dictionary<string, string>{
                   { "holidays_by", UserName }
                };

            var content = new FormUrlEncodedContent(values);
            dynamic response = null;
  
            response = await App.globalClient.PostAsync(App.HolidailyHost + "/holidays/", content);

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic holidayList = responseJSON.results;


            foreach (var holiday in holidayList)
            {
                string holidayDescription = holiday.description;
                string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";
                UserHolidays.Insert(0, new Holiday()
                {
                    Id = holiday.id,
                    Name = holiday.name,
                    Description = holiday.description,
                    NumComments = holiday.num_comments,
                    TimeSince = holiday.time_since,
                    DescriptionShort = HolidayDescriptionShort,
                    HolidayImage = holiday.image,
                    ShowAd = false,
                    ShowHolidayContent = true,
                    Date = holiday.date,
                    Votes = holiday.votes,
                    CelebrateStatus = holiday.celebrating == true ? "celebrate_active.png" : "celebrate.png",
                    Blurb = holiday.blurb
                });

            }
            if (UserHolidays.Count == 0)
            {
                NoHolidayResultsBox.IsVisible = true;
            }
            else
            {
                NoHolidayResultsBox.IsVisible = false;
            }
            return await Task.FromResult(UserHolidays);
        }


        public async Task<IEnumerable<Comment>> GetUserComments(string UserName)
        {
            UserComments = new List<Comment>();
            var values = new Dictionary<string, string>{
                   { "activity", UserName },
            };
            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic commentList = responseJSON.results;
            foreach (var comment in commentList)
            {
                string voteImage;
                if(comment.votes < 0)
                {
                    voteImage = "down.png";
                }else if(comment.votes == 0)
                {
                    voteImage = "up.png";
                }
                else
                {
                    voteImage = "up_active.png";
                }
                UserComments.Add(new Comment()
                {
                    Id = comment.id,
                    Content = comment.content,
                    HolidayId = comment.holiday_id,
                    UserName = comment.user,
                    TimeSince = comment.time_since,
                    Votes = comment.votes,
                    UpVoteStatus = voteImage
                });
            }
            if (UserComments.Count == 0)
            {
                NoCommentResultsBox.IsVisible = true;
            }
            else
            {
                NoCommentResultsBox.IsVisible = false;
            }
            return await Task.FromResult(UserComments);
        }



        public async void ToggleFindType(object sender, EventArgs args)
        {

            var contentViewSender = (ContentView)sender;
            var labelSender = (Label)contentViewSender.Children[0];
            var searchType = labelSender.Text;
            if (searchType == "Activity")
            {
                ActivityWrapper.IsVisible = true;
                HolidayListWrapper.IsVisible = false;
                ActivitySelected.IsVisible = true;
                HolidaysSelected.IsVisible = false;
                ActivityHeader.TextColor = Color.FromHex("4c96e8");
                UserHolidaysHeader.TextColor = Color.Gray;
            }
            else
            {
                HolidayListWrapper.IsVisible = true;
                ActivityWrapper.IsVisible = false;
                HolidaysSelected.IsVisible = true;
                ActivitySelected.IsVisible = false;
                ActivityHeader.TextColor = Color.Gray;
                UserHolidaysHeader.TextColor = Color.FromHex("4c96e8");
                if(UserHolidays != null)
                {
                    if(UserHolidays.Count == 0)
                        UserHolidaysList.ItemsSource = await GetUserHolidays(SelectedUser.UserName);
                }
                else
                {
                    UserHolidaysList.ItemsSource = await GetUserHolidays(SelectedUser.UserName);
                }
                
            }
        }

    }
}
