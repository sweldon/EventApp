using System;
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
        string ec2Instance = "http://ec2-18-205-119-102.compute-1.amazonaws.com:5555";
        HttpClient client = new HttpClient();

        public MockDataStore()
        {
            //items = new List<Item>();
            //var mockitems = new list<item>
            //{
            //    new item { id = guid.newguid().tostring(), text = "first item", description="this is an item description." },
            //    new item { id = guid.newguid().tostring(), text = "second item", description="this is an item description." },
            //    new item { id = guid.newguid().tostring(), text = "third item", description="this is an item description." },
            //    new item { id = guid.newguid().tostring(), text = "fourth item", description="this is an item description." },
            //    new item { id = guid.newguid().tostring(), text = "fifth item", description="this is an item description." },
            //    new item { id = guid.newguid().tostring(), text = "sixth item", description="this is an item description." },
            //};

            //foreach (var item in mockitems)
            //{
            //    items.add(item);
            //}

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

            var response = await client.GetAsync(ec2Instance + "/GetUsers");

            var responseString = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseString);
            dynamic responseJSON = JsonConvert.
            DeserializeObject(responseString);

            dynamic userList = responseJSON.UserList;
            

            foreach (var userItem in userList)
            {
                items.Insert(0, new User() { UserName = userItem });
            }

            return await Task.FromResult(items);
        }
    }
}