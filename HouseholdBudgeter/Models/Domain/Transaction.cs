using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.Domain
{
    public class Transaction
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
  
        public bool Void { get; set; }

        public DateTime TransactionDate { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual BankAccount BankAccount { get; set; }
        public int BankAccountId { get; set; }

        public virtual Category Category { get; set; }
        public int CategoryId { get; set; }

        public virtual ApplicationUser Creator { get; set; }
        public string  CreatorId { get; set; }

        public Transaction()
        {
            DateCreated = DateTime.Now;
        }
    }
}