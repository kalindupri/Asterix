using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Shared physics materials (created once at runtime).</summary>
    public static class PhysicsMaterials
    {
        static PhysicsMaterial2D _noFriction, _crate;

        /// <summary>
        /// For characters. Combine mode "Minimum" matters: Unity 6 averages friction by default, so a
        /// zero-friction character would still drag (and lose jump height) sliding along rock walls.
        /// </summary>
        public static PhysicsMaterial2D NoFriction
        {
            get
            {
                if (_noFriction != null) return _noFriction;
                _noFriction = new PhysicsMaterial2D("NoFriction")
                {
                    friction = 0f,
                    bounciness = 0f,
                    frictionCombine = PhysicsMaterialCombine2D.Minimum,
                    bounceCombine = PhysicsMaterialCombine2D.Minimum
                };
                return _noFriction;
            }
        }

        public static PhysicsMaterial2D Crate =>
            _crate != null ? _crate : (_crate = new PhysicsMaterial2D("Crate") { friction = 0.35f, bounciness = 0f });
    }
}
