﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    [Route("{ukPrn}/reservations", Name = RouteNames.ProviderIndex)]
    public class ProviderReservationsController : ReservationsBaseController
    {
        private readonly IMediator _mediator;
        private readonly IExternalUrlHelper _externalUrlHelper;
        private readonly IEncodingService _encodingService;

        public ProviderReservationsController(IMediator mediator, IExternalUrlHelper externalUrlHelper, IEncodingService encodingService) : base(mediator)
        {
            _mediator = mediator;
            _externalUrlHelper = externalUrlHelper;
            _encodingService = encodingService;
        }

        public async Task<IActionResult> Index(ReservationsRouteModel routeModel)
        {
            var backLink = routeModel.IsFromManage.HasValue && routeModel.IsFromManage.Value
                ? Url.RouteUrl(RouteNames.ProviderManage,routeModel)
                : _externalUrlHelper.GenerateDashboardUrl();

            var viewResult = await CheckNextGlobalRule(RouteNames.ProviderStart, ProviderClaims.ProviderUkprn, backLink, RouteNames.ProviderSaveRuleNotificationChoiceNoReservation);
            
            if (viewResult == null)
            {
                return RedirectToRoute(RouteNames.ProviderStart, routeModel);
            }

            return viewResult;
        }

        [Route("start", Name = RouteNames.ProviderStart)]
        public async Task<IActionResult> Start(uint ukPrn, bool isFromManage)
        {
            var response = await _mediator.Send(new GetFundingRulesQuery());

            if (response?.ActiveGlobalRules != null && response.ActiveGlobalRules.Any())
            {
                var backLink = isFromManage
                    ? Url.RouteUrl(RouteNames.ProviderManage, new {ukPrn, isFromManage})
                    : _externalUrlHelper.GenerateDashboardUrl(); 
                return View( "ProviderFundingPaused", backLink);
            }

            var employers = (await _mediator.Send(new GetTrustedEmployersQuery { UkPrn = ukPrn })).Employers.ToList();

            if (!employers.Any())
            {
                return View("NoPermissions");
            }
            
            var viewModel = new ProviderStartViewModel
            {
                IsFromManage = isFromManage
            };
            return View("Index", viewModel);
        }

        [HttpGet]
        [Route("choose-employer", Name = RouteNames.ProviderChooseEmployer)]
        public async Task<IActionResult> ChooseEmployer(ReservationsRouteModel routeModel)
        {
            if (!routeModel.UkPrn.HasValue)
            {
                throw new ArgumentException("UkPrn must be set", nameof(ReservationsRouteModel.UkPrn));
            }

            var getTrustedEmployersResponse = await _mediator.Send(new GetTrustedEmployersQuery {UkPrn = routeModel.UkPrn.Value});
            
            
            var eoiEmployers = new List<AccountLegalEntity>();
            foreach (var employer in getTrustedEmployersResponse.Employers)
            {
                employer.AccountLegalEntityPublicHashedId = _encodingService.Encode(employer.AccountLegalEntityId,
                    EncodingType.PublicAccountLegalEntityId);
                eoiEmployers.Add(employer);
            }

            var viewModel = new ChooseEmployerViewModel
            {
                Employers = eoiEmployers
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("confirm-employer/{id?}", Name=RouteNames.ProviderConfirmEmployer)]
        public async Task<IActionResult> ConfirmEmployer(ConfirmEmployerViewModel viewModel)
        {

            if (viewModel.Id.HasValue)
            {
                var result = await _mediator.Send(new GetCachedReservationQuery
                {
                    Id = viewModel.Id.Value,
                    UkPrn = viewModel.UkPrn
                });

                viewModel.AccountLegalEntityName = result.AccountLegalEntityName;
                viewModel.AccountPublicHashedId = _encodingService.Encode(result.AccountId, EncodingType.AccountId);
                viewModel.AccountLegalEntityPublicHashedId = result.AccountLegalEntityPublicHashedId;
                viewModel.AccountName = result.AccountName;
                return View(viewModel);

            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("confirm-employer/{id?}", Name = RouteNames.ProviderConfirmEmployer)]
        public async Task<IActionResult> ProcessConfirmEmployer(ConfirmEmployerViewModel viewModel)
        {
            if (!viewModel.Confirm.HasValue)
            {
                ModelState.AddModelError("confirm-yes", "Select whether to secure funds for this employer or not");
                return View("ConfirmEmployer", viewModel);
            }

            try
            {
                if (!viewModel.Confirm.Value)
                {
                    return RedirectToRoute(RouteNames.ProviderChooseEmployer, new
                    {
                        viewModel.UkPrn
                    });
                }

                var reservationId = Guid.NewGuid();

                await _mediator.Send(new CacheReservationEmployerCommand
                {
                    Id = reservationId,
                    AccountId = _encodingService.Decode(viewModel.AccountPublicHashedId, EncodingType.PublicAccountId),
                    AccountLegalEntityId = _encodingService.Decode(viewModel.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId),
                    AccountLegalEntityName = viewModel.AccountLegalEntityName,
                    AccountLegalEntityPublicHashedId = viewModel.AccountLegalEntityPublicHashedId,
                    UkPrn = viewModel.UkPrn,
                    AccountName = viewModel.AccountName
                });

                return RedirectToRoute(RouteNames.ProviderApprenticeshipTraining, new
                {
                    Id = reservationId,
                    EmployerAccountId = viewModel.AccountPublicHashedId,
                    viewModel.UkPrn
                });

            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                return View("ConfirmEmployer", viewModel);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached");
            }
        }

        [HttpGet]
        [Route("employer-agreement-not-signed/{id?}", Name=RouteNames.ProviderEmployerAgreementNotSigned)]
        public async Task<IActionResult> EmployerAgreementNotSigned(ReservationsRouteModel routeModel, string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                id = routeModel.AccountLegalEntityPublicHashedId;
            }
            
            var result = await _mediator.Send(new GetAccountLegalEntityQuery {AccountLegalEntityPublicHashedId = id});
            var viewModel = new EmployerAgreementNotSignedViewModel
            {
                AccountName = result.LegalEntity.AccountLegalEntityName, 
                DashboardUrl = _externalUrlHelper.GenerateDashboardUrl(null),
                BackUrl = routeModel.IsFromSelect.HasValue && routeModel.IsFromSelect.Value ? routeModel.PreviousPage : "",
                IsUrl = routeModel.IsFromSelect.HasValue && routeModel.IsFromSelect.Value
            };

            return View(viewModel);
        
        }
    }
}
