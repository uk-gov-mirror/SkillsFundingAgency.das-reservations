﻿using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class AuthorizationService
    {
        private const string ProviderDaa = "DAA";

        public static void AddAuthorizationService(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    PolicyNames
                        .HasEmployerAccount
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                        policy.Requirements.Add(new EmployerAccountRequirement());
                    });
                options.AddPolicy(
                    PolicyNames
                        .HasProviderAccount
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ProviderClaims.ProviderUkprn);
                        policy.RequireClaim(ProviderClaims.Service, ProviderDaa);
                        policy.Requirements.Add(new ProviderUkPrnRequirement());
                    });
                options.AddPolicy(
                    PolicyNames.HasProviderOrEmployerAccount, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireAssertion(context =>
                        {
                            var hasUkprn = context.User.HasClaim(claim =>
                                claim.Type.Equals(ProviderClaims.ProviderUkprn));
                            var hasDaa = context.User.HasClaim(claim =>
                                claim.Type.Equals(ProviderClaims.Service) &&
                                claim.Value.Equals(ProviderDaa));
                            var hasEmployerAccountId = context.User.HasClaim(claim =>
                                claim.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
                            return hasUkprn && hasDaa || hasEmployerAccountId;
                        });
                        policy.Requirements.Add(new HasProviderOrEmployerAccountRequirement());
                    });
                options.AddPolicy(
                    PolicyNames.HasEmployerViewerUserRole
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                        policy.Requirements.Add(new HasEmployerViewerUserRoleRequirement());
                    });
            });
        }
    }
}