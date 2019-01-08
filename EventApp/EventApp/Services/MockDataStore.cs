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
    public class MockDataStore : IDataStore<User>
    {

        List<User> items;
        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";
        HttpClient client = new HttpClient();

        public MockDataStore()
        {
            

        }

        public async Task<bool> AddItemAsync(User user)
        {
            items.Add(user);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(User user)
        {
            var oldItem = items.Where((User arg) => arg.UserName == user.UserName).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(user);

            return await Task.FromResult(true);
        }


        public async Task<User> GetItemAsync(string username)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.UserName == username));
        }

        public async Task<IEnumerable<User>> GetItemsAsync(bool forceRefresh = false)
        {
            items = new List<User>();

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
            var response = await client.PostAsync(ec2Instance + "/portal/get_holidays/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic userList = responseJSON.HolidayList;
            

            foreach (var userItem in userList)
            {
                items.Insert(0, new User() { UserName = userItem });
            }

            return await Task.FromResult(items);
        }
    }
}