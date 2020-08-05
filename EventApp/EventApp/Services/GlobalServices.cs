using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EventApp.Models;
using System.Collections.ObjectModel;

namespace EventApp.Services
{
    public class GlobalServices
    {



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


    }
}

