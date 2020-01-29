using System;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace EventApp.ViewModels
{
    public class ConfettiLeaderViewModel : BaseViewModel
    {
        public ObservableCollection<User> UserList { get; set; }
        public Command LoadUsers { get; set; }

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
                var response = await App.globalClient.GetAsync(App.HolidailyHost + "/users/top");
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                dynamic userList = responseJSON.results;
                foreach (var user in userList)
                {
                    UserList.Insert(0, new User() {
                        UserName = user.username,
                        Confetti = user.confetti,
                        Submissions = user.holiday_submissions,
                        Approved = user.approved_holidays,
                        Comments = user.comments });
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
