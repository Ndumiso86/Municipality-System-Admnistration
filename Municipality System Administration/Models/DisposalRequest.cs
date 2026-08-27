using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Municipality_System_Administration.Models
{
    public class DisposalRequest
    {
        [Key]
        public int DisposalRequestId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public string RequestedByUserId { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        [Required]
        [Display(Name = "Reason for Disposal")]
        public string Reason { get; set; }

        // REMOVED: Staff no longer selects disposal method
        // public string DisposalMethod { get; set; } // REMOVED

        [Display(Name = "Status")]
        public string Status { get; set; } // Pending, Reviewed, Approved, Rejected, Completed

        [Display(Name = "Review Date")]
        public DateTime? ReviewedDate { get; set; }

        [Display(Name = "Reviewed By")]
        public string ReviewedByUserId { get; set; }

        [Display(Name = "Review Notes")]
        public string ReviewNotes { get; set; }

        [Display(Name = "Approval Date")]
        public DateTime? ApprovalDate { get; set; }

        [Display(Name = "Approved By")]
        public string ApprovedByUserId { get; set; }

        [Display(Name = "Approval Notes")]
        public string ApprovalNotes { get; set; }

        // NEW: Admin/Asset Manager selects disposal method
        [Display(Name = "Disposal Method")]
        public string DisposalMethod { get; set; } // Auction, Scrap, Donate, Recycle, Trade-in, Write-off

        [Display(Name = "Disposal Value")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? DisposalValue { get; set; }

        [Display(Name = "Completion Date")]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "Completion Notes")]
        public string CompletionNotes { get; set; }

        // Navigation properties
        [ForeignKey("AssetId")]
        public virtual Asset Asset { get; set; }

        [ForeignKey("RequestedByUserId")]
        public virtual ApplicationUser RequestedBy { get; set; }

        [ForeignKey("ReviewedByUserId")]
        public virtual ApplicationUser ReviewedBy { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public virtual ApplicationUser ApprovedBy { get; set; }
    }
}