# Changelog

Discord support server: https://discord.gg/vBDzZAq3AF.

Please always post your [KSP.log file](https://gist.github.com/JonnyOThan/04c2074b56f78e56d115621effee30f9) when reporting issues.

## 0.9.4.4 - 2025-10-06

- Add patched version of scatterer for volumetric clouds V5


## 0.9.4.3 - 2025-09-08

### Notable Changes

- There are updates to non-volumetric Scatterer and Scatterer/Eve for volumetrics v4 in the Optional Mods folder.  Make sure to also install these if you're updating.  Many of the fixes are related to using the incorrect resolution for rendering.  You may notice a performance hit now that it's using the correct eye resolution.  You can compensate for this by reducing the eye resolution setting in SteamVR.
- The VR toggle key can now be configured in KerbalVR's settings.cfg.

### Bug Fixes

- Fixed several issues in scatterer and volumetric EVE
- Fixed the external hatch lever on the Restock mk2 lander can when in rover mode
- Fixed several external hatches on stock parts when SSPX is installed
- Fixed B9PS errors related to hatches on the mobile lab and hitchhiker pod when SSPX and restock were installed
- Disabled KSPCF's collisionmanager patch which interferes with grabbing ladders while on EVA

### Known Issues

- #145 PAW does not update or display properly when the game does not have focus


## 0.9.4.2 - 2025-02-07

### Notable Changes

- SteamVR updated to 2.8.  You **MUST** install the KSP_x64_Data folder from this release.
- Newer versions of Scatterer and Volumetric Clouds v4 are now supported.  See the readme file in Optional Mods or the [installation guide](https://github.com/FirstPersonKSP/Kerbal-VR/wiki/Installation-Guide) for details.

### Known Issues

- #145 PAW does not update or display properly when the game does not have focus


## 0.9.4.1 - 2023-12-19

### Bug Fixes

- Fixed softlock on startup caused by an apparent change in SteamVR behavior when enumerating devices

### Known Issues

- #145 PAW does not update or display properly when the game does not have focus


## 0.9.4.0 - 2023-11-10

### New Dependencies

- FreeIva 0.2.18.1 or later is now required

### Notable Changes

- [Exploration Rover System](https://forum.kerbalspaceprogram.com/topic/206952-112x-exploration-rover-system-by-aset/) is now fully supported
- [ALCOR](https://forum.kerbalspaceprogram.com/topic/220718-112-alcor-pod-adopted/) is now fully supported
- [Artemis Construction Kit](https://forum.kerbalspaceprogram.com/topic/209546-112x-artemis-construction-kit-stockalike-orion-sls-v140-lockheed-lander/) is now fully supported (with KSA IVA Upgrade)

### Bug Fixes

- Fixed coffee mug interaction in ProbeControlRoom
- Fixed issues with external command chair
- Fixed Near Future Props hatch issues
- Fixed far side hatch handles
- New steering yoke prop used in ERS, mk2 lander can, and probe control room
- Turning in place in EVA now works
- Quick load and revert buttons now work properly from VR
- Restored cover animations to most props so they can still be used with the mouse

### Known Issues

- #145 PAW does not update or display properly when the game does not have focus


## 0.9.3.0 - 2023-04-19

### Notable Changes

- Physical prop modules have been moved to FreeIva.  KerbalVR now requires FreeIva version 0.2.13.0 or later.

### Bugs Fixed

- Duplicate experiment data is now automatically dumped when boarding a pod from EVA.  This fixes an issue where the kerbal would get flung off into space.
- Added a disableTwist optional patch.  This removes the ability to twist the flightstick (for people that have rudder pedals etc)
- Swapping kerbals is more reliable and no longer exits to the flight camera
- Added support for ASET switch_b_button props (used in SOCK)
- Better Vive controls

### Known Issues

- #145 PAW does not update or display properly when the game does not have focus


## 0.9.2.0 - 2023-03-10

### Notable Changes

- Near Future Props and HabTech Props are now supported.  That means Near Future Spacecraft (including Vexarp!), Stockalike Station Parts Expansion, HabTech2, Planetside, SOCK, ACK, etc should all mostly work.
- Added breakable props - careful with those beakers!
- Restock is now supported
- Kerbal eye position is now defined in the settings.cfg file

### Bug Fixes

- Fixed dialing wand getting stuck active
- Fixed lots of stuff with ProbeControlRoom
- Grab & pull movement should be much less flaky
- Grabbing the flightstick with both hands no longer breaks the world
- PAW can be summoned even when the HUD is hidden
- Physical props now work for the MAS versions of ASET props

### Known Issues

- #145 PAW does not update or display properly when the game does not have focus


## 0.9.1.3 - 2023-02-16

### New Dependencies

- FreeIva must be updated to at least 0.2.9.0
- RasterPropMonitor must be updated to at least 0.31.10.2
- Added support for the [ALCOR pod](https://spacedock.info/mod/1205/ALCOR%20Advanced%20IVA).

### Notable Changes

- You can now grab ladders, railings, and the interior walls of the spacecraft to move yourself around.  WARNING: this is a little flaky, and if you find yourself flung out into space just hit C on the keyboard to return to your seat.
- Physical Props: Many objects in the cockpits can be grabbed and moved around.  Some even have special interactions when you press the PinchIndex or the PinchThumb button while holding them.  The ALCOR pod is a great testbed for this.
- Added a dialing wand: activate the pinch action (PinchIndex and PinchThumb together) to summon a narrow stick you can use to push buttons and flick switches when your fingers are too big for the job.

### Bug Fixes

- Fixed hand skew when grabbing things that are non-uniformly scaled
- Improved kerbal head hiding logic
- Fixed hiding the kerbal arms when using non-default suits
- Fixed abnormal world scaling for good this time (I hope!)
- Fixed the up/down animations on the ASET HUD prop
- Fixed crashes caused by internal cameras (this was actually fixed in RasterPropMonitor)

### Known Issues

- #145 PAW does not update or display properly when the game does not have focus


## 0.9.0.2 - 2023-02-02

- Fixed scaling issues
- Update dependencies and installcheck for MAS and RPM: As of MAS 1.3.6, you no longer need to manually copy KerbalVR-MAS to the plugins directory.
- Bundled EVE binaries for new volumetric clouds: if you have installed the new volumetric clouds from blackrack, install the EnvironmentalVisualEnhancements directory from the `Optional Mods` folder into your gamedata directory.  DO NOT install the scatterer folder.


## 0.9.0 - 2023-01-06

### New Dependencies

- FreeIva version 0.2.4 is now required

### Notable Changes

- FreeIva support (see controls page for details)

### Bug Fixes

- Fixed a bug where the hands could disappear if you loaded a save while holding something
- Fixed a bug that could break throttle control if you were holding the throttle and crashed the ship

### Known Issues

- #170 Game crashes when using external cameras
- #145 PAW does not update or display properly when the game does not have focus