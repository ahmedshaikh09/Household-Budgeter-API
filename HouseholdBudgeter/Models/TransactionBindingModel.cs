using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class TransactionBindingModel
    {   
        [Required]
        public string Title { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}