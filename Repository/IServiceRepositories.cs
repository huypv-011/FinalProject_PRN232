using BussinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IServiceRepositories
    {
        List<Service> GetAllService();
        void AddService(Service service);
        Service GetServiceById(int idService);
        List<Service> GetAllServiceByName();
        List<Service> GetAllServiceByPrice();
        List<Service> GetServiceByName(string name);
        List<Service> GetServiceByPrice(double price);
        int EditService(Service service);
        void DeleteService(int idService);
    }
}
