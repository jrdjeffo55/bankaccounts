using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BankAccounts.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private bankaccountsContext _context;

        public HomeController(bankaccountsContext context)
        {
            _context = context;
        }

        // GET: /Home/
        [HttpGet]
        [Route("")]
        public IActionResult RegisterPage()
        {
            return View("Register");
        }

        [HttpGet]
        [Route("login")]
        public IActionResult LoginPage()
        {
            ViewBag.NiceTry = TempData["NiceTry"];
            return View("Login");
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult Logout()
        {
            return RedirectToAction("LoginPage");
        }



//================================== Register and login automatically =========================================
        [HttpPost]
        [Route("register")]
        public IActionResult Create(RegisterViewModel NewUser)
        {
            if (ModelState.IsValid)
            {
                // Check if Email is alredy regitered in DataBase
                List<User> CheckEmail = _context.Users.Where(theuser => theuser.Email == NewUser.Email).ToList();
                if (CheckEmail.Count > 0)
                {
                    ViewBag.ErrorRegister = "Email already in use...";
                    return View("Register");
                }
                // Password hashing
                PasswordHasher<RegisterViewModel> Hasher = new PasswordHasher<RegisterViewModel>();
                NewUser.Password = Hasher.HashPassword(NewUser, NewUser.Password);
                // Adding the created User Object to the DB
                User user = new User
                {
                    FirstName = NewUser.FirstName,
                    LastName = NewUser.LastName,
                    Email = NewUser.Email,
                    Password = NewUser.Password,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                // Extracting the JustCreated user in order to obtain his ID for the creation of the account
                User JustCreated = _context.Users.Single(theUser => theUser.Email == NewUser.Email);
                System.Console.WriteLine(JustCreated.UserId);
                // Create an account for the JustRegistered User
                Account account = new Account
                {
                    Balance = 100,
                    UserId = (int)JustCreated.UserId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Accounts.Add(account);
                _context.SaveChanges();
                // Saving ID and Name of the user who just regitered and automaticaly is logged into his account.
                // Passing the newly created acount id to the the redirect
                Account LoggedUserAccount = _context.Accounts.Single(theAccount => theAccount.UserId == JustCreated.UserId);
                HttpContext.Session.SetInt32("UserId", (int)JustCreated.UserId);
                HttpContext.Session.SetString("UserName", (string)JustCreated.FirstName + " " + (string)JustCreated.LastName);
                return RedirectToAction("Account", new { id = LoggedUserAccount.AccountId });
            }
            return View("Register");
        }

//============================================= Login a User ==================================================
        [HttpPost]
        [Route("loginNow")]
        public IActionResult Login(string LEmail = null, string Password = null)
        {
            // Checking if user inputs anything in the fields
            if(Password != null && LEmail != null)
            {
                // Checking if a User this provided Email exists in DB
                User CheckUser = _context.Users.Single(theuser => theuser.Email == LEmail);
                if (CheckUser != null)
                {
                    // Checking if the password matches
                    var Hasher  = new PasswordHasher<User>();
                    if(0 != Hasher.VerifyHashedPassword(CheckUser, CheckUser.Password, Password))
                    {
                        // If the checks are validated than save his ID and Name in session and redirect to the page with his account info
                        Account LoggedUserAccount = _context.Accounts.Single(theAccount => theAccount.UserId == CheckUser.UserId);
                        HttpContext.Session.SetInt32("UserId", (int)CheckUser.UserId);
                        HttpContext.Session.SetString("UserName", (string)CheckUser.FirstName + " " + (string)CheckUser.LastName);
                        return RedirectToAction("Account", new { id = LoggedUserAccount.AccountId });
                    }
                }
            }
            // If check are not validated display an error
            ViewBag.ErrorLogin = "Invalid Login Data...";
            return View("Login");
        }

// ==============================Display the Logged User Acount Information====================================
        [HttpGet]
        [Route("account/{id}")]
        public IActionResult Account(int id)
        {
            // Checking if user is logged in to access the account info
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                TempData["NiceTry"] = "You need to be logged in to view account info!";
                return RedirectToAction("LoginPage");
            }
            // If user is logged in he will be able to view only his own account info
            Account LoggedUserAccount = _context.Accounts.Include(theAccount => theAccount.Transactions).Single(theAccount => theAccount.UserId == HttpContext.Session.GetInt32("UserId"));
            if (LoggedUserAccount.AccountId != id)
            {
                TempData["NiceTry"] = "You can not access account information of other Users! Nice Try:)";
                return RedirectToAction("LoginPage");
            }
            // Order transactions in descending oreder
            LoggedUserAccount.Transactions = LoggedUserAccount.Transactions.OrderByDescending(t => t.Date).ToList();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Account = LoggedUserAccount;
            ViewBag.NotEnough = TempData["NotEnough"];
            return View("Account");
        }
// =============================Process The Transaction User is submiting======================================
        [HttpPost]
        [Route("process")]
        public IActionResult Process(double quantity, int accountId, int accountBalance)
        {
            Account LoggedUserAccount = _context.Accounts.Single(theAccount => theAccount.UserId == HttpContext.Session.GetInt32("UserId"));
            if(LoggedUserAccount.Balance + quantity >= 0)
            {
                // Change the balance amount if the check passes
                LoggedUserAccount.Balance += quantity;
                _context.SaveChanges();
                // Create a new transaction for that account
                Transaction transaction = new Transaction
                    {
                        Amount = quantity,
                        AccountId = accountId,
                        Date = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                _context.Transactions.Add(transaction);
                _context.SaveChanges();
                return RedirectToAction("Account", new { id = LoggedUserAccount.AccountId });
            }
            // if user is trying to withdraw more money than he has in his account redirect with an warning message.
            TempData["NotEnough"] = "WARNING! You don't have enough money to withdraw the amount entered :) Nice Try!";
            return RedirectToAction("Account", new { id = LoggedUserAccount.AccountId });
        }
    }
}