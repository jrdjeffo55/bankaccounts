using System;
using System.Collections.Generic;

namespace BankAccounts.Models
{
    public class Account : BaseEntity
    {
        public int AccountId { get; set; }
        public double Balance { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<Transaction> Transactions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public Account()
        {
            Transactions = new List<Transaction>();
        }
    }
}