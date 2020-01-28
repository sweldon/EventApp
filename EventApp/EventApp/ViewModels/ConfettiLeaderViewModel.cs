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
                var response = await App.globalClient.GetAsync(App.HolidailyHost + "/portal/get_confetti_leaders/");
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                dynamic userList = responseJSON.user_list;
                foreach (var user in userList)
                {
                    UserList.Insert(0, new User() { UserName = user.username, Confetti = user.confetti, Submissions = user.submissions, Approved = user.approved, Comments = user.comments });
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
