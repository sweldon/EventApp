using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;

namespace EventApp.Services
{

    public class HolidayService : HolidayInterface<Holiday>
    {

        List<Holiday> items;
        Holiday individualHoliday;

        public HolidayService()
        {
            

        }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public async Task<bool> AddItemAsync(Holiday item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Holiday item)
        {
            var oldItem = items.Where((Holiday arg) => arg.Description == item.Description).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }


        public async Task<Holiday> GetHolidayById(string id)
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
            individualHoliday = new Holiday() {
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

        public async Task<IEnumerable<Holiday>> SearchHolidays(string searchText)
        {
            items = new List<Holiday>();

            var values = new Dictionary<string, string>{
                   { "search", searchText },
                };

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/search/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic holidayList = responseJSON.results;
            foreach (var holiday in holidayList)
            {
                string holidayDescription = holiday.description;
                string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";
                items.Insert(0, new Holiday()
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

        public async Task<IEnumerable<Holiday>> GetTopHolidays(bool forceRefresh = false)
        {
            items = new List<Holiday>();
            var response = await App.globalClient.GetAsync(App.HolidailyHost + "/holidays?top=true");
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic holidayList = responseJSON.results;
            foreach (var holiday in holidayList)
            {
                string holidayDescription = holiday.description;
                string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";
                items.Insert(0, new Holiday()
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
                    Blurb = holiday.blurb,
                    CelebrateStatus = holiday.celebrating == true ? "celebrate_active.png" : "celebrate.png"
                });

            }

            return await Task.FromResult(items);
        }



    }
}