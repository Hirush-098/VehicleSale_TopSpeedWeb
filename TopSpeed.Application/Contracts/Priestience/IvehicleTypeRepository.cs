using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopSpeed.Domain.Models;

namespace TopSpeed.Application.Contracts.Priestience
{
    public interface IvehicleTypeRepository : IGenericRepository<VehicleType>
    {
        Task Update(VehicleType vehicleType);

    }
}
