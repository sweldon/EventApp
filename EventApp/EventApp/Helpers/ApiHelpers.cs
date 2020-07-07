using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;


namespace EventApp
{
    public class ApiHelpers
    {
        public static string currentUser
        {
            get { return Settings.CurrentUser; }
        }
        public static async Task<Object> MakePostRequest(Dictionary<string, string> values, string endpoint)
        {
            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync($"{App.HolidailyHost}/{endpoint}/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            return await Task.FromResult(responseJSON);
        }

        public static async Task<object> MakePatchRequest(Dictionary<string, string> values, string endpoint, string pk)
        {
            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PatchAsync($"{App.HolidailyHost}/{endpoint}/{pk}/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            return await Task.FromResult(responseJSON);
        }

        public static async Task<Holiday> GetHolidayById(string id)
        {

            var values = new Dictionary<string, string>{
                   { "id", id },
                   { "username", currentUser }
                };

            var content = new FormUrlEncodedContent(values);

            int TotalNumberOfAttempts = 12;
            int numberOfAttempts = 0;
            dynamic response = null;
            while (response == null)
            {
                try
                {
                    response = await App.globalClient.PostAsync(App.HolidailyHost + "/holidays/" + id + "/", content);
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
            dynamic holiday = JsonConvert.DeserializeObject(responseString);
            holiday = holiday.results;
            string holidayDescription = holiday.description;
            string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";
            Holiday individualHoliday = new Holiday()
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
                CelebrateStatus = holiday.celebrating == true ? "celebrate_active.png" : "celebrate.png"
            };

            return await Task.FromResult(individualHoliday);

        }

        public static async Task<Comment> GetCommentById(string id)
        {
            Comment individualComment = null;
            var values = new Dictionary<string, string>{
                   { "id", id }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/" + id + "/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic commentJSON = responseJSON.results;

            try
            {
                string TimeAgo = commentJSON.time_since;

                individualComment = new Comment()
                {
                    Id = commentJSON.id,
                    Content = commentJSON.content,
                    HolidayId = commentJSON.holiday_id,
                    UserName = commentJSON.user,
                    TimeSince = TimeAgo
                };
            }
            catch
            {

            }


            return await Task.FromResult(individualComment);

        }

    }
}
