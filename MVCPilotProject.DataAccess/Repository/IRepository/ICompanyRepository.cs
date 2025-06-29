﻿using MVCPilotProject.Models;

namespace MVCPilotProject.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company company);

    }
}
