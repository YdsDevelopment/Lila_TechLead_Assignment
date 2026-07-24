using System;
using System.Collections.Generic;

namespace LilaTest.WeaponSystem
{
    public sealed class PlayerWeaponController
    {
        private readonly Dictionary<WeaponSlot, Weapon> _equipped = new();
        private WeaponSlot _activeSlot = WeaponSlot.Primary1;
        private WeaponState _state = WeaponState.Idle;

        public Weapon ActiveWeapon => _equipped.GetValueOrDefault(_activeSlot);
        public WeaponSlot ActiveSlot => _activeSlot;
        public WeaponState State => _state;
        public IReadOnlyDictionary<WeaponSlot, Weapon> Equipped => _equipped;

        public event Action<WeaponSlot, Weapon> OnWeaponEquipped;
        public event Action<WeaponSlot, Weapon> OnWeaponUnequipped;
        public event Action<WeaponSlot, Weapon> OnWeaponSwitched;
        public event Action<Weapon> OnWeaponFired;
        public event Action<Weapon> OnReloadStarted;
        public event Action<Weapon> OnReloadCompleted;
        public event Action<Weapon> OnEmpty;
        public event Action<Weapon, int, int> OnAmmoChanged;

        public bool EquipWeapon(WeaponSlot slot, Weapon weapon)
        {
            if (_equipped.ContainsKey(slot))
                UnequipWeapon(slot);

            _equipped[slot] = weapon;
            weapon.State = WeaponState.Idle;
            weapon.OnFired += HandleWeaponFired;
            weapon.OnReloadStarted += HandleReloadStarted;
            weapon.OnReloadCompleted += HandleReloadCompleted;
            weapon.OnEmpty += HandleEmpty;
            weapon.OnAmmoChanged += HandleAmmoChanged;

            OnWeaponEquipped?.Invoke(slot, weapon);
            return true;
        }

        public void UnequipWeapon(WeaponSlot slot)
        {
            if (!_equipped.TryGetValue(slot, out var weapon))
                return;

            weapon.OnFired -= HandleWeaponFired;
            weapon.OnReloadStarted -= HandleReloadStarted;
            weapon.OnReloadCompleted -= HandleReloadCompleted;
            weapon.OnEmpty -= HandleEmpty;
            weapon.OnAmmoChanged -= HandleAmmoChanged;

            _equipped.Remove(slot);
            OnWeaponUnequipped?.Invoke(slot, weapon);

            if (_activeSlot == slot)
                SwitchToFirstAvailable();
        }

        public bool SwitchWeapon(WeaponSlot slot)
        {
            if (!_equipped.ContainsKey(slot) || _activeSlot == slot)
                return false;

            if (_state == WeaponState.Reloading || _state == WeaponState.Switching)
                return false;

            _activeSlot = slot;
            _state = WeaponState.Idle;

            OnWeaponSwitched?.Invoke(slot, ActiveWeapon);
            return true;
        }

        public bool Fire(float currentTime)
        {
            var weapon = ActiveWeapon;
            if (weapon == null || _state == WeaponState.Reloading || _state == WeaponState.Switching)
                return false;

            return weapon.Fire(currentTime);
        }

        public bool Reload(float currentTime)
        {
            var weapon = ActiveWeapon;
            if (weapon == null)
                return false;

            if (!weapon.CanReload())
                return false;

            _state = WeaponState.Reloading;
            weapon.StartReload(currentTime);
            return true;
        }

        public void Tick(float deltaTime, float currentTime)
        {
            if (_state != WeaponState.Reloading)
                return;

            var weapon = ActiveWeapon;
            if (weapon == null)
                return;

            weapon.UpdateReload(currentTime);
            if (weapon.State != WeaponState.Reloading)
                _state = WeaponState.Idle;
        }

        private void SwitchToFirstAvailable()
        {
            foreach (var kvp in _equipped)
            {
                _activeSlot = kvp.Key;
                OnWeaponSwitched?.Invoke(kvp.Key, kvp.Value);
                return;
            }
            _activeSlot = WeaponSlot.Primary1;
        }

        private void HandleWeaponFired(Weapon weapon)
        {
            OnWeaponFired?.Invoke(weapon);
        }

        private void HandleReloadStarted(Weapon weapon)
        {
            OnReloadStarted?.Invoke(weapon);
        }

        private void HandleReloadCompleted(Weapon weapon)
        {
            _state = WeaponState.Idle;
            OnReloadCompleted?.Invoke(weapon);
        }

        private void HandleEmpty(Weapon weapon)
        {
            OnEmpty?.Invoke(weapon);
        }

        private void HandleAmmoChanged(Weapon weapon)
        {
            OnAmmoChanged?.Invoke(weapon, weapon.CurrentAmmo, weapon.TotalReserveAmmo);
        }
    }
}
