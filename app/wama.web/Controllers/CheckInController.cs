﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WAMA.Core.Models.Service;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WAMA.Web.Controllers
{
    public class CheckInController : WamaBaseController
    {
        private IUserAccountService _UserAccountService;
        private IWaiverService _waiverService;
        private static ICheckInService _CheckInService;

        public CheckInController(IUserAccountService userAccountService, ICheckInService checkInService, IWaiverService waiverService)
        {
            _UserAccountService = userAccountService;
            _waiverService = waiverService;
            _CheckInService = checkInService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string memberId)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(memberId))
            {
                var userAccount = await _UserAccountService.GetUserAccountAsync(memberId);
                var waiverInfo = await _waiverService.GetWaiverAsync(memberId);

                if (userAccount == null)
                {
                    ViewBag.AccountExists = "false";
                    ViewBag.MemberId = memberId;
                    SetErrorMessages(string.Format(AppString.NoAccountExistsWithID, memberId));
                }
                else if (!userAccount.HasBeenApproved)
                {
                    SetErrorMessages(AppString.AccountPendingApprovalMessage);
                }
                else if (waiverInfo == null || System.DateTimeOffset.Now.Subtract(waiverInfo.SignedOn).TotalDays >= 90)
                {
                    return RedirectToAction(actionName: nameof(Waiver));
                }
                else
                {
                    await _CheckInService.PerformCheckInAsync(memberId);
                    return RedirectToAction(
                            actionName: nameof(Successful),
                            routeValues: memberId);
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult WaiverNotSigned()
        {
            return View();
        }

        [HttpGet]
        public IActionResult WaiverSignedButCertificationExpired()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Successful()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Waiver()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccountSuspended()
        {
            return View();
        }
    }
}