using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.Domain
{
    public class HouseHold
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual ApplicationUser Owner { get; set; }
        public string OwnerId { get; set; }

        public virtual List<ApplicationUser> Members { get; set; }

        public virtual List<ApplicationUser> InvitedUsers { get; set; }

        public virtual List<Category> Categories { get; set; }

        public HouseHold()
        {
            DateCreated = DateTime.Now;
            Members = new List<ApplicationUser>();
            InvitedUsers = new List<ApplicationUser>();
            Categories = new List<Category>();
        }
    }
}