using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class ContractorPaymentInfo
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string ContractorId { get; set; }

        public Contractor Contractor { get; set; }

        [MaxLength(256)]
        public string PayPalEmail { get; set; }

        // You can add other payment methods here as needed
        // For example:
        // public string BankAccountNumber { get; set; }
        // public string StripeConnectId { get; set; }

        [MaxLength(50)]
        public string DefaultPaymentMethod { get; set; } = "PayPal";

        public bool IsPaymentInfoComplete => !string.IsNullOrEmpty(PayPalEmail);

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
