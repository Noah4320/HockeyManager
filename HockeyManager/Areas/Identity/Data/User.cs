﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Models;
using Microsoft.AspNetCore.Identity;

namespace HockeyManager.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the User class
    public class User : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string FirstName { get; set; }
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string LastName { get; set; }

        public virtual ICollection<Favourites> Favourites { get; set; }
        public virtual ICollection<PoolList> PoolsOwned { get; set; }
        public virtual ICollection<Season> Seasons { get; set; }
    }
}
