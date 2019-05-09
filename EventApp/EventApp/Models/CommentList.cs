using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
namespace EventApp.Models
{
    public class CommentList : List<Comment>
    {
        public List<Comment> Comments => this;
    }
}
