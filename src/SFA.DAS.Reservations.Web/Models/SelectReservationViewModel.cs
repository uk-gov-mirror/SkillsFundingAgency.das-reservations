﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectReservationViewModel
    {
        public string CohortReference { get; set; }
        public string TransferSenderId { get; set; }
        public Guid? SelectedReservationId { get; set; }
        public bool? CreateNew { get; set; }
        public IEnumerable<AvailableReservationViewModel> AvailableReservations { get; set; }
    }
}