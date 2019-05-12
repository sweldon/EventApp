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
    public class TrendingViewModel : BaseViewModel
    {
        public Command LoadTopHolidays { get; set; }
        public ObservableCollection<Holiday> TopHolidays { get; set; }

        public TrendingViewModel()
        {
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
