﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models
{
    public class MemberBindingModel
    {
        [Required]
        public string UserEmail { get; set; }
    }
}