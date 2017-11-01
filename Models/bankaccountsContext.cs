using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Models
{
    public class bankaccountsContext : DbContext
    {
        public bankaccountsContext(DbContextOptions<bankaccountsContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users {get; set;}
        public DbSet<Account> Accounts {get; set;}
        public DbSet<Transaction> Transactions {get; set;}
        
    }
}