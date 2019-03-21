using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/reservations", Name="Search")]
    public class EmployerReservationsController : Controller
    {
        // GET
        public IActionResult Index(string q = null, List<int> selectedLevels = null)
        {
            var model = new SearchCriteria
            {
                Keywords = q,
                SelectedLevels = selectedLevels,
                SearchRouteName = "Search"
            };
            
            return View(model);
        }
    }


    public class SearchCriteria
    {
        public string Keywords { get; set; }
        public List<int> SelectedLevels { get; set; }
        public string SearchRouteName { get; set; }
    }
}