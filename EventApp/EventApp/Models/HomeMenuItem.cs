using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
namespace EventApp.Models
{
    public enum MenuItemType
    {
        Browse,
        Trending
 
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }

        public Image Image { get; set; }
    }
}
