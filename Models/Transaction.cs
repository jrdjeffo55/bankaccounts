using System;
namespace BankAccounts.Models
{
    public class Transaction : BaseEntity
    {
        public int TransactionId { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}