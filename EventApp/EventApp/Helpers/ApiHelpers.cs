using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace EventApp
{
    public class ApiHelpers
    {
        public static async Task<Object> MakePostRequest(Dictionary<string, string> values, string endpoint)
        {
            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync($"{App.HolidailyHost}/{endpoint}/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            return await Task.FromResult(responseJSON);
        }
    }
}
