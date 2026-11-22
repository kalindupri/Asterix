using Ironoak.Gameplay;
using Ironoak.Powers;
using UnityEngine;

namespace Ironoak.Characters
{
    /// <summary>
    /// Procedural animation for Brenna's cut-out puppet. No keyframes: every pose is computed from
    /// the live physics state each frame, so it reacts exactly to what the player does.
    ///
    /// Angles are degrees, counter-clockwise, in "facing right" space. Limbs hang straight down at 0;
    /// a positive angle swings a limb forward (toward the facing direction).
    /// </summary>
    public class PuppetAnimator : MonoBehaviour
    {
        [Header("Run")]
        public float strideRate = 2.4f;     // cycle radians per unit travelled
        public float legSwing = 38f;
        public float kneeBend = 60f;
        public float armSwing = 32f;
        public float runLean = 9f;
        public float bobHeight = 0.045f;

        [Header("Secondary motion")]
        public float braidStiffness = 14f;
        public float braidDrag = 4.5f;

        PuppetRig _rig;
        PlayerController _pc;
        PlayerHealth _hp;
        SlingAbility _sling;

        Transform _pelvis, _torso, _head, _armBU, _armBL, _armFU, _armFL, _legBU, _legBL, _legFU, _legFL;
        readonly Transform[] _braid = new Transform[3];
        readonly float[] _braidAngle = new float[3];
        Vector3 _pelvisRest;

        float _t, _phase, _runW, _airW, _aimW, _castT, _throwT, _deadSpin;
        Vector2 _squash = Vector2.one;

        public void Init(PuppetRig rig, PlayerController pc, PlayerHealth hp)
        {
            _rig = rig;
            _pc = pc;
            _hp = hp;
            _sling = pc.GetComponent<SlingAbility>();
            var root = pc.GetComponent<RootgrowAbility>();

            _pelvis = rig.Bone("pelvis");
            _torso = rig.Bone("torso");
            _head = rig.Bone("head");
            _armBU = rig.Bone("arm_back_upper");
            _armBL = rig.Bone("arm_back_lower");
            _armFU = rig.Bone("arm_front_upper");
            _armFL = rig.Bone("arm_front_lower");
            _legBU = rig.Bone("leg_back_upper");
            _legBL = rig.Bone("leg_back_lower");
            _legFU = rig.Bone("leg_front_upper");
            _legFL = rig.Bone("leg_front_lower");
            for (int i = 0; i < 3; i++) _braid[i] = rig.Bone("braid" + (i + 1));
            _pelvisRest = rig.RestPosition("pelvis");

            pc.Jumped += () => _squash = new Vector2(0.86f, 1.14f);
            pc.Landed += () => _squash = new Vector2(1.16f, 0.84f);
            if (_sling != null) _sling.Thrown += () => _throwT = 0.25f;
            if (root != null) root.Cast += () => _castT = 0.45f;
        }

        static void Rot(Transform t, float deg)
        {
            if (t != null) t.localRotation = Quaternion.Euler(0f, 0f, deg);
        }

        static float Approach(float current, float target, float rate, float dt) =>
            Mathf.Lerp(current, target, 1f - Mathf.Exp(-rate * dt));

        void LateUpdate()
        {
            if (_pc == null || _rig == null) return;
            float dt = Time.deltaTime;
            _t += dt;

            Vector2 v = _pc.Velocity;
            float facing = _pc.Facing;
            bool grounded = _pc.Grounded;
            float speed = Mathf.Abs(v.x);

            _runW = Approach(_runW, grounded ? Mathf.Clamp01(speed / _pc.maxSpeed) : 0f, 12f, dt);
            _airW = Approach(_airW, grounded ? 0f : 1f, 18f, dt);
            _aimW = Approach(_aimW, _sling != null && _sling.Aiming ? 1f : 0f, 20f, dt);
            _castT = Mathf.Max(0f, _castT - dt);
            _throwT = Mathf.Max(0f, _throwT - dt);
            if (grounded) _phase += speed * strideRate * dt;

            float s = Mathf.Sin(_phase), c = Mathf.Cos(_phase);
            float breathe = Mathf.Sin(_t * 2.2f);

            // ---- ground pose: idle blended with run cycle ----
            float legF = s * legSwing * _runW;
            float legB = -s * legSwing * _runW;
            float shinF = -(Mathf.Max(0f, c) * kneeBend * _runW + 4f);
            float shinB = -(Mathf.Max(0f, -c) * kneeBend * _runW + 4f);
            float armF = -s * armSwing * _runW + 3f;
            float armB = s * armSwing * _runW - 3f;
            float foreF = 12f + 28f * _runW;
            float foreB = 12f + 28f * _runW;
            float torso = -runLean * _runW + breathe * 1.2f * (1f - _runW);
            float head = breathe * 1.6f * (1f - _runW) + runLean * 0.5f * _runW;
            float bob = (Mathf.Abs(c) - 0.6f) * bobHeight * _runW + breathe * 0.006f * (1f - _runW);

            // ---- air pose: jump (rising) blended with fall ----
            if (_airW > 0.001f)
            {
                float rise = Mathf.Clamp01(v.y / 10f * 0.5f + 0.5f);
                float flail = Mathf.Sin(_t * 18f) * 6f * (1f - rise);
                float aLegF = Mathf.Lerp(12f, 38f, rise), aShinF = Mathf.Lerp(-20f, -70f, rise);
                float aLegB = Mathf.Lerp(-6f, -18f, rise), aShinB = Mathf.Lerp(-45f, -30f, rise);
                float aArmF = Mathf.Lerp(135f, 55f, rise) + flail, aArmB = Mathf.Lerp(120f, -35f, rise) - flail;
                float aTorso = Mathf.Lerp(4f, -6f, rise);
                legF = Mathf.Lerp(legF, aLegF, _airW); shinF = Mathf.Lerp(shinF, aShinF, _airW);
                legB = Mathf.Lerp(legB, aLegB, _airW); shinB = Mathf.Lerp(shinB, aShinB, _airW);
                armF = Mathf.Lerp(armF, aArmF, _airW); armB = Mathf.Lerp(armB, aArmB, _airW);
                foreF = Mathf.Lerp(foreF, 20f, _airW); foreB = Mathf.Lerp(foreB, 25f, _airW);
                torso = Mathf.Lerp(torso, aTorso, _airW);
                bob *= 1f - _airW;
            }

            // ---- Rootgrow cast: both arms thrust down toward the knot ----
            if (_castT > 0f)
            {
                float k = Mathf.Sin(_castT / 0.45f * Mathf.PI);
                armF = Mathf.Lerp(armF, 70f, k); armB = Mathf.Lerp(armB, 55f, k);
                foreF = Mathf.Lerp(foreF, 5f, k); foreB = Mathf.Lerp(foreB, 5f, k);
                torso = Mathf.Lerp(torso, -12f, k);
            }

            // ---- sling aim: the front arm tracks the aim direction, body leans into it ----
            if (_aimW > 0.001f && _sling != null)
            {
                Vector2 d = _sling.AimDirection;
                d.x *= facing;
                float aimAngle = Mathf.Atan2(d.x, -d.y) * Mathf.Rad2Deg; // 0 = down, 90 = forward, 180 = up
                float aTorso = Mathf.Clamp(-d.y * 10f, -10f, 10f) + 4f;
                torso = Mathf.Lerp(torso, aTorso, _aimW);
                armF = Mathf.Lerp(armF, aimAngle - torso, _aimW);
                foreF = Mathf.Lerp(foreF, 0f, _aimW);
                armB = Mathf.Lerp(armB, 25f, _aimW);
                foreB = Mathf.Lerp(foreB, 60f, _aimW);
                legF = Mathf.Lerp(legF, 14f, _aimW); legB = Mathf.Lerp(legB, -14f, _aimW);
            }
            if (_throwT > 0f) foreF -= Mathf.Sin(_throwT / 0.25f * Mathf.PI) * 70f; // whip on release

            // ---- apply ----
            if (_pelvis != null) _pelvis.localPosition = _pelvisRest + new Vector3(0f, bob, 0f);
            Rot(_torso, torso);
            Rot(_head, head);
            Rot(_legFU, legF); Rot(_legFL, shinF);
            Rot(_legBU, legB); Rot(_legBL, shinB);
            Rot(_armFU, armF); Rot(_armFL, foreF);
            Rot(_armBU, armB); Rot(_armBL, foreB);

            // ---- braid: spring chain with drag from movement ----
            float parentWorld = torso + head;
            float drag = Mathf.Clamp(-v.x * facing * braidDrag - v.y * 2.5f, -70f, 70f);
            for (int i = 0; i < 3; i++)
            {
                float target = 18f + drag * (0.6f + i * 0.25f) + Mathf.Sin(_t * 2.6f - i * 0.8f) * 4f;
                _braidAngle[i] = Approach(_braidAngle[i], target, braidStiffness - i * 3f, dt);
                Rot(_braid[i], _braidAngle[i] - parentWorld);
                parentWorld = _braidAngle[i];
            }

            // ---- squash & stretch, facing, damage and death ----
            _squash = Vector2.Lerp(_squash, Vector2.one, 1f - Mathf.Exp(-10f * dt));
            transform.localScale = new Vector3(_squash.x * facing, _squash.y, 1f);

            if (_hp != null)
            {
                if (_hp.Dead)
                {
                    _deadSpin = Approach(_deadSpin, 90f, 6f, dt);
                    transform.localRotation = Quaternion.Euler(0f, 0f, _deadSpin * facing);
                    _rig.SetVisible(true);
                }
                else
                {
                    bool blink = _hp.Invulnerable && ((int)(Time.time * 14f) & 1) == 1;
                    _rig.SetVisible(!blink);
                    _rig.Flash(new Color(1f, 0.45f, 0.4f), _hp.Invulnerable ? 0.35f : 0f);
                }
            }
        }
    }
}
