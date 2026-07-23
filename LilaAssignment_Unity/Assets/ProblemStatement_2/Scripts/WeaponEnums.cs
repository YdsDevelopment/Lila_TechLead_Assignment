namespace TicTacToe.WeaponSystem
{
    public enum WeaponSlot
    {
        Primary1,
        Primary2,
        Secondary
    }

    public enum WeaponType
    {
        AssaultRifle,
        Shotgun,
        SMG,
        SniperRifle,
        Pistol,
        Revolver
    }

    public enum WeaponState
    {
        Idle,
        Firing,
        Reloading,
        Empty,
        Switching
    }

    public enum FireMode
    {
        Single,
        Burst,
        Auto
    }
}
