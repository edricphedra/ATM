using System;
using System.Collections.Generic;
using System.Text;

namespace ATM.Models
{
    public class TransactionATM
    {
        public int TrxId { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public int LastStatment { get; set; }
        public int PostStatment { get; set; }
    }
}
