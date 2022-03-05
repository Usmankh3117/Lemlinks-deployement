using LimLink_API.DBHelper;
using LimLink_API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LimLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditController : ControllerBase
    {
        private readonly ApplicationDbContext _dBContext;

        public CreditController(ApplicationDbContext dbContext)
        {
            _dBContext = dbContext;
        }

        [HttpPost]
        [Route("GetAccountCredit")]
        public IActionResult GetAccountCredit([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                var accountSetting = _dBContext.AccountSetting.FirstOrDefault(a => a.UserId == model.UserId);
                if (accountSetting != null)
                    return Ok(new { Status = true, Message = "Success", data = accountSetting });
                else
                    return Ok(new { Status = false, Message = "No credit found." });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("DeductAccountCredit")]
        public IActionResult DeductAccountCredit([FromBody] DeductCreditModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId) && model.Credit > 0)
            {
                var accountSetting = _dBContext.AccountSetting.FirstOrDefault(a => a.UserId == model.UserId);
                if (accountSetting != null && accountSetting.AvailableCredit > 0)
                {
                    accountSetting.AvailableCredit = accountSetting.AvailableCredit - model.Credit;
                    accountSetting.UpdatedOn = DateTime.Now;
                    _dBContext.SaveChanges();
                    return Ok(new { Status = true, Message = "Success", data = accountSetting });
                }
                else
                    return Ok(new { Status = false, Message = "No credit found." });
            }
            return Ok(new { Status = false, Message = "Error" });
        }
    }
}
