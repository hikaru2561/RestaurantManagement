﻿using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{

    public enum TableStatus
    {
        Available = 0,
        Reserved = 1,
        InUse = 2
    }
    public class DingningTable
    {
        
        public int DingningTableId { get; set; }

        public string Name { get; set; }

        public bool IsVIP { get; set; }

        public TableStatus Status { get; set; }

        public int Capacity { get; set; }

        public ICollection<Reservation>? Reservations { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}