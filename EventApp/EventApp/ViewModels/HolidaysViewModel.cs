using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using EventApp.Models;
using EventApp.Views;
using System.Collections.Generic;

namespace EventApp.ViewModels
{
    public class HolidaysViewModel : BaseViewModel
    {

        public Command LoadItemsCommand { get; set; }
        private List<HolidayList> HolidayList;
        public List<HolidayList> GroupedHolidayList { get { return HolidayList; } set { HolidayList = value; base.OnPropertyChanged(); } }

        public HolidaysViewModel()
        {
            GroupedHolidayList = new List<HolidayList>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {

                var Items = new HolidayList();
                var OldList = new HolidayList();
                var TomorrowList = new HolidayList();
                GroupedHolidayList = new List<HolidayList>();

                var holidays = await HolidayStore.GetHolidaysAsync(true);

                foreach (var holiday in holidays)
                {
                    if (holiday.TimeSince == "Today") {
                        Items.Insert(0, holiday);
                    }
                    else if(holiday.TimeSince == "Tomorrow")
                    {
                        TomorrowList.Insert(0, holiday);
                    }
                    else
                    {
                        OldList.Insert(0, holiday);
                    }
                    
                }

                Items.Heading = "Today";
                Items.HeadingImage = "today_icon.png";
                OldList.Heading = "Past Week";
                OldList.HeadingImage ="past_icon.png";
                TomorrowList.Heading = "Tomorrow";
                TomorrowList.HeadingImage = "tomorrow_icon.png";


                var list = new List<HolidayList>()
                {
                    Items,
                    TomorrowList,
                    OldList
                };

                GroupedHolidayList = list;
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