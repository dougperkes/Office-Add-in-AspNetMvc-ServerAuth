using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using OfficeAddInServerAuth.Helpers;

namespace OfficeAddInServerAuth.Models
{
    public class DropBoxSpaceUsage
    {
        public DropBoxSpaceUsage()
        {
            allocation = new Allocation();
        }

        public long used { get; set; }
        public Allocation allocation { get; set; }

        public long remaining => allocation.allocated - used;

        public class Allocation
        {
            [JsonProperty(".tag")]
            public string tag { get; set; }
            public long allocated { get; set; }
            public long used { get; set; }
        }
    }

    
}