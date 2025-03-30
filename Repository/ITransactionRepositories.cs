using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface ITransactionRepositories
    {
        public void AddTransaction(int idbooking, double total, DateTime currentDay);
        public void DeleteTransaction(int idBooking);
    }
}
