using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LimLink_API.DBHelper;
using LimLink_API.Helper;
using LimLink_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace LimLink_API.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dBContext;

        public HomeController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            this._userManager = userManager;
            _configuration = configuration;
            _dBContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                string webhookSecert = "whsec_xlzTJYMTZ8BQRC7shwHp3sMSsXYZCTo9"; // Live
                //string webhookSecert = "whsec_PNG7V3ZxiNmdKFTD2UmmIIe6A90VnkWx"; // Test
                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  Request.Headers["Stripe-Signature"],
                  webhookSecert
                );

                // Handle the checkout.session.completed event
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (session != null)
                    {
                        if (session.PaymentStatus == "paid")
                        {
                            string priceId = string.Empty;
                            string customerEmail = session.CustomerDetails.Email;
                            long amountPaid = session.AmountTotal.HasValue ? (session.AmountTotal.Value / 100) : 0;
                            long credit = 0;
                            string credirDescription = string.Empty;

                            var options = new SessionListLineItemsOptions
                            {
                                Limit = 2,
                            };
                            var service = new SessionService();
                            StripeList<LineItem> lstItems = service.ListLineItems(session.Id, options);

                            if (lstItems != null && lstItems.Count() > 0)
                            {
                                if (lstItems.Data.Count > 0)
                                {
                                    priceId = lstItems.Data[0].Price.Id;
                                    credirDescription = lstItems.Data[0].Description;
                                }

                            }

                            if (!string.IsNullOrWhiteSpace(priceId) && !string.IsNullOrWhiteSpace(customerEmail))
                            {
                                credit = GetCreditByPrice(priceId);

                                var users = await _userManager.FindByEmailAsync(customerEmail);
                                if (users != null)
                                {
                                    var accountSetting = _dBContext.AccountSetting.FirstOrDefault(a => a.UserId == users.Id);
                                    if (accountSetting == null)
                                    {
                                        accountSetting = new LimLink_Models.AccountSetting();
                                        accountSetting.UserId = users.Id;
                                        accountSetting.TotalCredit = credit;
                                        accountSetting.AvailableCredit = credit;
                                        accountSetting.CreatedOn = DateTime.Now;
                                        accountSetting.UpdatedOn = DateTime.Now;
                                        _dBContext.AccountSetting.Add(accountSetting);
                                    }
                                    else
                                    {
                                        accountSetting.TotalCredit = accountSetting.TotalCredit + credit;
                                        accountSetting.AvailableCredit = accountSetting.AvailableCredit + credit;
                                        accountSetting.UpdatedOn = DateTime.Now;
                                    }

                                    LimLink_Models.PaymentHistory paymentHistory = new LimLink_Models.PaymentHistory();
                                    paymentHistory.AmountPaid = Convert.ToInt32(amountPaid);
                                    paymentHistory.UserId = users.Id;
                                    paymentHistory.PlanId = priceId;
                                    paymentHistory.CreditAdded = credit;
                                    paymentHistory.CreatedOn = DateTime.Now;
                                    _dBContext.PaymentHistory.Add(paymentHistory);
                                    _dBContext.SaveChanges();

                                    string Body = "Hey " + users.FullName + "!<br/><br/>" +
                                   "Thank you for your recent transaction on Lemlinks. " + credirDescription + " successfully added in your Lemlinks account.<br/><br/>" +
                                   "Good Luck! <br/><br/> The Lemlinks Support Team";

                                    EmailServicecs emailSerivce = new EmailServicecs(_configuration);
                                    await emailSerivce.SendEmail(customerEmail, Body, "Thank you for purchase | Lemlinks");
                                }
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public void CreateSession(string PriceId)
        {
            StripeConfiguration.ApiKey = _configuration.GetSection("StripeURLs").GetSection("APIKey").Value;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = PriceId,
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = _configuration.GetSection("StripeURLs").GetSection("SuccessURL").Value,
                CancelUrl = _configuration.GetSection("StripeURLs").GetSection("CancelURL").Value,
            };

            var service = new SessionService();
            var session = service.Create(options);
            Response.Redirect(session.Url);
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Cancel()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                ViewBag.errorMessage = "This looks like not a valid link. Please check your email for the correct link.";
                return View();
            }

            try
            {
                var users = await _userManager.FindByIdAsync(userId);
                if (users != null)
                {
                    var result = await _userManager.ConfirmEmailAsync(users, token);
                    if (result.Succeeded)
                    {
                        ViewBag.successMessage = "Confirm email successfully.";
                        return View();
                    }
                    else
                    {
                        ViewBag.errorMessage = "ConfirmEmail failed";
                        return View();
                    }
                }
            }
            catch (InvalidOperationException ioe)
            {
                if (ioe.Message == "UserId not found.")
                    ViewBag.errorMessage = "This looks like not a valid link. Please check your email for the correct link.";
                else
                    ViewBag.errorMessage = ioe.Message;
                return View();
            }

            ViewBag.errorMessage = "ConfirmEmail failed";
            return View("Error");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            ResetPasswordModel model = new ResetPasswordModel()
            {
                Token = token,
                UserId = userId
            };
            return await Task.Run(() => View(model));
        }

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                model.Token = model.Token.Replace(' ', '+');
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        ModelState.Clear();
                        model.IsSuccess = true;
                        return View(model);
                    }
                    else if (result?.Errors?.Count() > 0)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }
            return await Task.Run(() => View(model));
        }

        public long GetCreditByPrice(string priceId)
        {
            long credit = 0;

            switch (priceId)
            {
                case "price_1JIwE6CrhLDCjGGitNM5grEL":
                    credit = 100000;
                    break;
                case "price_1JIwEcCrhLDCjGGiBsXjydws":
                    credit = 500000;
                    break;
                case "price_1JLLVFCrhLDCjGGiaWuVAH84":
                    credit = 1000000;
                    break;
                case "price_1JLLVdCrhLDCjGGi5EINYFhJ":
                    credit = 2000000;
                    break;
                case "price_1JLLVyCrhLDCjGGiYXY5LwtE":
                    credit = 5000000;
                    break;
                case "price_1JLLWJCrhLDCjGGiuMIIP59O":
                    credit = 10000000;
                    break;
            }

            return credit;
        }
    }
}
