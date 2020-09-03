﻿using System.Collections.Generic;
using System.Linq;
using Pnprpg.DomainService.Models;

namespace Pnprpg.DomainService.IServices
{
    public interface ITraitService
    {
        IQueryable<TraitModel> GetAll();
        TraitGroup GetForHero(HeroModel hero);
        ServiceResponse<HeroModel> AssignTraitToHero(HeroModel hero, int traitId);
        ServiceResponse<HeroModel> ResetTraitsForHero(HeroModel hero);
    }
}
