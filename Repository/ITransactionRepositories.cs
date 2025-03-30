using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObject;

namespace Repository
{
    public interface ITransactionRepositories
    {
        public void AddTransaction(Transaction transaction);
        public void DeleteTransaction(int idBooking);
        public MonthRevenue GetRevenue(int year);
        public List<int> GetYears();
    }
}
