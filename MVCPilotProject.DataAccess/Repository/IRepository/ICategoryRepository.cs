﻿using MVCPilotProject.Models;

namespace MVCPilotProject.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository: IRepository<Category>
    {
        void Update(Category category);
    }
}
