﻿using System.Linq;
using Pnprpg.DomainService.Entities;

namespace Pnprpg.DomainService.IRepositories
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        IQueryable<T> Get(int id);
        IQueryable<T> GetRandom();
        IQueryable<T> Select();
        int InsertOrUpdate(T entity);
        void Delete(T entity);
        void Delete(int id);
        void BatchInsert(IQueryable<T> list);
        void BatchDelete(IQueryable<T> list);
    }
}
