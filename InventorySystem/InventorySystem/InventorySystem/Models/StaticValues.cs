﻿using System;
using Xamarin.Essentials;

namespace InventorySystem.Models
{
    public class StaticValues
    {
        public static string UserId
        {
            get => Preferences.Get(nameof(UserId), Guid.Empty.ToString());
            set => Preferences.Set(nameof(UserId), value);
        }

        public static string FirstName
        {
            get => Preferences.Get(nameof(FirstName), string.Empty);
            set => Preferences.Set(nameof(FirstName), value);
        }

        public static string LastName
        {
            get => Preferences.Get(nameof(LastName), string.Empty);
            set => Preferences.Set(nameof(LastName), value);
        }

        public static string Username
        {
            get => Preferences.Get(nameof(Username), string.Empty);
            set => Preferences.Set(nameof(Username), value);
        }

        public static string Email
        {
            get => Preferences.Get(nameof(Email), string.Empty);
            set => Preferences.Set(nameof(Email), value);
        }

        public static bool IsAdmin
        {
            get => Preferences.Get(nameof(IsAdmin), false);
            set => Preferences.Set(nameof(IsAdmin), value);
        }

        public static void RemoveUserData()
        {
            Preferences.Remove(nameof(UserId));
            Preferences.Remove(nameof(FirstName));
            Preferences.Remove(nameof(LastName));
            Preferences.Remove(nameof(Username));
            Preferences.Remove(nameof(Email));
            Preferences.Remove(nameof(IsAdmin));
        }
    }
}