using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons.Pets;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí dạng triệu hồi Pet. Pet sẽ tồn tại vĩnh viễn và đi theo người chơi.
    /// Vũ khí này đóng vai trò là "người quản lý" chỉ số và cooldown cho Pet.
    /// </summary>
    public class Weapon_PetSummon : WeaponBase
    {
        [Header("Pet Settings")]
        [Tooltip("Prefab của con Pet sẽ được sinh ra (Phải có gắn PetController)")]
        public PetController petPrefab;
        
        private PetController _spawnedPet;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);

            // Tìm GameObject có tag Player để truyền cho Pet đi theo
            // Hoặc lấy trực tiếp transform của Character gốc (nếu weapon được gắn trên con player)
            Transform playerTransform = transform.root; 

            if (petPrefab != null)
            {
                // Sinh ra con pet ở ngay vị trí người chơi
                _spawnedPet = Instantiate(petPrefab, playerTransform.position, Quaternion.identity);
                _spawnedPet.Initialize(this, playerTransform);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Chưa gắn Pet Prefab!");
            }
        }

        protected override bool CanAttack()
        {
            // Chỉ tấn công khi pet đang trong trạng thái combat và ở gần mục tiêu
            return _spawnedPet != null && _spawnedPet.CurrentState == PetController.PetState.Combat && _spawnedPet.IsCloseToTarget();
        }

        protected override void PerformAttack()
        {
            // Báo hiệu cho Pet thực hiện cú cắn
            if (_spawnedPet != null)
            {
                _spawnedPet.BiteTarget();
            }
        }

        private void OnDestroy()
        {
            // Xoá pet khi súng này bị xoá (hoặc chết)
            if (_spawnedPet != null)
            {
                Destroy(_spawnedPet.gameObject);
            }
        }
    }
}
