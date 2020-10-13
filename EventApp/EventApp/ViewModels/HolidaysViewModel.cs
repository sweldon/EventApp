using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using EventApp.Models;
using EventApp.Views;
using System.Collections.Generic;
using System.Linq;

namespace EventApp.ViewModels
{
    public class HolidaysViewModel : BaseViewModel
    {

        public Command LoadItemsCommand { get; set; }
        private List<HolidayList> HolidayList;
        public List<HolidayList> GroupedHolidayList { get { return HolidayList; }
            set { HolidayList = value; base.OnPropertyChanged(); } }
        public ObservableCollection<Holiday> Holidays { get; set; }
        public HolidaysViewModel()
        {
            GroupedHolidayList = new List<HolidayList>();
            Holidays = new ObservableCollection<Holiday>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

        }

        // Key code responsible for maintaining celebration status
        // Across pages. Very cool how it works, use this in the future.
        public void UpdateCelebrateStatus(string holiday, bool upvote, string newVotes)
        {
            Debug.WriteLine($"Updating celebrate status of {holiday} to {upvote}");
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
        public async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Holidays.Clear();

                var holidays = await Services.GlobalServices.GetHolidaysAsync();

                foreach (var holiday in holidays)
                {

                    Holidays.Add(holiday);
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