________________________________________________________________________________________
                                  Ultimate Rope Editor
                          Copyright © 2015 Ultimate Game Tools
                            http://www.ultimategametools.com
                               info@ultimategametools.com
________________________________________________________________________________________
Version 1.18


________________________________________________________________________________________
Introduction

The Ultimate Rope Editor is a powerful tool to add rope based physics to your games.
It can generate procedural ropes and also apply rope physics to your existing meshes.
The editor is fully what-you-see-is-what-you-get, physics included, because everything
can be created and configured using the Unity Editor in play mode.
As there is no need to stop and re-run to see the changes, setting everything up becomes
a much faster process. The rope editor allows each rope state to persist when exiting
from the play mode, this is extremely useful when keeping the current state to finish
editing later or to make it to to the actual game.


________________________________________________________________________________________
Requirements

Works on Unity 3.5, 4.x and 5
Compatible with all platforms


________________________________________________________________________________________
Help

For up to date help: http://www.ultimategametools.com/products/rope_editor/help
For additional support contact us at http://www.ultimategametools.com/contact or by
e-mail at info@ultimategametools.com


________________________________________________________________________________________
Version history

v1.18 - 14/05/2015:

[FIX] Fixed bug causing the following error to pop up on the console at runtime:
      MissingReferenceException: The object of type 'SkinnedMeshRenderer' has been
	  destroyed but you are still trying to access it.

v1.17 - 05/03/2015:

[NEW] Added full Unity 5 compatibility.

v1.16 - 27/10/2014:

[DEL] Removed UltimateRope.cs.bak which was included by accident in v1.15 (26/10/2014).

v1.15 - 26/10/2014:

[NEW] Added rope prefab instancing support through the new "Enable Prefab Usage"
      parameter.
[FIX] Fixed a bug that made breakable ropes snap when the last link was reached. 

v1.14 - 29/12/2013:

[FIX] Changed scripts to remove the following deprecated warning:
      "UnityEngine.Transform.GetChildCount() is obsolete, use Transform.childCount
	  instead"

v1.13 - 22/10/2013:

[NEW] Improved collision performance on extensible ropes (extensible links now have
      their colliders disabled).
[FIX] Extensible ropes can now also be breakable.
[FIX] Fixed parenting issues on the first and last node of each rope segment.
[FIX] "Send Rope Events" no more locks the parameters in the rope segments section.

v1.12 - 14/09/2013:

[FIX] Changed + and - numeric pad keys to i and o for Mac compatibility in the sample
      scenes.
[FIX] Removed warning messages on Unity 4.x complaining about deprecated method
      SetActiveRecursively() (version 1.11 did only work on 4.0).

v1.11 - 13/06/2013:

[FIX] Fixed bug that caused error messages when a rope was extensible but without coil.
[FIX] Removed warning messages on Unity 4.x complaining about deprecated method
      SetActiveRecursively().

v1.10 - 29/03/2013:

[NEW] Added the new "Convert to static mesh" button that will make the rope static.
      This is useful when you want to use the editor to model a rope but will remain
	  static in the game. This way it benefits of better performance and the possibility
	  to have lightmaps and exact fitting mesh colliders.
[NEW] Added new parameter "Lock Start/End ZAxis" to procedural ropes.
      This parameter will force each first link of a segment to exit in the direction
	  of the start node Z axis and the last link of a segment to enter in the direction
	  of the ending node Z axis. Also, both links will be fixed and won't move.
	  This will help in some cases where you don't want the rope to exit bent down due
	  do to gravity or ending the same way.
[NEW] Links can now be changed to kinematic through the rigidbody component and that
      information will be also be stored when "persist after playmode" is activated.
	  Rope regeneration (through "reset rope" or by forcing it to regenerate changing
	  some parameters) will remove this information though.
[NEW] Tangents are now also created when building the procedural rope mesh. Tangents
      are needed for many advanced shaders.
[FIX] Fixed bug that caused UltimateRope.Regenerate() to create joints incorrectly when
      executed at runtime.
[FIX] Fixed bug that caused coils to be generated incorrectly at start.

V1.00 - 18/02/2013:

[---] Initial release