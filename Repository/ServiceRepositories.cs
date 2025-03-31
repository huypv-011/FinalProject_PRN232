using BussinessObject;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ServiceRepositories : IServiceRepositories
    {
        private readonly ServiceDAO _serviceDAO;

        public ServiceRepositories(BookingVillaPrnContext context)
        {
            _serviceDAO = new ServiceDAO(context);
        }
        public List<Service> GetAllService() => _serviceDAO.GetAllService();
        public void AddService(Service service) => _serviceDAO.AddService(service);
        public Service GetServiceById(int idService) => _serviceDAO.GetServiceById(idService);
        public List<Service> GetAllServiceByName() => _serviceDAO.GetAllServiceByName();
        public List<Service> GetAllServiceByPrice() => _serviceDAO.GetAllServiceByPrice();
        public List<Service> GetServiceByName(string name) => _serviceDAO.GetServiceByName(name);
        public List<Service> GetServiceByPrice(double price) => _serviceDAO.GetServiceByPrice(price);
        public int EditService(Service service) => _serviceDAO.EditService(service);
        public void DeleteService(int idService) => _serviceDAO.DeleteService(idService);

        public List<Service> GetAllServicePagination(int index) => _serviceDAO.GetAllServicePagination(index);

        public int GetNumberTotalService() => _serviceDAO.GetNumberTotalService();

        public int GetNumberService() => _serviceDAO.GetNumberService();
    }
}
