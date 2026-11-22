# Ironoak — Puzzle Registry

Every puzzle in the game is logged here **before** it is built. This is what keeps puzzles
from repeating.

**Rules**
1. **One idea, one place.** Each *core idea* appears once. Reusing an idea needs a real
   twist (a new power, a time limit, darkness, moving water, an escort), noted in the *Twist*
   column, or the puzzle is cut.
2. **From Level 2 on**, every non-lesson puzzle combines the level's new power with at least
   one older power.
3. **Lesson puzzles teach without text:** a safe space, one obvious solution, and the
   solution visible before the player needs it.
4. **Status:** `idea` → `greybox` → `tested` → `final`.

**Powers:** S = Sling · R = Rootgrow · B = Branchgrow · V = Vinefall · L = Lodestone · H = Forge-heat
**Types:** Lesson · Test · Twist · Combo · Set piece · Trial (optional, hard)

---

## Level 1 — The Oak Road (POC)

| ID | Type | Powers | Core idea | Twist vs. earlier | Status |
|---|---|---|---|---|---|
| P1-01 | Lesson | S | Sling a bell to open a gate | — (first puzzle) | idea |
| P1-02 | Test | S | Cut a rope to drop a bridge, timed so you cross before a patrol turns | Adds timing to P1-01 | idea |
| P1-03 | Lesson | R | Raise a root column to reach a ledge | — | idea |
| P1-04 | Test | R | Raise a root under a crate to lift it, then use the crate as a step | Root moves *objects*, not you | idea |
| P1-05 | Twist | R | Grow a root wall to dam a stream; the pool drains and opens a path (and Secret 1C) | Root changes the *environment* | idea |
| P1-06 | Combo | R+S | Lift a crate with a root, then sling its rope so it drops onto a pressure plate | First combination | idea |
| P1-T | Trial | R+S | Three knots, one crate, two plates: root-lift order matters, and a wrong order traps the crate | Sequencing / planning | idea |
| P1-07 | Set piece | R | Boar stampede: grow roots on the run to vault and to block a boar | Root used *under time pressure* | idea |

## Level 2 — The River Crossing

| ID | Type | Powers | Core idea | Twist vs. earlier | Status |
|---|---|---|---|---|---|
| P2-01 | Lesson | B | Grow a branch as a bridge across a gap | — | idea |
| P2-02 | Test | B | Angle a branch as a ramp to slide a crate across | Branch carries objects | idea |
| P2-03 | Twist | B+S | Snap your own branch so it falls as a dam or a weight | Destroying your creation is the solution | idea |
| P2-04 | Combo | B+R | Root and branch dam raise the water so a floating crate reaches a ledge | Water level as the puzzle state | idea |
| P2-05 | Combo | B+R+S | Branches block a spearman's spears while you sling the raft's mooring | Defence plus puzzle together | idea |
| P2-T | Trial | B | Branches snap after 3 s: a timed relay across a chasm | Branch with a timer | idea |
| P2-06 | Set piece | B | Rapids ride: grow branches to deflect debris away from the raft | Branch as a shield, on the move | idea |

## Level 3 — The Night Marsh

| ID | Type | Powers | Core idea | Twist vs. earlier | Status |
|---|---|---|---|---|---|
| P3-01 | Lesson | V | Drop a vine and climb it | — | idea |
| P3-02 | Test | V | Swing from vine to vine over a bog | Momentum movement | idea |
| P3-03 | Twist | V | A vine as a pulley: hang a log on it to raise a gate | Vine as a mechanism, not movement | idea |
| P3-04 | Combo | V+R | Roots block lantern beams so you can pass while hanging on a vine | First stealth puzzle | idea |
| P3-05 | Combo | V+B+R | Stealth route through 3 vision cones, each blocked or bypassed with a different power | Choice of solutions | idea |
| P3-T | Trial | V+S | Lights-out room lit only by a lantern you swing on a vine | Darkness as the puzzle | idea |
| P3-06 | Set piece | V+R | Search line: hide in reeds, climb vines above the lanterns as they sweep | Stealth under a moving threat | idea |

## Level 4 — The Roman Camp

| ID | Type | Powers | Core idea | Twist vs. earlier | Status |
|---|---|---|---|---|---|
| P4-01 | Lesson | L | Pull an iron crate toward you | — | idea |
| P4-02 | Test | L | Push a counterweight to raise a portcullis | Pull vs. push | idea |
| P4-03 | Twist | L | Rip a shield from a shield-bearer, then use it as a platform or bridge | Enemy equipment becomes a tool | idea |
| P4-04 | Combo | L+B+R | Escort: build a path a freed smith can walk (no jumping) | Solving for *someone else's* movement | idea |
| P4-05 | Combo | L+B+S | Gate with 3 locks: one magnetic, one branch-triggered, one sling target | Multi-power lock | idea |
| P4-T | Trial | L | Magnetic chains swinging over spikes; pull and release to cross | Lodestone as a movement tool | idea |
| P4-06 | Set piece | L+S+all | Wagon breakout: free 3 wagons before the alarm horn ends | Timed multi-objective | idea |

## Level 5 — The Iron Mines

| ID | Type | Powers | Core idea | Twist vs. earlier | Status |
|---|---|---|---|---|---|
| P5-01 | Lesson | H | Heat iron bars and bend them apart to pass | — | idea |
| P5-02 | Test | H | Melt a lock | Heat destroys | idea |
| P5-03 | Twist | H | A heated iron bridge expands to close a gap, then shrinks as it cools | Timer created by a material property | idea |
| P5-04 | Combo | H+L | Pull a glowing hot crate past braziers to light them in sequence | Heat carried by Lodestone | idea |
| P5-05 | Combo | H+L+V+B+R | Forge room: all powers, one machine to restart | Grand combination before the finale | idea |
| P5-T | Trial | H | Heat-timer relay: 5 bars must glow at the same time | Parallel timers | idea |
| P5-06 | Set piece | H | Cart escape: heat and bend rails mid-ride to switch tracks | Heat used at speed | idea |

## Level 6 — Fort Varro

| ID | Type | Powers | Core idea | Twist vs. earlier | Status |
|---|---|---|---|---|---|
| P6-01 | Combo | R+V+B | Storm wall climb; wind changes which power is safe | Environmental pressure on old powers | idea |
| P6-02 | Combo | L+H | Ballista bolts stuck in walls are iron: heat and pull them to make steps | Enemy fire becomes a platform | idea |
| P6-03 | Combo | All | Free Tavo's cell: each lock needs a different power, in order | Mastery check | idea |
| P6-T | Trial | All | The Last Trial: all five powers in a single unbroken sequence | Full mastery | idea |
| P6-B | Set piece | All | War machine boss: L armour → H joints → R+B reach the gear → S one perfect shot | Each phase a different power | idea |

---

## Coverage summary

| Power | Introduced | Used as a tool in | Used under pressure in |
|---|---|---|---|
| Sling | L1 | every level | L1 patrol, L4 breakout, L6 boss |
| Rootgrow | L1 | L1–L6 | L1 chase, L3 search line |
| Branchgrow | L2 | L2–L6 | L2 rapids |
| Vinefall | L3 | L3–L6 | L3 search line |
| Lodestone | L4 | L4–L6 | L4 breakout, L6 boss |
| Forge-heat | L5 | L5–L6 | L5 cart escape, L6 boss |

**Total:** 41 puzzles: 29 main-path puzzles, 6 Druid's Trials, 6 set pieces.
