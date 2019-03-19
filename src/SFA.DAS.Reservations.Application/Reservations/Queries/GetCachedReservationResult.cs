﻿using System;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationResult
    {
        public Guid Id { get; set; }
        public string AccountPublicHashedId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string StartDate { get; set; }
        public string StartDateDescription { get; set; }
        public string AccountId { get; set; }
        public string CourseId { get; set; }
        public string CourseDescription { get; set; }
    }
}