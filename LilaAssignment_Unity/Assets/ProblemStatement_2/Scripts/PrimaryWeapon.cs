namespace TicTacToe.WeaponSystem
{
    public sealed class PrimaryWeapon : Weapon
    {
        public bool HasScope { get; private set; }
        public bool HasUnderBarrel { get; private set; }

        public PrimaryWeapon(string name, WeaponType type, FireMode mode,
                              int magazineSize, int totalAmmo, float fireRate,
                              float reloadTime, float damage, float range,
                              bool hasScope = false, bool hasUnderBarrel = false)
            : base(name, type, mode, magazineSize, totalAmmo, fireRate, reloadTime, damage, range)
        {
            HasScope = hasScope;
            HasUnderBarrel = hasUnderBarrel;
        }
    }
}
