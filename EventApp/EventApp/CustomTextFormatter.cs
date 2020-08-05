using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using EventApp.Views;
using Xamarin.Forms;

namespace EventApp
{
    public class CustomTextFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var formatted = new FormattedString();
            // TODO find out why this is looping 2x
            foreach (var item in ProcessString((string)value))
            {
                formatted.Spans.Add(CreateSpan(item));
            }
            return formatted;
        }

        private Span CreateSpan(StringSection section)
        {
            var span = new Span()
            {
                Text = section.Text
            };

            if (section.Text.StartsWith("@"))
            {
                // TODO set parameter to section.Text without the @
                span.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = _openProfile,
                    CommandParameter = section.Text
                });
                span.TextColor = Color.FromHex("4c96e8");
            }
            else if (!string.IsNullOrEmpty(section.Link))
            {
                span.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = _navigationCommand,
                    CommandParameter = section.Link
                });
                span.TextColor = Color.Blue;
            } 

            return span;
        }

        public IList<StringSection> ProcessString(string rawText)
        {
            
            const string spanPattern = @"(((http|https):\/\/)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*))|@([a-zA-Z0-9_]+)";

            MatchCollection collection = Regex.Matches(rawText, spanPattern, RegexOptions.Singleline);

            var sections = new List<StringSection>();

            var lastIndex = 0;
            foreach (Match item in collection)
            {
                
                var foundText = item.Value;
                // Add span up to link, from 0 to start of url (item.Index)
                sections.Add(new StringSection() { Text = rawText.Substring(lastIndex, item.Index - lastIndex) });
                lastIndex = item.Index + item.Length;
                // Add actual span for url
                var html = new StringSection()
                {
                    Link = item.Value,
                    Text = item.Value,
                };
                
                sections.Add(html);
            }

            // Add rest
            sections.Add(new StringSection() { Text = rawText.Substring(lastIndex) });
            return sections;
        }

        public class StringSection
        {
            public string Text { get; set; }
            public string Link { get; set; }
        }

        private ICommand _navigationCommand = new Command<string>((url) =>
        {
            try
            {
                if (!url.StartsWith("http") && !url.StartsWith("https://"))
                    url = $"https://{url}";
                Device.OpenUri(new Uri(url));
            }
            catch
            {
                Application.Current.MainPage.DisplayAlert("Uh oh!", "That's " +
                    "not a valid link", "OK");
            }

        });

        private ICommand _openProfile = new Command<string>((username) =>
        {
            string usernameCleaned = username.Substring(1);
            int modalStackLength = Application.Current.MainPage.Navigation.ModalStack.Count;
            if(modalStackLength > 0)
            {
                //Device.BeginInvokeOnMainThread(() =>
                //{
                //    ((RootPage)Application.Current.MainPage).Detail.
                //    Navigation.PopModalAsync();
      
                //        ((RootPage)Application.Current.MainPage).Detail.
                //        Navigation.PushAsync(
                //            new UserPage(user: null, userName: usernameCleaned));

                //});
            }
            else
            {
                Debug.WriteLine($"Adding command for push async for {username}");
                Device.BeginInvokeOnMainThread(() => {
                    ((RootPage)Application.Current.MainPage).Detail.
                    Navigation.PushAsync(new UserPage(
                        user: null, userName: usernameCleaned));
                });
            }


        });

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
