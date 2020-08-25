﻿using System.Collections.Generic;
using System.Web.Mvc;

namespace Pnprpg.DomainService.Models
{
    public class BranchEditModel : IBaseEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        [AllowHtml]
        public string Description { get; set; }

        public ICollection<int> Bonuses { get; set; }
    }
}