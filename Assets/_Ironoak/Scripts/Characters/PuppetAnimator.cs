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
        [Header("Locomotion")]
        [Tooltip("Distance covered by one full cycle (two steps) when walking slowly / at full run.")]
        public float walkStride = 1.1f;
        public float runStride = 2.1f;
        public float walkArmSwing = 22f;
        public float runArmSwing = 52f;
        public float runLean = 13f;
        public float runBob = 0.05f;
        public float walkBob = 0.02f;
        [Tooltip("Extra forward lean per unit/s² of acceleration (negative when braking).")]
        public float accelLean = 0.12f;

        // Key poses sampled at 8 evenly spaced points of the stride (0 = this leg's heel contact).
        // Thigh: + forward. Shin: relative to thigh, - bends the knee back.
        static readonly float[] RunThigh = { 28f, 10f, -15f, -38f, -25f, 15f, 50f, 45f };
        static readonly float[] RunShin = { -12f, -38f, -30f, -25f, -95f, -108f, -65f, -18f };
        static readonly float[] WalkThigh = { 22f, 12f, 0f, -12f, -22f, -10f, 12f, 24f };
        static readonly float[] WalkShin = { -5f, -12f, -6f, -8f, -25f, -50f, -35f, -10f };
        // Body height over the stride (both legs, so the pattern repeats twice): low just after
        // contact, high in flight (run) or while passing (walk).
        static readonly float[] RunBob = { -0.2f, -1f, 0.1f, 0.9f, -0.2f, -1f, 0.1f, 0.9f };
        static readonly float[] WalkBob = { -0.5f, 0f, 0.7f, 0.1f, -0.5f, 0f, 0.7f, 0.1f };

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

        float _t, _phase, _moveW, _speed01, _airW, _aimW, _castT, _throwT, _landT, _deadSpin;
        float _lastVx, _accel;
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
            pc.Landed += () => { _squash = new Vector2(1.16f, 0.84f); _landT = LandTime; };
            if (_sling != null) _sling.Thrown += () => _throwT = 0.25f;
            if (root != null) root.Cast += () => _castT = 0.45f;
        }

        static void Rot(Transform t, float deg)
        {
            if (t != null) t.localRotation = Quaternion.Euler(0f, 0f, deg);
        }

        static float Approach(float current, float target, float rate, float dt) =>
            Mathf.Lerp(current, target, 1f - Mathf.Exp(-rate * dt));

        const float LandTime = 0.16f;

        /// <summary>Smooth looping (Catmull-Rom) sample of evenly spaced keys at phase p (cycles).</summary>
        static float Cyc(float[] k, float p)
        {
            int n = k.Length;
            float x = Mathf.Repeat(p, 1f) * n;
            int i = (int)x;
            float t = x - i;
            float p0 = k[(i - 1 + n) % n], p1 = k[i % n], p2 = k[(i + 1) % n], p3 = k[(i + 2) % n];
            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (3f * p1 - p0 - 3f * p2 + p3) * t * t * t);
        }

        static float Gait(float[] walk, float[] run, float p, float s01) => Mathf.Lerp(Cyc(walk, p), Cyc(run, p), s01);

        void LateUpdate()
        {
            if (_pc == null || _rig == null) return;
            float dt = Time.deltaTime;
            _t += dt;

            Vector2 v = _pc.Velocity;
            float facing = _pc.Facing;
            bool grounded = _pc.Grounded;
            float speed = Mathf.Abs(v.x);

            _moveW = Approach(_moveW, grounded && speed > 0.25f ? 1f : 0f, 10f, dt);
            _speed01 = Approach(_speed01, Mathf.Clamp01(speed / _pc.maxSpeed), 8f, dt);
            _airW = Approach(_airW, grounded ? 0f : 1f, 18f, dt);
            _aimW = Approach(_aimW, _sling != null && _sling.Aiming ? 1f : 0f, 20f, dt);
            _castT = Mathf.Max(0f, _castT - dt);
            _throwT = Mathf.Max(0f, _throwT - dt);
            _landT = Mathf.Max(0f, _landT - dt);
            float s01 = _speed01;
            if (grounded) _phase += speed / Mathf.Lerp(walkStride, runStride, s01) * dt;
            if (dt > 0f) _accel = Approach(_accel, grounded ? (v.x - _lastVx) / dt * facing : 0f, 10f, dt);
            _lastVx = v.x;

            float breathe = Mathf.Sin(_t * 2.2f);
            float idleW = 1f - _moveW;

            // ---- idle: relaxed, weight on the back leg, breathing through the shoulders ----
            float sway = Mathf.Sin(_t * 0.9f);
            float iLegF = 9f, iShinF = -7f, iLegB = -5f, iShinB = -3f;
            float iArmF = 7f + breathe * 2.5f, iForeF = 18f + breathe * 3f;
            float iArmB = -6f - breathe * 2f, iForeB = 14f;
            float iTorso = breathe * 1.5f + sway * 0.8f;
            float iHead = -breathe * 1f + Mathf.Sin(_t * 0.63f) * 2.5f;
            float iBob = breathe * 0.007f - 0.01f;

            // ---- walk/run: key-posed gait, legs half a stride apart, arms opposite the legs ----
            float pF = _phase, pB = _phase + 0.5f;
            float mLegF = Gait(WalkThigh, RunThigh, pF, s01), mShinF = Gait(WalkShin, RunShin, pF, s01);
            float mLegB = Gait(WalkThigh, RunThigh, pB, s01), mShinB = Gait(WalkShin, RunShin, pB, s01);
            float armAmp = Mathf.Lerp(walkArmSwing, runArmSwing, s01);
            float armWave = Mathf.Cos((_phase - 0.78f) * Mathf.PI * 2f); // 1 when the front leg is furthest forward
            float mArmF = -armAmp * armWave, mArmB = armAmp * armWave;
            float foreBase = Mathf.Lerp(18f, 82f, s01), foreExtra = Mathf.Lerp(14f, 24f, s01);
            float mForeF = foreBase + foreExtra * Mathf.Max(0f, -armWave);
            float mForeB = foreBase + foreExtra * Mathf.Max(0f, armWave);
            float lean = Mathf.Lerp(3f, runLean, s01) + Mathf.Clamp(_accel * accelLean, -8f, 9f);
            float bobKey = Mathf.Lerp(Cyc(WalkBob, _phase) * walkBob, Cyc(RunBob, _phase) * runBob, s01);
            float mTorso = -lean + bobKey * 25f;               // chest dips a little on each contact
            float mHead = lean * 0.55f - bobKey * 30f;         // eyes stay on the road ahead
            float mBob = bobKey - Mathf.Lerp(0.02f, 0.035f, s01); // angled legs are shorter: keep the feet on the ground

            float legF = Mathf.Lerp(mLegF, iLegF, idleW), shinF = Mathf.Lerp(mShinF, iShinF, idleW);
            float legB = Mathf.Lerp(mLegB, iLegB, idleW), shinB = Mathf.Lerp(mShinB, iShinB, idleW);
            float armF = Mathf.Lerp(mArmF, iArmF, idleW), foreF = Mathf.Lerp(mForeF, iForeF, idleW);
            float armB = Mathf.Lerp(mArmB, iArmB, idleW), foreB = Mathf.Lerp(mForeB, iForeB, idleW);
            float torso = Mathf.Lerp(mTorso, iTorso, idleW);
            float head = Mathf.Lerp(mHead, iHead, idleW);
            float bob = Mathf.Lerp(mBob, iBob, idleW);
            float hipX = sway * 0.012f * idleW;

            // ---- landing: knees soak up the impact ----
            if (_landT > 0f)
            {
                float k = Mathf.Sin(_landT / LandTime * Mathf.PI) * (1f - 0.5f * _moveW);
                legF += 30f * k; shinF -= 55f * k;
                legB += 22f * k; shinB -= 45f * k;
                torso -= 10f * k; head += 6f * k;
                bob -= 0.09f * k;
                armF += 15f * k; armB += 10f * k;
            }

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
            if (_pelvis != null) _pelvis.localPosition = _pelvisRest + new Vector3(hipX, bob, 0f);
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
