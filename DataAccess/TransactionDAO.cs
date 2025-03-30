using BussinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TransactionDAO
    {
        private readonly BookingVillaPrnContext _context;

        public TransactionDAO(BookingVillaPrnContext context)
        {
            _context = context;
        }

        public void AddTransaction(int idbooking, double total, DateTime currentDay)
        {
            var transaction = new Transaction
            {
                IdTransactions = idbooking,
                Price = total,
                Date = currentDay
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();
        }

        public void DeleteTransaction(int idBooking)
        {
            var transaction = _context.Transactions.FirstOrDefault(t => t.IdTransactions == idBooking);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                _context.SaveChanges();
            }
        }
    }
}
