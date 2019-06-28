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
        public List<HolidayList> GroupedHolidayList { get { return HolidayList; } set { HolidayList = value; base.OnPropertyChanged(); } }
        public ObservableCollection<Holiday> Holidays { get; set; }
        public HolidaysViewModel()
        {
            GroupedHolidayList = new List<HolidayList>();
            Holidays = new ObservableCollection<Holiday>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Holidays.Clear();
                //var Items = new List<Holiday>();
                //var Items = new HolidayList();
                var OldList = new List<Holiday>();
                //var TomorrowList = new HolidayList();
                //GroupedHolidayList = new List<HolidayList>();

                var holidays = await HolidayStore.GetHolidaysAsync(true);
                var showAd = false;

                // Final Ad, bottom of stack
                //Holidays.Insert(0, new Holiday()
                //{
                //    Id = "-1",
                //    ShowAd = true,
                //    ShowHolidayContent = false,
                //});
                bool todayDone = false;
                foreach (var holiday in holidays)
                {

                    if (holiday.TimeSince == "Tomorrow")
                    {
                        // Skip tomorrow's for now
                        //tomorrowlist.insert(0, holiday);
                    }
                    else
                    {
                        // Put at right after todays
                        //if(holiday.TimeSince == "Today" && !todayDone)
                        //{
                        //    Holidays.Insert(0, new Holiday()
                        //    {
                        //        Id = "-1",
                        //        ShowAd = true,
                        //        ShowHolidayContent = false,
                        //    });
                        //    todayDone = true;
                        //}
                        Holidays.Insert(0, holiday);
                    }


                }

                // Add at location 3
                Holidays.Insert(4, new Holiday()
                {
                    Id = "-1",
                    ShowAd = true,
                    ShowHolidayContent = false,
                });

                // And then add every 3 after that
                //int listCount = Holidays.Count();
                //for(int i = 0; i < listCount; i++)
                //{
                //    if(i % 3 == 0 && i != 0 && i != 3)
                //    {
                //        Holidays.Insert(i, new Holiday()
                //        {
                //            Id = "-1",
                //            ShowAd = true,
                //            ShowHolidayContent = false,
                //        });
                //    }
                //}





                //Items.Heading = "Today";
                //Items.HeadingImage = "today_icon.png";
                //OldList.Heading = "Past Week";
                //OldList.HeadingImage ="past_icon.png";
                //TomorrowList.Heading = "Tomorrow";
                //TomorrowList.HeadingImage = "tomorrow_icon.png";


                //var list = new List<HolidayList>()
                //{
                //    Items
                //};



                //GroupedHolidayList = list;
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