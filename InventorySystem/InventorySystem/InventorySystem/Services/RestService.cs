using InventorySystem.Interfaces;
using InventorySystem.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace InventorySystem.Services
{
    public class RestService : IRestService
    {
        public const string Token = "token";
        private string _token;

        private static HttpClient _client;

        private Item Item { get; set; }
        private List<Item> Items { get; set; }

        public RestService()
        {
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) }; //Po 10 sekundach HttpClient zwr�ci problem z po��czeniem.
            Items = null;
            Item = null;
        }

        public async Task<bool> VerifyLogin(string email, string password)
        {
            var valuesLogin = new Login()
            {
                Email = email,
                Password = password
            };

            var json = JsonConvert.SerializeObject(valuesLogin, Formatting.Indented);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage;

            try
            {
                responseMessage = await _client.PostAsync(new Uri(Constants.AccountLogin), content);
            }
            catch (Exception ex)
            {
                ConnectionErrorMethod(ex);
                return false;
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
                DependencyService.Get<IMessage>().LongAlert(Constants.UnauthorizedError);
                ShowInConsole(responseMessage);
                return false;
            }

            var jsonAsStringAsync = await responseMessage.Content.ReadAsStringAsync();
            var userData = JsonConvert.DeserializeObject<UserData>(jsonAsStringAsync);

            if (userData == null) return false;

            SaveUserDetails(userData);
            await Xamarin.Essentials.SecureStorage.SetAsync(Token, userData.Token);

            await Application.Current.SavePropertiesAsync();
            return true;
        }

        public async Task<bool> Register(string username, string firstname, string lastname, string email, string password)
        {
            var valuesRegister = new Register()
            {
                Email = email,
                FirstName = firstname,
                LastName = lastname,
                Password = password,
                UserName = username
            };

            var json = JsonConvert.SerializeObject(valuesRegister, Formatting.Indented);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage;

            try
            {
                responseMessage = await _client.PostAsync(new Uri(Constants.AccountRegister), content);
            }
            catch (Exception ex)
            {
                ConnectionErrorMethod(ex);
                return false;
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
                DependencyService.Get<IMessage>().LongAlert(Constants.RegistrationError);
                ShowInConsole(responseMessage);
                return false;
            }

            var jsonAsStringAsync = await responseMessage.Content.ReadAsStringAsync();
            var userData = JsonConvert.DeserializeObject<UserData>(jsonAsStringAsync);

            if (userData == null) return false;

            SaveUserDetails(userData);
            await Xamarin.Essentials.SecureStorage.SetAsync(Token, userData.Token);

            await Application.Current.SavePropertiesAsync();
            return true;
        }

        public async Task<bool> GetCurrentUser()
        {
            if (!CheckForToken())
                if (!await GetToken())
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.NoTokenError);
                    Console.WriteLine(new KeyNotFoundException("No token found."));
                    return false;
                }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(Constants.AccountEndpoint)))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage responseMessage;

                try
                {
                    responseMessage = await _client.SendAsync(requestMessage);
                }
                catch (Exception ex)
                {
                    ConnectionErrorMethod(ex);
                    return false;
                }

                if (!responseMessage.IsSuccessStatusCode)
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.ConnectionError);
                    ShowInConsole(responseMessage);
                    return false;
                }

                var jsonAsStringAsync = await responseMessage.Content.ReadAsStringAsync();
                var userData = JsonConvert.DeserializeObject<UserData>(jsonAsStringAsync);

                return userData != null;
            }
        }
        public async Task<List<Item>> GetAllItems()
        {
            if (!CheckForToken())
                if (!await GetToken())
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.NoTokenError);
                    Console.WriteLine(new KeyNotFoundException("No token found."));
                    return null;
                }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(Constants.ItemsEndpoint)))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage responseMessage;

                try
                {
                    responseMessage = await _client.SendAsync(requestMessage);
                }
                catch (Exception ex)
                {
                    ConnectionErrorMethod(ex);
                    return null;
                }

                if (!responseMessage.IsSuccessStatusCode)
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.ItemsError);
                    ShowInConsole(responseMessage);
                    return null;
                }

                var jsonAsStringAsync = await responseMessage.Content.ReadAsStringAsync();
                Items = JsonConvert.DeserializeObject<List<Item>>(jsonAsStringAsync);

                return Items;
            }
        }

        //Methods used by ModifyItemPage
        public async Task<Item> GetSpecificItem(string id)
        {
            if (!CheckForToken())
                if (!await GetToken())
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.NoTokenError);
                    Console.WriteLine(new KeyNotFoundException("No token found."));
                    return null;
                }

            var uri = new Uri(Constants.ItemsEndpoint + "/" + id);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                HttpResponseMessage responseMessage;
                try
                {
                    responseMessage = await _client.SendAsync(requestMessage);
                }
                catch (Exception ex)
                {
                    ConnectionErrorMethod(ex);
                    return null;
                }

                if (!responseMessage.IsSuccessStatusCode)
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.SpecificItemError);
                    ShowInConsole(responseMessage);
                    return null;
                }

                var jsonAsStringAsync = await responseMessage.Content.ReadAsStringAsync();
                Item = JsonConvert.DeserializeObject<Item>(jsonAsStringAsync);

                return Item;
            }
        }
        public async Task<bool> UpdateItem(Guid itemId, Item item)
        {
            if (!CheckForToken())
                if (!await GetToken())
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.NoTokenError);
                    Console.WriteLine(new KeyNotFoundException("No token found."));
                    return false;
                }

            HttpResponseMessage response;

            var uri = Constants.ItemsEndpoint + $"/{itemId}";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, uri);
            var json = JsonConvert.SerializeObject(item, Formatting.Indented);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            requestMessage.Content = content;

            try
            {
                response = await _client.SendAsync(requestMessage);
            }
            catch (Exception e)
            {
                ConnectionErrorMethod(e);
                return false;
            }

            if (response.IsSuccessStatusCode) return true;
            DependencyService.Get<IMessage>().LongAlert(Constants.UpdateItemError);
            ShowInConsole(response);
            return false;
        }
        //
        
        //TODO: Check if DeleteItem works
        public async Task<bool> DeleteItem(Guid itemId)
        {
            if (!CheckForToken())
                if (!await GetToken())
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.NoTokenError);
                    Console.WriteLine(new KeyNotFoundException("No token found."));
                    return false;
                }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, new Uri(Constants.ItemsEndpoint + $"/{itemId}")))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage responseMessage;

                try
                {
                    responseMessage = await _client.SendAsync(requestMessage);
                }
                catch (Exception ex)
                {
                    ConnectionErrorMethod(ex);
                    return false;
                }

                if (!responseMessage.IsSuccessStatusCode)
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.DeletionError);
                    ShowInConsole(responseMessage);
                    return false;
                }
                else
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.DeletionSuccessful);
                    ShowInConsole(responseMessage);
                    return true;
                }
            }
        }

        public async Task<bool> AddItem(Item item)
        {
            if (!CheckForToken())
                if (!await GetToken())
                {
                    DependencyService.Get<IMessage>().LongAlert(Constants.NoTokenError);
                    Console.WriteLine(new KeyNotFoundException("No token found."));
                    return false;
                }
                    


            throw new NotImplementedException();
        }

        //Additional usefull methods
        private static void SaveUserDetails(UserData userData)
        {
            StaticValues.UserId = userData.Id.ToString();
            StaticValues.FirstName = userData.FirstName;
            StaticValues.LastName = userData.LastName;
            StaticValues.Username = userData.Username;
            StaticValues.Email = userData.Email;
            StaticValues.IsAdmin = userData.IsAdmin;
        }
        private async Task<bool> GetToken()
        {
            _token = await Xamarin.Essentials.SecureStorage.GetAsync(Token);
            return CheckForToken();
        }
        private bool CheckForToken()
        {
            if (!string.IsNullOrWhiteSpace(_token)) return true;
            return false;
        }
        private static void ShowInConsole(string message)
        {
            Console.WriteLine("[API Error Message] " + message);
        }
        private static void ShowInConsole(HttpResponseMessage response)
        {
            Console.WriteLine("[API Response Message] " + response.StatusCode + ", " + response.Content.ReadAsStringAsync());
        }
        private void ConnectionErrorMethod(Exception ex)
        {
            DependencyService.Get<IMessage>().LongAlert(Constants.ConnectionError);
            ShowInConsole(ex.Message);
        }
    }
}
