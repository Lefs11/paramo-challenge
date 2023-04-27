using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sat.Recruitment.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Sat.Recruitment.Api.Controllers
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Errors { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public partial class UsersController : ControllerBase
    {

        private readonly List<User> _users = new List<User>();
        public UsersController()
        {
        }

        [HttpPost]
        [Route("/create-user")]
        public Result CreateUser(string name, string email, string address, string phone, string userType, string money)
        {
            var errors = ValidateErrors(name, email, address, phone);

            if (errors != null)
                return new Result()
                {
                    IsSuccess = false,
                    Errors = errors
                };

            var newUser = new User
            {
                Name = name,
                Email = NormalizeEmail(email),
                Address = address,
                Phone = phone,
                UserType = userType,
                Money = decimal.Parse(money)
            };

            try
            {
                AddUser(newUser);

                return new Result()
                {
                    IsSuccess = true,
                    Errors = "User Created"
                };
            }
            catch
            {
                return new Result()
                {
                    IsSuccess = false,
                    Errors = "The user is duplicated"
                };
            }
        }

        //Validate errors
        private string ValidateErrors(string name, string email, string address, string phone)
        {
            if (string.IsNullOrEmpty(name))
                return "The name is required";
            else if (string.IsNullOrEmpty(email))
                return "The email is required";
            else if (string.IsNullOrEmpty(address))
                return "The address is required";
            else if (string.IsNullOrEmpty(phone))
                return "The phone is required";
            else
                return null;
        }

        //Normalize email
        private string NormalizeEmail(string email)
        {
            var aux = email.Split("@");
            var atIndex = aux[0].IndexOf("+");
            var username = atIndex < 0 ? aux[0].Replace(".", "") : aux[0].Replace(".", "").Remove(atIndex);

            return username + "@" + aux[1];
        }

        private User ParseUser(string line)
        {
            var values = line.Split(',');
            return new User
            {
                Name = values[0],
                Email = values[1],
                Phone = values[2],
                Address = values[3],
                UserType = values[4],
                Money = decimal.Parse(values[5])
            };
        }

        private void UserTypePercentage(User user)
        {
            var gif = 0m;
            if (user.UserType == "Normal")
            {
                if (user.Money > 100)
                {
                    gif = user.Money * 0.12m;
                }
                else
                {
                    if (user.Money > 10)
                    {
                        gif = user.Money * 0.8m;
                    }
                }
            }
            if (user.UserType == "SuperUser")
            {
                if (user.Money > 100)
                {
                    gif = user.Money * 0.20m;
                }
            }
            if (user.UserType == "Premium")
            {
                if (user.Money > 100)
                {
                    gif = user.Money * 2;
                }
            }
            user.Money += gif;
        }

        private StreamReader ReadUsersFromFile()
        {
            var path = Directory.GetCurrentDirectory() + "/Files/Users.txt";
            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(fileStream);

            return reader;
        }

        private void AddUser(User newUser)
        {
            var reader = ReadUsersFromFile();
            var isDuplicated = false;
            
            while (reader.Peek() >= 0)
            {
                var line = reader.ReadLineAsync().Result;
                var user = ParseUser(line);

                if (user.Email == newUser.Email || user.Phone == newUser.Phone)
                {
                    isDuplicated = true;
                }
                else if (user.Name == newUser.Name && user.Address == newUser.Address)
                {
                    isDuplicated = true;
                }
            }
            reader.Close();

            if (!isDuplicated)
            {
                UserTypePercentage(newUser);
                _users.Add(newUser);
            }
            else
            {
                throw new Exception("The user is duplicated");
            }
        }
    }

}
