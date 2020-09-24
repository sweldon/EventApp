using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net.Http;

namespace EventApp.Services
{
    public class GlobalServices
    {

        public static string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public static bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }

        public async static Task<ObservableCollection<Tweet>> GetTweets(int page=0)
        {
            ObservableCollection<Tweet> tweets = new ObservableCollection<Tweet>();
            try
            {
                var response = await App.globalClient.GetAsync($"{App.HolidailyHost}/tweets?page={page}");
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic tweetList = JsonConvert.DeserializeObject(responseString);

                foreach (var t in tweetList)
                {
                    tweets.Add(new Tweet()
                    {
                        UserName = t.user,
                        Body = t.body,
                        Timestamp = t.timestamp,
                        Image = t.image,
                        UserProfileImage = t.user_profile_image,
                        ShowImage = t.image == null ? false : true,
                        Handle = t.handle,
                        Verified = t.user_verified,
                        Url = t.url,
                    });
                }
            }
            catch
            {

            }


            return await Task.FromResult(tweets);

        }

        public static async Task<ObservableCollection<Holiday>> GetHolidaysAsync(int page = 0, bool past = false)
        {
            ObservableCollection<Holiday> items = new ObservableCollection<Holiday>();
            var values = new Dictionary<string, string>{
                { "page", page.ToString() }
            };

            if (isLoggedIn)
                values["username"] = currentUser;

            if (past)
                values["past"] = "true";

            var content = new FormUrlEncodedContent(values);
            int TotalNumberOfAttempts = 12;
            int numberOfAttempts = 0;
            dynamic response = null;
            while (response == null)
            {
                try
                {
                    response = await App.globalClient.PostAsync(App.HolidailyHost + "/holidays/", content);
                }
                catch
                {
                    numberOfAttempts++;
                    if (numberOfAttempts >= TotalNumberOfAttempts)
                        throw;
                    await Task.Delay(5000);
                }
            }


            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic holidayList = responseJSON.results;

            foreach (var holiday in holidayList)
            {
                string holidayDescription = holiday.description;
                string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";
                items.Add(new Holiday()
                {
                    Id = holiday.id,
                    Name = holiday.name,
                    Description = holiday.description,
                    NumComments = holiday.num_comments,
                    TimeSince = holiday.time_since,
                    DescriptionShort = HolidayDescriptionShort,
                    HolidayImage = holiday.image,
                    ShowAd = false,
                    ShowHolidayContent = true,
                    Date = holiday.date,
                    Votes = holiday.votes,
                    CelebrateStatus = holiday.celebrating == true ? "celebrate_active.png" : "celebrate.png",
                    Blurb = holiday.blurb,
                    Active = holiday.active
                });

            }

            return await Task.FromResult(items);
        }

        public static async Task VoteHoliday(string holidayId, string vote)
        {

            var values = new Dictionary<string, string>{
                   { "username", currentUser },
                   { "vote", vote }
                };

            var content = new FormUrlEncodedContent(values);
            await App.globalClient.PostAsync(App.HolidailyHost + "/holidays/" + holidayId + "/", content);

        }
    }
}

