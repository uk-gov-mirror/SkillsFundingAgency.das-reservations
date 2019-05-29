﻿namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public static class RouteNames
    {
        public const string ProviderIndex = "provider-index";
        public const string ProviderStart = "provider-start";
        public const string ProviderManage = "provider-manage";
        public const string ProviderManageCreate = "provider-manage-create";
        public const string ProviderApprenticeshipTraining = "provider-apprenticeship-training";
        public const string ProviderCreateApprenticeshipTraining = "provider-create-apprenticeship-training";
        public const string ProviderReview = "provider-review";
        public const string ProviderPostReview = "provider-create-reservation";
        public const string ProviderCompleted = "provider-reservation-created";
        public const string ProviderSignOut = "provider-signout";
        public const string ProviderPostCompleted = "provider-reservation-completed";
        public const string ProviderChooseEmployer = "provider-choose-employer";
        public const string ProviderConfirmEmployer = "provider-confirm-employer";

        public const string EmployerIndex = "employer-index";
        public const string EmployerStart = "employer-start";
        public const string EmployerManage = "employer-manage";
        public const string EmployerManageCreate = "employer-manage-create";
        public const string EmployerCreateApprenticeshipTraining = "employer-create-apprenticeship-training";
        public const string EmployerApprenticeshipTraining = "employer-apprenticeship-training";
        public const string EmployerReview = "employer-review";
        public const string EmployerPostReview = "employer-create-reservation";
        public const string EmployerCompleted = "employer-reservation-created";
        public const string EmployerPostCompleted = "employer-post-completed";
        public const string EmployerSignOut = "employer-signout";
        public const string EmployerSelectCourse = "employer-select-course";
        public const string EmployerSkipSelectCourse = "employer-skip-select-course";
        public const string EmployerSelectLegalEntity = "employer-select-legal-entity";
		public const string EmployerChangePassword = "employer-change-password";
        public const string EmployerChangeEmail = "employer-change-email";
    }
}
