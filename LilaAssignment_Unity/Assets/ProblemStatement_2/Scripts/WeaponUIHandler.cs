using UnityEngine;
using UnityEngine.UI;

namespace LilaTest.WeaponSystem
{
    public sealed class WeaponUIHandler : MonoBehaviour
    {
        [SerializeField] private Text primaryName;
        [SerializeField] private Text primaryAmmo;
        [SerializeField] private Text secondaryName;
        [SerializeField] private Text secondaryAmmo;
        [SerializeField] private GameObject reloadIndicator;

        private PlayerWeaponController _controller;
        private WeaponHUD _hud;

        private void Awake()
        {
            _hud = new WeaponHUD();
            _hud.OnHUDUpdated += RefreshUI;
        }

        private void Start()
        {
            _controller = FindObjectOfType<WeaponInputHandler>()?.Controller;
        }

        private void Update()
        {
            if (_controller != null)
                _hud.Refresh(_controller);
        }

        private void RefreshUI()
        {
            var primary1 = _hud.Slots[WeaponSlot.Primary1];
            var primary2 = _hud.Slots[WeaponSlot.Primary2];

            var activePrimary = primary2.IsActive ? primary2 : primary1;

            if (primaryName != null)
                primaryName.text = activePrimary.WeaponName;
            if (primaryAmmo != null)
                primaryAmmo.text = $"{activePrimary.CurrentAmmo} / {activePrimary.ReserveAmmo}";

            var secondary = _hud.Slots[WeaponSlot.Secondary];
            if (secondaryName != null)
                secondaryName.text = secondary.WeaponName;
            if (secondaryAmmo != null)
                secondaryAmmo.text = $"{secondary.CurrentAmmo} / {secondary.ReserveAmmo}";

            if (reloadIndicator != null)
                reloadIndicator.SetActive(activePrimary.IsReloading || secondary.IsReloading);
        }
    }
}
