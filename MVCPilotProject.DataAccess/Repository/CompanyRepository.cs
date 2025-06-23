using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVCPilotProject.DataAccess.Data;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;

namespace MVCPilotProject.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company company)
        {
            _db.Companies.Update(company);
        }
    }
}
