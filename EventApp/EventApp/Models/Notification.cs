using System;

namespace EventApp.Models
{
    public class Notification
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public bool Read { get; set; }
        public string Content { get; set; }
        public string TimeSince { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
}