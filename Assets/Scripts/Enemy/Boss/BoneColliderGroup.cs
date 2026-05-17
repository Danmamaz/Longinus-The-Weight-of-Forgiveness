using UnityEngine;
using Longinus.InGameItems;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Groups one or more <see cref="DamageCollider"/> components attached to boss bones
    /// so a single Animation Event string can enable or disable compound attack hitboxes.
    /// </summary>
    public class BoneColliderGroup : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("Identifier used by Animation Events (e.g. 'SweepArm', 'ThrustWeapon', 'AoESlam').")]
        private string _groupName;

        [SerializeField, Tooltip("All DamageCollider components belonging to this group.")]
        private DamageCollider[] _colliders;

        #endregion

        #region Public Properties

        public string GroupName => _groupName;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Activates every collider in this group.
        /// </summary>
        public void Enable()
        {
            foreach (var c in _colliders)
            {
                if (c != null) c.Enable();
            }
        }

        /// <summary>
        /// Deactivates every collider in this group.
        /// </summary>
        public void Disable()
        {
            foreach (var c in _colliders)
            {
                if (c != null) c.Disable();
            }
        }

        #endregion
    }
}
