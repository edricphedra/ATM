using System;
using System.Collections.Generic;
using System.Text;

namespace ATM.Models
{
    public class Owed
    {
        public int OwedId { get; set; }
        public int UserOrigin { get; set; }
        public int UserTarget { get; set; }
        public int Amount { get; set; }
    }
}