using EFCore.BulkExtensions;
using LimLink_API.DBHelper;
using LimLink_API.Model;
using LimLink_Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace LimLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LimLinkController : ControllerBase
    {
        private readonly ApplicationDbContext dBContext;
        private readonly UserManager<ApplicationUser> userManager;

        public LimLinkController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            dBContext = dbContext;
            this.userManager = userManager;
        }

        #region HistoryLinks

        [HttpPost]
        [Route("SaveHistoryLink")]
        public async Task<IActionResult> SaveHistoryLink([FromBody] HistoryLinkRequestModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId) && model.Links != null && model.Links.Count > 0)
            {
                DateTime PkTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time"));
                List<HistoryLinks> historyLinks = new List<HistoryLinks>();
                //bool isFirstTime = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId).Any();
                var alluserLinks = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId).ToList();

                List<HistoryLinks> lstPreviouslyLinks = new List<HistoryLinks>();
                var allHistoryLinks = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false && a.IsShared == false).ToList();
                foreach (var item in model.Links.Distinct().ToList())
                {
                    if (!allHistoryLinks.Any(a => a.Link.Trim().ToLower() == item.Trim().ToLower()))
                    {
                        historyLinks.Add(new HistoryLinks()
                        {
                            Link = item,
                            CreatedOn = PkTime,
                            UpdatedOn = PkTime,
                            IsDelete = false,
                            IsShared = false,
                            UserId = model.UserId,
                        });
                    }
                    else
                    {
                        var objHistoryLink = allHistoryLinks.FirstOrDefault(a => a.Link.Trim().ToLower() == item.Trim().ToLower());
                        if (objHistoryLink != null)
                        {
                            lstPreviouslyLinks.Add(objHistoryLink);
                        }
                    }
                }
                if (lstPreviouslyLinks.Any())
                {
                    lstPreviouslyLinks.ForEach(a => a.UpdatedOn = PkTime);
                    dBContext.SaveChanges();
                }
                var lastDayLinks = alluserLinks.Where(a => a.CreatedOn >= PkTime.AddHours(-24) || a.UpdatedOn >= PkTime.AddHours(-24)).ToList();
                if (historyLinks.Any())
                    await dBContext.BulkInsertAsync(historyLinks);

                var accountSetting = dBContext.AccountSetting.FirstOrDefault(a => a.UserId == model.UserId);
                if (accountSetting != null && accountSetting.AvailableCredit > 0)
                {
                    int userCredit = 0;
                    foreach (var link in model.Links.Distinct().ToList())
                    {
                        if (!lastDayLinks.Any(a => a.Link.Trim().ToLower() == link.Trim().ToLower()))
                            userCredit++;
                    }

                    accountSetting.AvailableCredit = accountSetting.AvailableCredit - userCredit;
                    dBContext.SaveChanges();
                }

                return Ok(new { Status = true, Message = "Success" });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("GetHistoryLink")]
        public IActionResult GetHistoryLink([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                List<GetHistoryResponse> historyResponse = new List<GetHistoryResponse>();

                var allLinks = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false && a.IsShared == false).ToList();
                var allSavedLink = dBContext.UserLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false).ToList();
                var groupByDate = allLinks.Select(a => a.CreatedOn.Date).Distinct().ToList();

                foreach (var item in groupByDate)
                {
                    GetHistoryResponse history = new GetHistoryResponse();
                    history.Date = item.Date.ToShortDateString();

                    List<AllHistoryLinks> allHistoryLinks = new List<AllHistoryLinks>();
                    var linksByDate = allLinks.Where(a => a.CreatedOn.Date == item).ToList();
                    foreach (var links in linksByDate)
                    {
                        AllHistoryLinks allHistory = new AllHistoryLinks();
                        allHistory.Id = links.Id;
                        allHistory.Link = links.Link;
                        if (allSavedLink.Where(a => a.Link.Trim().ToLower() == links.Link.Trim().ToLower()).Any())
                        {
                            allHistory.isSaved = true;
                            allHistory.SaveLinkId = allSavedLink.Where(a => a.Link.Trim().ToLower() == links.Link.Trim().ToLower()).FirstOrDefault().Id;
                        }
                        else
                            allHistory.isSaved = false;
                        allHistoryLinks.Add(allHistory);
                    }
                    history.historyLinks = allHistoryLinks;
                    historyResponse.Add(history);
                }
                return Ok(new { Status = true, Message = "Success", data = historyResponse });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("DeleteHistoryLinkById")]
        public IActionResult DeleteHistoryLinkById([FromBody] DeleteHistoryModel model)
        {
            if (model.Id > 0)
            {
                var historyLink = dBContext.HistoryLinks.FirstOrDefault(a => a.Id == model.Id);
                if (historyLink != null)
                {
                    historyLink.IsDelete = true;
                    dBContext.SaveChanges();
                    return Ok(new { Status = true, Message = "Success" });
                }
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("DeleteAllHistoryLinks")]
        public IActionResult DeleteAllHistoryLinks([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                var allLinks = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false && a.IsShared == false).ToList();
                allLinks.ForEach(a => a.IsDelete = true);
                dBContext.SaveChanges();
                return Ok(new { Status = true, Message = "Success" });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        #endregion

        #region UserLinks

        [HttpPost]
        [Route("SaveUserLink")]
        public IActionResult SaveUserLink([FromBody] SaveUserLinkModel model)
        {
            if (model.Id > 0)
            {
                DateTime PkTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time"));
                var historyLink = dBContext.HistoryLinks.FirstOrDefault(a => a.Id == model.Id);
                if (historyLink != null)
                {
                    int linkId = 0;
                    var allUserLink = dBContext.UserLinks.Where(a => a.UserId == historyLink.UserId && a.IsDelete == false);
                    if (!allUserLink.Any(a => a.Link.Trim().ToLower() == historyLink.Link.ToLower().Trim()))
                    {
                        UserLinks user = new UserLinks();
                        user.Link = historyLink.Link;
                        user.UserId = historyLink.UserId;
                        user.CreatedOn = PkTime;
                        user.UpdatedOn = PkTime;
                        user.IsDelete = false;
                        dBContext.UserLinks.Add(user);
                        dBContext.SaveChanges();
                        linkId = user.Id;
                    }
                    return Ok(new { Status = true, Message = "Success", SavedId = linkId });
                }
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("GetUserLink")]
        public IActionResult GetUserLink([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                var allLinks = dBContext.UserLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false).ToList();
                return Ok(new { Status = true, Message = "Success", data = allLinks });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("DeleteAllUserLink")]
        public IActionResult DeleteAllUserLink([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                var allLinks = dBContext.UserLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false).ToList();
                allLinks.ForEach(a => a.IsDelete = true);
                dBContext.SaveChanges();
                return Ok(new { Status = true, Message = "Success" });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("DeleteUserLink")]
        public IActionResult DeleteUserLink([FromBody] DeleteHistoryModel model)
        {
            if (model.Id > 0)
            {
                var userLinks = dBContext.UserLinks.FirstOrDefault(a => a.Id == model.Id);
                if (userLinks != null)
                {
                    userLinks.IsDelete = true;
                    dBContext.SaveChanges();
                }
                return Ok(new { Status = true, Message = "Success" });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        #endregion

        #region SharedLinks

        [HttpPost]
        [Route("GetSharedLink")]
        public IActionResult GetSharedLink([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                List<SharedlinkResponse> sharedlinkResponses = new List<SharedlinkResponse>();

                var allLinks = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false && a.IsShared == true).ToList();
                var groupbyUser = allLinks.Select(a => a.SharedByName).Distinct().ToList();

                foreach (var item in groupbyUser)
                {
                    SharedlinkResponse sharedlink = new SharedlinkResponse();
                    sharedlink.SharedUserName = item;

                    List<AllSharedLinks> allSharedLinks = new List<AllSharedLinks>();
                    var linksByUser = allLinks.Where(a => a.SharedByName == item).ToList();
                    foreach (var links in linksByUser)
                    {
                        AllSharedLinks allShared = new AllSharedLinks();
                        allShared.Id = links.Id;
                        allShared.Link = links.Link;
                        allSharedLinks.Add(allShared);
                    }
                    sharedlink.sharedLinks = allSharedLinks;
                    sharedlinkResponses.Add(sharedlink);
                }
                return Ok(new { Status = true, Message = "Success", data = sharedlinkResponses });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("DeleteAllSharedLinks")]
        public IActionResult DeleteAllSharedLinks([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                var allLinks = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId && a.IsDelete == false && a.IsShared == true).ToList();
                allLinks.ForEach(a => a.IsDelete = true);
                dBContext.SaveChanges();
                return Ok(new { Status = true, Message = "Success" });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        #endregion

        [HttpPost]
        [Route("ShareUserLink")]
        public async Task<IActionResult> ShareUserLink([FromBody] LinkShareModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId) && !string.IsNullOrWhiteSpace(model.Email))
            {
                DateTime PkTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time"));
                var shareduser = await userManager.FindByEmailAsync(model.Email);
                if (shareduser == null)
                    return Ok(new { Status = false, Message = "User not found." });

                if (shareduser.Id == model.UserId)
                    return Ok(new { Status = false, Message = "You can't share with yourself." });

                var sharedByuser = await userManager.FindByIdAsync(model.UserId);

                if (model.LinkId > 0)
                {
                    var allHistoryLinks = dBContext.HistoryLinks.Where(a => a.UserId == shareduser.Id && a.IsDelete == false && a.IsShared == true);
                    if (model.Type == 1)
                    {
                        var historyLink = dBContext.HistoryLinks.FirstOrDefault(a => a.Id == model.LinkId);
                        if (historyLink != null)
                        {
                            if (!allHistoryLinks.Any(a => a.Link.Trim().ToLower() == historyLink.Link.Trim().ToLower()))
                            {
                                HistoryLinks historyLinks = new HistoryLinks()
                                {
                                    Link = historyLink.Link,
                                    CreatedOn = PkTime,
                                    UpdatedOn = PkTime,
                                    IsDelete = false,
                                    IsShared = true,
                                    UserId = shareduser.Id,
                                    SharedBy = sharedByuser.Id,
                                    SharedByName = sharedByuser.FullName,
                                };
                                dBContext.HistoryLinks.Add(historyLinks);
                                dBContext.SaveChanges();
                            }
                            return Ok(new { Status = true, Message = "Link successfully shared." });
                        }
                        else
                            return Ok(new { Status = false, Message = "Link not found." });
                    }
                    else
                    {
                        var userLink = dBContext.UserLinks.FirstOrDefault(a => a.Id == model.LinkId);
                        if (userLink != null)
                        {
                            if (!allHistoryLinks.Any(a => a.Link.Trim().ToLower() == userLink.Link.Trim().ToLower()))
                            {
                                HistoryLinks historyLinks = new HistoryLinks()
                                {
                                    Link = userLink.Link,
                                    CreatedOn = PkTime,
                                    UpdatedOn = PkTime,
                                    IsDelete = false,
                                    IsShared = true,
                                    UserId = shareduser.Id,
                                    SharedBy = sharedByuser.Id,
                                    SharedByName = sharedByuser.FullName,
                                };
                                dBContext.HistoryLinks.Add(historyLinks);
                                dBContext.SaveChanges();
                            }
                            return Ok(new { Status = true, Message = "Link successfully shared." });
                        }
                        else
                            return Ok(new { Status = false, Message = "Link not found." });
                    }
                }
                else
                    return Ok(new { Status = false, Message = "Link not found." });
            }
            return Ok(new { Status = false, Message = "Error" });
        }

        [HttpPost]
        [Route("LastDayLinks")]
        public IActionResult LastDayLinks([FromBody] GetHistoryModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                DateTime PkTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time"));
                var historyLinks = dBContext.HistoryLinks.Where(a => a.UserId == model.UserId).ToList();
                var lastDayLinks = historyLinks.Where(a => a.CreatedOn >= PkTime.AddHours(-24) || a.UpdatedOn >= PkTime.AddHours(-24)).ToList();
                return Ok(new { Status = true, Message = "Success", data = lastDayLinks.Select(a => a.Link).ToList() });
            }
            return Ok(new { Status = false, Message = "Error" });
        }
    }
}
