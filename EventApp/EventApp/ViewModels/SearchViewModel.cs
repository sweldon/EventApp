using System;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using EventApp.Views;
using System.Collections.Generic;

namespace EventApp.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        public ObservableCollection<Holiday> HolidayResults { get; set; }
        public DateTime SelectedDate { get; set; }

        public Command LoadTopHolidays { get; set; }
        public ObservableCollection<Holiday> TopHolidays { get; set; }

        public SearchViewModel()
        {
            HolidayResults = new ObservableCollection<Holiday>();
            SelectedDate = DateTime.Now;
            TopHolidays = new ObservableCollection<Holiday>();
            LoadTopHolidays = new Command(async () => await ExecuteLoadTopHolidays());
        }

        async Task ExecuteLoadTopHolidays()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                TopHolidays.Clear();
                var holidays = await HolidayStore.GetTopHolidays(true);
                foreach (var holiday in holidays)
                {
                    TopHolidays.Insert(0, holiday);
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
