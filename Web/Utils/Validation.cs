using System;
using System.Collections.Generic;
using System.Linq;
using Data.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Web.Utils.Interfaces;

namespace Web.Utils
{
    public static class Validation
    {
        private static ModelStateDictionary _modelState;
        private static Ship _ship;
        private static IEnumerable<Weapon> _weapons;
        private static ICalculations _calculations;

        public static void ValidateShip(ModelStateDictionary modelState, Ship ship, ICalculations calculations, double maximumTakeoffMass, Licence licence)
        {
            _modelState = modelState;
            _ship = ship;
            _calculations = calculations;
            _weapons = _ship.Wings.SelectMany(wing => wing.Hardpoint);
            ValidateNumberOfWings();
            ValidateNumberOfWeapons();
            ValidateHullWeight(maximumTakeoffMass);
            ValidateEnergyConsumption();
            ValidateImploderWeapons();
            ValidateCombinationWeapons();
            ValidateNullifierWeapon();
            ValidateKineticWeapons();
            ValidateMaxLicenseWeight(licence);
            ValidateAtLeastOneWeaponPerWing();
        }

        private static void ValidateNumberOfWings()
        {
            if (_ship.Wings.Count % 2 != 0)
                _modelState.AddModelError("OddWings", "The amount of wings on a ship must be even");
        }

        private static void ValidateNumberOfWeapons()
        {
            foreach (var wing in _ship.Wings.Where(wing => wing.Hardpoint.Count > wing.NumberOfHardpoints))
                _modelState.AddModelError("WeaponOverload", "There are too many weapons on " + wing.Name);
        }

        private static void ValidateHullWeight(double maximumTakeoffMass)
        {
            if (_calculations.GetShipWeight(_ship) > maximumTakeoffMass)
                _modelState.AddModelError("CapacityOverload", "The ship is too heavy to take off");
        }

        private static void ValidateEnergyConsumption()
        {
            if (_calculations.GetEnergyConsumption(_ship) > _ship.Energy)
                _modelState.AddModelError("EnergyConsumptionOverdraft",
                    "The energy consumption of the ship is too high");
        }

        private static void ValidateImploderWeapons()
        {
            if (_ship.Engine.Id == 2 && _weapons.Any(weapon => weapon.Id == 9))
                _modelState.AddModelError("ImplosionDanger",
                    "The combination of Imploder weapon and Intrepid Class engine is not allowed");
        }

        private static void ValidateCombinationWeapons()
        {
            if (_weapons.Any(weapon => weapon.DamageType == DamageTypeEnum.Heat) &&
                _weapons.Any(weapon => weapon.DamageType == DamageTypeEnum.Cold))
                _modelState.AddModelError("HeatStress", "The combination of heat and cold weapons is not allowed");
            if (_weapons.Any(weapon => weapon.DamageType == DamageTypeEnum.Statis) &&
                _weapons.Any(weapon => weapon.DamageType == DamageTypeEnum.Gravity))
                _modelState.AddModelError("ForceStress",
                    "The combination of statis and gravity weapons is not allowed");
        }

        private static void ValidateNullifierWeapon()
        {
            foreach (var wing in _ship.Wings.Where(wing =>
                wing.Hardpoint.Any(weapon => weapon.Id == 14) && wing.Hardpoint.Count < 2))
                _modelState.AddModelError("LoneNullifier", "The Nullifier can't be the only weapon on " + wing.Name);
        }

        private static void ValidateKineticWeapons()
        {
            var kineticWings = _ship.Wings.Where(wing =>
                wing.Hardpoint.Any(weapon => weapon.DamageType == DamageTypeEnum.Kinetic));

            var energyPerWing = kineticWings.Select(wing => wing.Hardpoint.Where(weapon => weapon.DamageType == DamageTypeEnum.Kinetic).Sum(weapon => weapon.EnergyDrain)).ToList();

            if (energyPerWing.Count <= 0)
                return;

            if ((energyPerWing.Max() - energyPerWing.Min() >= 35) || (energyPerWing.Count == 1 && energyPerWing[0] >= 35))
                _modelState.AddModelError("KineticDifference",
                    "The energy drain of kinetic weapons on different wings cannot be more than 35");
        }

        private static void ValidateMaxLicenseWeight(Licence licence)
        {
            var maxWeight = (int) licence;
            if (maxWeight == -1)
                return;
            
            if (_calculations.GetShipWeight(_ship) > maxWeight)
                _modelState.AddModelError("ToHeavyForLicense", "The ship is too heavy for your license");
        }

        private static void ValidateAtLeastOneWeaponPerWing()
        {
            var hasWingWithoutWeapon = _ship.Wings.Any(wing => wing.Hardpoint.Count < 1);

            if (hasWingWithoutWeapon)
                _modelState.AddModelError("AtLeastOneWeaponPerWing", "Please select at least one weapon per wing.");
        }
    }
}