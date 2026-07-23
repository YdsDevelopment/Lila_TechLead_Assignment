using System;

namespace TicTacToe.WeaponSystem
{
    public class Weapon
    {
        public string Name { get; protected set; }
        public WeaponType Type { get; protected set; }
        public WeaponState State { get; internal set; }
        public FireMode Mode { get; protected set; }

        public int MagazineSize { get; protected set; }
        public int CurrentAmmo { get; protected set; }
        public int TotalReserveAmmo { get; protected set; }

        public float FireRate { get; protected set; }
        public float ReloadTime { get; protected set; }
        public float Damage { get; protected set; }
        public float Range { get; protected set; }

        private float _lastFireTime;
        private float _reloadStartTime;

        public event Action<Weapon> OnFired;
        public event Action<Weapon> OnReloadStarted;
        public event Action<Weapon> OnReloadCompleted;
        public event Action<Weapon> OnAmmoChanged;
        public event Action<Weapon> OnEmpty;

        public Weapon(string name, WeaponType type, FireMode mode, int magazineSize, int totalAmmo,
                       float fireRate, float reloadTime, float damage, float range)
        {
            Name = name;
            Type = type;
            Mode = mode;
            MagazineSize = magazineSize;
            TotalReserveAmmo = totalAmmo;
            CurrentAmmo = magazineSize;
            FireRate = fireRate;
            ReloadTime = reloadTime;
            Damage = damage;
            Range = range;
            State = WeaponState.Idle;
        }

        public bool CanFire(float currentTime)
        {
            if (State == WeaponState.Reloading || State == WeaponState.Switching)
                return false;

            if (CurrentAmmo <= 0)
                return false;

            float interval = 60f / FireRate;
            return (currentTime - _lastFireTime) >= interval;
        }

        public bool Fire(float currentTime)
        {
            if (!CanFire(currentTime))
                return false;

            CurrentAmmo--;
            _lastFireTime = currentTime;
            State = CurrentAmmo > 0 ? WeaponState.Firing : WeaponState.Empty;

            OnFired?.Invoke(this);
            OnAmmoChanged?.Invoke(this);

            if (CurrentAmmo <= 0)
                OnEmpty?.Invoke(this);

            return true;
        }

        public bool CanReload()
        {
            if (State == WeaponState.Reloading || State == WeaponState.Switching)
                return false;

            if (CurrentAmmo >= MagazineSize)
                return false;

            if (TotalReserveAmmo <= 0)
                return false;

            return true;
        }

        public void StartReload(float currentTime)
        {
            if (!CanReload())
                return;

            State = WeaponState.Reloading;
            _reloadStartTime = currentTime;
            OnReloadStarted?.Invoke(this);
        }

        public void UpdateReload(float currentTime)
        {
            if (State != WeaponState.Reloading)
                return;

            if ((currentTime - _reloadStartTime) >= ReloadTime)
                CompleteReload();
        }

        private void CompleteReload()
        {
            int needed = MagazineSize - CurrentAmmo;
            int available = Math.Min(needed, TotalReserveAmmo);
            CurrentAmmo += available;
            TotalReserveAmmo -= available;
            State = WeaponState.Idle;

            OnReloadCompleted?.Invoke(this);
            OnAmmoChanged?.Invoke(this);
        }

        public void AddAmmo(int amount)
        {
            TotalReserveAmmo += amount;
            OnAmmoChanged?.Invoke(this);
        }

        public float GetAmmoRatio()
        {
            return (float)CurrentAmmo / MagazineSize;
        }

        public bool IsFull()
        {
            return CurrentAmmo >= MagazineSize;
        }
    }
}
