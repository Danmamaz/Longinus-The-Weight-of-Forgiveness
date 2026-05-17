using UnityEngine;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Seals the boss arena and engages the boss when the player enters the trigger volume.
    /// Attach to a trigger Collider at the arena entrance. Fires once.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BossArenaTrigger : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("BossController to engage when the player enters.")]
        private BossController _boss;

        [SerializeField, Tooltip("Tag used to identify the player collider.")]
        private string _playerTag = "Player";

        [SerializeField, Tooltip("When true, disables this collider after firing so the trigger cannot repeat.")]
        private bool _consumeOnEnter = true;

        #endregion

        #region Private Variables

        private bool _hasFired;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        #endregion

        #region Event Listeners/Callbacks

        private void OnTriggerEnter(Collider other)
        {
            if (_hasFired) return;
            if (!other.CompareTag(_playerTag)) return;
            if (_boss == null) return;

            _hasFired = true;
            _boss.EngageBoss();

            if (_consumeOnEnter)
                GetComponent<Collider>().enabled = false;
        }

        #endregion
    }
}
