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
        public SearchViewModel()
        {
            HolidayResults = new ObservableCollection<Holiday>();

            SelectedDate = DateTime.Now;
        }

    }
}
