namespace TicTacToe.WeaponSystem
{
    public sealed class SecondaryWeapon : Weapon
    {
        public bool IsSidearm { get; private set; }

        public SecondaryWeapon(string name, WeaponType type, FireMode mode,
                                int magazineSize, int totalAmmo, float fireRate,
                                float reloadTime, float damage, float range,
                                bool isSidearm = true)
            : base(name, type, mode, magazineSize, totalAmmo, fireRate, reloadTime, damage, range)
        {
            IsSidearm = isSidearm;
        }
    }
}
