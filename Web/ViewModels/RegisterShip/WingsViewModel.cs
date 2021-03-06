using System.Collections.Generic;
using System.Linq;
using Data.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.RegisterShip
{
    public class WingsViewModel : FullShipViewModel
    {
        public IEnumerable<Wing> AvailableWings { get; set; }
        public IEnumerable<Weapon> AvailableWeapons { get; set; }
        
        public IEnumerable<SelectListItem> GetSelectableAvailableWings()
        {
            return AvailableWings.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = $"{w.Name} -  {w.Agility},  {w.Speed},  {w.Energy},  {w.Weight},  {w.NumberOfHardpoints}"
            });
        }

        public IEnumerable<SelectListItem> GetSelectableAvailableWeapons()
        {
            return AvailableWeapons.Select(w => new SelectListItem
                {Value = w.Id.ToString(), Text = $"{w.Name} - {w.DamageType},  {w.EnergyDrain},  {w.Weight}"});
        }
    }
}