using BussinessObject;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class TransactionRepositories : ITransactionRepositories
    {
        private readonly TransactionDAO _transactionDAO;

        public TransactionRepositories(BookingVillaPrnContext context)
        {
            _transactionDAO = new TransactionDAO(context);
        }
        public void AddTransaction(int idbooking, double total, DateTime currentDay) => _transactionDAO.AddTransaction(idbooking, total, currentDay);

        public void DeleteTransaction(int idBooking) => _transactionDAO.DeleteTransaction(idBooking);
    }
}
