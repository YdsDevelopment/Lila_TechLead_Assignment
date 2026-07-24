using System.Collections.Generic;

namespace LilaTest.WeaponSystem
{
    public sealed class WeaponHUD
    {
        public sealed class SlotDisplay
        {
            public WeaponSlot Slot { get; }
            public string WeaponName { get; private set; }
            public int CurrentAmmo { get; private set; }
            public int MaxAmmo { get; private set; }
            public int ReserveAmmo { get; private set; }
            public bool IsActive { get; private set; }
            public bool IsReloading { get; private set; }
            public bool IsEmpty { get; private set; }

            public SlotDisplay(WeaponSlot slot)
            {
                Slot = slot;
            }

            public void Update(Weapon weapon, bool isActive, bool isReloading)
            {
                WeaponName = weapon?.Name ?? "Empty";
                CurrentAmmo = weapon?.CurrentAmmo ?? 0;
                MaxAmmo = weapon?.MagazineSize ?? 0;
                ReserveAmmo = weapon?.TotalReserveAmmo ?? 0;
                IsActive = isActive;
                IsReloading = isReloading;
                IsEmpty = weapon != null && weapon.CurrentAmmo <= 0;
            }
        }

        private readonly Dictionary<WeaponSlot, SlotDisplay> _slots = new()
        {
            { WeaponSlot.Primary1, new SlotDisplay(WeaponSlot.Primary1) },
            { WeaponSlot.Primary2, new SlotDisplay(WeaponSlot.Primary2) },
            { WeaponSlot.Secondary, new SlotDisplay(WeaponSlot.Secondary) }
        };

        public IReadOnlyDictionary<WeaponSlot, SlotDisplay> Slots => _slots;
        public SlotDisplay ActiveSlot { get; private set; }

        public event System.Action OnHUDUpdated;

        public void Refresh(PlayerWeaponController controller)
        {
            foreach (var kvp in _slots)
            {
                var slot = kvp.Key;
                var display = kvp.Value;
                var weapon = controller.Equipped.GetValueOrDefault(slot);
                bool isActive = controller.ActiveSlot == slot;
                bool isReloading = controller.State == WeaponState.Reloading && isActive;
                display.Update(weapon, isActive, isReloading);

                if (isActive)
                    ActiveSlot = display;
            }

            OnHUDUpdated?.Invoke();
        }
    }
}
