using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;
namespace EventApp.Models
{
    public class CommentList : ObservableCollection<Comment>
    {
        public ObservableCollection<Comment> Comments => this;
    }
}
