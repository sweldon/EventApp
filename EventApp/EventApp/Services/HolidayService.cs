﻿using System;
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
        Dictionary<string, string> holidayResult;

        HttpClient client = new HttpClient();

        public HolidayService()
        {
            

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


        public async Task<Holiday> GetHolidayByName(string name)
        {

            var values = new Dictionary<string, string>{
                   { "day_name", name }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_holiday/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            individualHoliday = new Holiday() { Name=responseJSON.name,Description=responseJSON.description};
  
            return await Task.FromResult(individualHoliday);
        }

        public async Task<Holiday> GetHolidayById(string id)
        {

            var values = new Dictionary<string, string>{
                   { "id", id }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_holiday_by_id/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            individualHoliday = new Holiday() { Id=id, Name = responseJSON.name, Description = responseJSON.description };

            return await Task.FromResult(individualHoliday);

        }

        public async Task<IEnumerable<Holiday>> GetHolidaysAsync(bool forceRefresh = false)
        {
            items = new List<Holiday>();

            DateTime currentDate = DateTime.Today;
            string dateString = currentDate.ToString("dd-MM-yyyy");
            string dayNumber = dateString.Split('-')[0].TrimStart('0');
            int monthNumber = Int32.Parse(dateString.Split('-')[1]);

            List<string> months = new List<string>() {
                "January","February","March","April","May","June","July",
                "August", "September", "October", "November", "December"
            };

            string monthString = months[monthNumber - 1];

            var values = new Dictionary<string, string>{
                   { "month", monthString },
                   { "day", dayNumber }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_holidays/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic holidayList = responseJSON.HolidayList;



            foreach (var holiday in holidayList)
            {
                string holidayDescription = holiday.description;
                string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";
                items.Insert(0, new Holiday() { Id = holiday.id, Name = holiday.name, Description = holiday.description, NumComments = holiday.num_comments, TimeSince = holiday.time_since, DescriptionShort= HolidayDescriptionShort });
            }

            return await Task.FromResult(items);
        }


        public async Task<IEnumerable<Holiday>> GetTopHolidays(bool forceRefresh = false)
        {
            items = new List<Holiday>();
            var response = await client.GetAsync(App.HolidailyHost + "/portal/get_top_holidays/");
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic holidayList = responseJSON.TopHolidayList;
            foreach (var holiday in holidayList)
            {
                string holidayDescription = holiday.description;;
                string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";
                items.Insert(0, new Holiday() { Id = holiday.id, Name = holiday.name, Description = holiday.description, NumComments = holiday.num_comments, TimeSince = holiday.time_since, DescriptionShort = HolidayDescriptionShort, Votes=holiday.votes });
            }

            return await Task.FromResult(items);
        }

    }
}