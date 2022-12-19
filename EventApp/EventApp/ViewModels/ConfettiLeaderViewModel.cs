using System;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EventApp.ViewModels
{
    public class ConfettiLeaderViewModel : BaseViewModel
    {
        public ObservableCollection<User> UserList { get; set; }
        public Command LoadUsers { get; set; }
        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public ConfettiLeaderViewModel()
        {
            UserList = new ObservableCollection<User>();
            LoadUsers = new Command(async () => await LoadConfettiLeaders());

        }

        async Task LoadConfettiLeaders()
        {

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                UserList.Clear();
                var values = new Dictionary<string, string>{
                    { "requesting_user", currentUser },
                };
                dynamic response = await ApiHelpers.MakePostRequest(values, "users");
                dynamic userList = response.results;
                foreach (var user in userList)
                {
                    var avatar = user.profile_image == null ? "default_user_128.png" : user.profile_image;
                    UserList.Add(new User() {
                        UserName = user.username,
                        Confetti = user.confetti,
                        Submissions = user.holiday_submissions,
                        Approved = user.approved_holidays,
                        Comments = user.num_comments,
                        Avatar = avatar,
                        LastOnline = user.last_online
                    });
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }



        }

    }
}
