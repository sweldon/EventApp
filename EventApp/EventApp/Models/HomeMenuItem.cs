using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
namespace EventApp.Models
{
    public enum MenuItemType
    {
        Holidays,
        Notifications,
        Search,
        Updates,
        Premium,
        Rewards,
        AddHoliday,
        ConfettiLeaders,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }

        public string MenuImage { get; set; }
    }
}
