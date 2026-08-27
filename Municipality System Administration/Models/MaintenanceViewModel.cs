using System;
using System.Collections.Generic;

namespace Municipality_System_Administration.Models
{
    public class MaintenanceViewModel
    {
        public int AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public int? MaintenanceFrequency { get; set; }
        public string MaintenanceNotes { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class MaintenanceHistoryItem
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }
}