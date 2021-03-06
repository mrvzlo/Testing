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
    public class WeaponService : BaseService, IWeaponService
    {
        private readonly IWeaponRepository _weaponRepository;
        private readonly IBonusService _bonusService;

        public WeaponService(IMapper mapper, IWeaponRepository weaponRepository, IBonusService bonusService) : base(mapper)
        {
            _weaponRepository = weaponRepository;
            _bonusService = bonusService;
        }

        public IQueryable<WeaponViewModel> GetAll(MajorType major, int? filter = null)
        {
            var query = _weaponRepository.Select(major).ProjectTo<WeaponViewModel>(MapperConfig);
            return filter is null ? query : query.Where(x => x.Skill.Id == filter);
        }

        public WeaponEditModel GetForEdit(int? id)
        {
            if (id == null)
                return new WeaponEditModel();
            var model = _weaponRepository.Get(id.Value).ProjectTo<WeaponEditModel>(MapperConfig).FirstOrDefault();
            return model ?? new WeaponEditModel();
        }

        public int Save(WeaponEditModel model)
        {
            var id = MappingSave(_weaponRepository, model);

            var bonuses = model.Bonuses?.Select(x => new WeaponBonus
            {
                WeaponId = id,
                BonusId = x
            }).AsQueryable();

            _bonusService.BatchSave(bonuses, id, BonusType.Weapon);
            return id;
        }

        public void Delete(int id)
        {
            _bonusService.BatchClear(id, BonusType.Weapon);
            _weaponRepository.Delete(id);
        }
    }
}
