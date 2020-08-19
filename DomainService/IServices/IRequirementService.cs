﻿using System.Collections.Generic;
using Pnprpg.DomainService.Models;

namespace Pnprpg.DomainService.IServices
{
    public interface IRequirementService
    {
        List<Selectable> GetVariants(AbilityRequirement requirement);
        List<Selectable> GetVariants(PerkRequirement requirement);
        List<Selectable> GetVariants(RaceRequirement requirement);
    }
}