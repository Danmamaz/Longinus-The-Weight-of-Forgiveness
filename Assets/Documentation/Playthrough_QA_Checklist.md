# Longinus: The Weight of Forgiveness — Playthrough QA Checklist

**Vertical Slice — Estimated playtime: ~20 min**
**Build version:** 1.0.0-vertical-slice

---

## Critical Path

- [ ] Game launches to Main Menu without errors in Console
- [ ] Logo animation plays; menu buttons slide up after intro sequence
- [ ] **New Game** wipes any existing save and loads the Introduction Chapter
- [ ] **Continue** is disabled (greyed or skipped) if no save file exists
- [ ] **Continue** loads the last save correctly (position, scene, plot flags)
- [ ] Tutorial prompts appear at the correct moments
- [ ] Player can move, sprint, roll, and attack
- [ ] Melee enemy spawns and pursues the player
- [ ] Player can kill the melee enemy
- [ ] Checkpoint is interactable; rest animation plays; enemies respawn
- [ ] Resting restores health and stamina to full
- [ ] Scene teleporter transitions from Introduction Chapter → Beach Arena
- [ ] Boss encounter loads correctly
- [ ] Boss Phase 1 and Phase 2 transitions trigger correctly
- [ ] Boss can be killed or spared; both paths end the demo

---

## Audio

- [ ] Main Menu music plays on startup
- [ ] Music crossfades when transitioning between scenes (no abrupt cut)
- [ ] Beach Arena has distinct ambient music track
- [ ] Sword swing SFX plays on every player attack
- [ ] Hit SFX plays when player or enemy takes damage
- [ ] Player hurt SFX is distinct from enemy hurt SFX
- [ ] Checkpoint activation SFX plays on rest
- [ ] Menu click SFX plays when the menu buttons appear
- [ ] No AudioSource errors (NullReferenceException) in Console during scene load
- [ ] No audio stuttering or double-play on rapid hits

---

## Visual / Post-Processing

- [ ] Hit spark + smoke particles play on every successful hit
- [ ] Dissolve shader triggers on enemy death (body fades out)
- [ ] Foliage bends away from the player as they walk through it
- [ ] Post-processing transitions to Death preset when player dies
- [ ] Post-processing transitions to Rest preset during checkpoint rest
- [ ] Post-processing transitions back to Normal after resting ends
- [ ] Boss Phase 2 triggers a visible tint shift (reddish tone) on arena geometry
- [ ] Arena fog volume is visible in the Beach level
- [ ] Ambient dust motes are visible in the Introduction Chapter
- [ ] No magenta (missing shader) materials anywhere in both scenes
- [ ] No Z-fighting or major clipping artifacts on main path geometry

---

## Performance

- [ ] Stable ≥ 30 fps in Introduction Chapter (use Stats overlay or frame counter)
- [ ] Stable ≥ 30 fps during boss fight with particles and PostFX active
- [ ] No frame spikes (>100 ms) when checkpoint rest triggers respawn of all enemies
- [ ] No memory leak warnings in Console after 10 minutes of play
- [ ] Hit impact pool does not produce NullReferenceException after 20+ consecutive hits
- [ ] Scene transition does not cause visible freeze lasting more than 1 second

---

## Save / Load

- [ ] Resting at checkpoint saves position, health, stamina, and scene index
- [ ] Quitting and relaunching resumes in the correct scene at the correct position
- [ ] New Game correctly deletes the old save (Continue no longer works until next checkpoint)
- [ ] Corrupt save auto-falls back to `.backup` without crash
- [ ] `save.dat` exists in `Application.persistentDataPath` after first checkpoint rest

---

## Edge Cases

- [ ] Player dies → Death screen → respawn at last checkpoint (do not crash)
- [ ] Spam-clicking attack during roll does not freeze the combo state
- [ ] Enemy that was mid-attack at checkpoint rest respawns cleanly (no stuck animation)
- [ ] Pausing during a scene transition does not softlock
- [ ] Pressing Interact when no interactable is nearby does nothing (no error)
