using HockeyManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.ViewModel
{
    public class PoolsViewModel
    {
        public IEnumerable<PoolList> poolList { get; set; }
        public Pool Pool { get; set; }
    }
}
