﻿using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Pnprpg.DomainService.Entities;
using Pnprpg.DomainService.Enums;
using Pnprpg.DomainService.IRepositories;
using Pnprpg.DomainService.IServices;
using Pnprpg.DomainService.Models;

namespace Pnprpg.Domain.Services
{
    public class BonusService : BaseService, IBonusService
    {
        private readonly IBonusRepository _bonusRepository;

        public BonusService(IMapper mapper, IBonusRepository bonusRepository) : base(mapper)
        {
            _bonusRepository = bonusRepository;
        }

        public BonusEditModel GetForEdit(int? id)
        {
            var bonus = id == null ? new Bonus() : _bonusRepository.Get(id.Value);
            return Mapper.Map<BonusEditModel>(bonus);
        }

        public void Delete(int id) => _bonusRepository.Delete(id);

        public IQueryable<BonusViewModel> GetAll(int? filter = null)
        {
            var query = _bonusRepository.Select().ProjectTo<BonusViewModel>(MapperConfig);
            return filter is null ? query : query.Where(x => (int)x.Type == filter);
        }

        public void Save(BonusEditModel model)
        {
            var bonus = new Bonus
            {
                Id = model.Id,
                Description = model.Description,
                Type = model.Type,
                Name = model.Name,
                Icon = model.Icon
            };

            _bonusRepository.InsertOrUpdate(bonus);
        }

        public void BatchSave(IQueryable<BaseBonusJoin> list, int parentId, BonusType parentType)
        {
            _bonusRepository.ClearBonuses(parentId, parentType);
            if (list == null || !list.Any())
                return;

            _bonusRepository.BatchInsertBonuses(list);
        }

        public void BatchClear(int parentId, BonusType type)
        {
            _bonusRepository.ClearBonuses(parentId, type);
        }
    }
}
