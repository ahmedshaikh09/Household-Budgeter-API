using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.Domain
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual HouseHold HouseHold { get; set; }
        public int HouseHoldId { get; set; }

        public Category()
        {
            DateCreated = DateTime.Now;
        }
    }
}