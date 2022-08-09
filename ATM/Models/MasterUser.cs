using System;
using System.Collections.Generic;
using System.Text;

namespace ATM.Models
{
    public class MasterUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int Pin { get; set; }
        public int Balance { get; set; }
    }
}