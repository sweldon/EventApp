using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
namespace EventApp.Models
{
    public class HolidayList : List<Holiday>
    {
        public string Heading { get; set; }
        public string HeadingImage { get; set; }
        public List<Holiday> Holidays => this;
    }
}
