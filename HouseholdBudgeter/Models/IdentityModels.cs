﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using HouseholdBudgeter.Models.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace HouseholdBudgeter.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public virtual List<HouseHold> HouseHolds { get; set; }

        [InverseProperty(nameof(HouseHold.Owner))]
        public virtual List<HouseHold> OwnedHouseHolds { get; set; }

        [InverseProperty(nameof(HouseHold.InvitedUsers))]
        public virtual List<HouseHold> InvitedUsers { get; set; }

        [InverseProperty(nameof(HouseHold.Members))]
        public virtual List<HouseHold> Members { get; set; }

        public ApplicationUser()
        {
            HouseHolds = new List<HouseHold>();
            OwnedHouseHolds = new List<HouseHold>();
            InvitedUsers = new List<HouseHold>();          
            Members = new List<HouseHold>();          
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public DbSet<HouseHold> HouseHolds { get; set; }
        public DbSet<Category> Categories { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}