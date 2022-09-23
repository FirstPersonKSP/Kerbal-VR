# KerbalVR Developer Notes

These are a collection of notes aimed at developers working with the KerbalVR codebase.

## History

This is JonnyOThan's fork of Vivero's KerbalVR mod.  You should look at https://github.com/Vivero/Kerbal-VR to understand why some things are the way that they are.  But I will try to keep this document up to date regarding how to build and contribute to this mod as it stands today.

Vivero's version of KerbalVR achieved stereoscopic rendering by using custom cameras in the game world to render the scene to textures and a native DLL to send those textures off to the headset.  My approach is different: I've used a Unity patcher to enable Unity's native VR support, which seems to give smoother performance overall.  See https://www.notion.so/beastsaber/How-To-Force-Any-Unity-Game-to-Run-In-Native-VR-Mode-cf8c50f66f2740d5b692db786a8386a1 

## Organization

- **KerbalVR_Mod**

  This is the Kerbal Space Program mod. A standard C# project that implements
  all of the mod features, structured like any other KSP mod.

  **KerbalVR-RPM**

  This contains any RPM-specific logic.  KerbalVR will have abstract interfaces for props that need to take action based on VR events, and this library contains the implementation of those interfaces that are hooked up to RPM.  Some day, another library could be made for interfacing with MAS in order to support that system as well.  This project references RPM directly and uses Krafs publicizer to get at some of the internals.

- **KerbalVR_Unity**

  This is a Unity project used to export the app toolbar UI assets, and other
  custom VR-related assets. It includes the KSP PartTools package to export
  asset bundles that are loaded by KSP at runtime.

- **KerbalVR_UnitySteamVR**

  This is a Unity project with the SteamVR project. Currently it is only used
  to facilitate exporting SteamVR input bindings.

## Compiling

- **KerbalVR_Mod solution**

  Open in **Visual Studio 2022**. For each of the KerbalVR and KerbalVR-RPM projects, in the project properties under Reference Paths, add the path to your KSP root directory (where the executable is).  This information is not stored in the csproj so different developers can have different KSP install locations.  To compile,  hit **Build > Build Solution**.

  These projects automatically copy files into the local repo's GameData directory.  If you are iterating on code, I suggest making a junction or symlink from your KSP's GameData directory to point at your development directory.

  If you clone the *KerbalVR* repo, you can build this solution alone **without
  the having to first build the other projects**. The repo will contain the
  latest binaries generated from the other projects, including the Unity asset bundles.

- **KerbalVR_Unity**

  Open in **Unity 2019.4.18f1**. Use *PartTools* to export the asset bundles
  for KSP. Run *&lt;KerbalVR_root&gt;\KerbalVR_Unity\export_asset_bundles.cmd*
  to copy the asset bundles from this project into the appropriate directory
  in the *KerbalVR_Mod* project.

- **KerbalVR_UnitySteamVR**

  Open in **Unity 2019.4.18f1**. Go to **Window > SteamVR Input**. Make any
  modifications to actions as needed. Click **Save and Generate**. To make
  changes to controller bindings, click **Open binding UI**. Edit the
  *Local Changes* configuration for a selected controller. When finished,
  click the **Replace Default Binding** button on the bottom-right.

  To export all the changes, run
  *&lt;KerbalVR_root&gt;\KerbalVR_BuildTools\copy_input_bindings.py*,
  which will place all the input bindings into the GameData folder.

## Other Thoughts

I experimented for a long time trying to use the SteamVR interaction system out-of-the-box, which requires exporting an assetbundle containing SteamVR scripts from unity and loading that in KSP at runtime.  This *almost* worked except that non-component data in the prefab was not serialized correctly due to not being supported by Unity: https://issuetracker.unity3d.com/issues/assetbundle-is-not-loaded-correctly-when-they-reference-a-script-in-custom-dll-which-contains-system-dot-serializable-in-the-build.  Note that the scripted components themselves loaded just fine as long as a SteamVR.dll assembly was present and contained classes with the same names (it didn't even have to be the same one that was built from Unity).

Because of the prefab/assetbundle experiments, SteamVR is now split out to its own project and assembly.  It is now rather close to the stock version that Unity would build rather than the stripped down version that was included in the old mod.  The only edits to this project should be to make it compatible with the KSP environment.

I haven't figured out how to distribute this yet; the VR patching step is probably too complicated for most users.  A BepInEx mod similar to the "VREnabler" on the patching page might be the best path forward.
