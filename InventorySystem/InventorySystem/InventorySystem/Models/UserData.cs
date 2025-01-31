﻿using System;
using Newtonsoft.Json;

namespace InventorySystem
{
    public class UserData
    {
        // Class modeling data about user from LoginVerify

        [JsonProperty("id")] public Guid Id { get; set; }

        [JsonProperty("token")] public string Token { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("email")] public string Email { get; set; }

        [JsonProperty("firstName")] public string FirstName { get; set; }

        [JsonProperty("lastName")] public string LastName { get; set; }

        [JsonProperty("isAdmin")] public bool IsAdmin { get; set; }
    }
}