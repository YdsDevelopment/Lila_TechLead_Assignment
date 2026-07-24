using UnityEngine;

namespace LilaTest.WeaponSystem
{
    public sealed class WeaponInputHandler : MonoBehaviour
    {
        [SerializeField] private KeyCode primary1Key = KeyCode.Alpha1;
        [SerializeField] private KeyCode primary2Key = KeyCode.Alpha2;
        [SerializeField] private KeyCode secondaryKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode reloadKey = KeyCode.R;
        [SerializeField] private int fireMouseButton = 0;

        private PlayerWeaponController _controller;

        public PlayerWeaponController Controller => _controller;

        private void Awake()
        {
            _controller = new PlayerWeaponController();
        }

        private void Update()
        {
            PollInput();
            _controller.Tick(Time.deltaTime, Time.time);
        }

        private void PollInput()
        {
            if (Input.GetKeyDown(primary1Key))
                _controller.SwitchWeapon(WeaponSlot.Primary1);

            if (Input.GetKeyDown(primary2Key))
                _controller.SwitchWeapon(WeaponSlot.Primary2);

            if (Input.GetKeyDown(secondaryKey))
                _controller.SwitchWeapon(WeaponSlot.Secondary);

            if (Input.GetMouseButton(fireMouseButton))
                _controller.Fire(Time.time);

            if (Input.GetKeyDown(reloadKey))
                _controller.Reload(Time.time);
        }
    }
}
