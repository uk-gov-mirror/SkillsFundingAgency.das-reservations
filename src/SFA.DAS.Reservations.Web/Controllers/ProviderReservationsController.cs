﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    [Route("{ukprn}/reservations")]
    public class ProviderReservationsController : Controller
    {
        public IActionResult Index()
        {
            
            return View(RouteData.Values["ukprn"]);
        }
    }
}