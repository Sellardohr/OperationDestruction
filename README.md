# OperationDestruction
#Code written for a 2D isometric strategy RPG featuring heavy individual character and team customization.

#The most interesting script is BattleManager.cs, which is the main controller for the isometric battle system.
#This script creates data structures that allow the code to go back and forth between screen space which communicates directly with Unity and the normalized, integer-based battle grid which is used in other functions.
#It includes pathfinding functions that allow characters on the grid to identify what squares they can reach based on their own movement attributes and terrain obstacles and commands to move them about and execute abilities. It also facilitates a GUI for the user with battle and character information.

#Also of interest is IndividualPartScript.cs and various scripts referenced therein, which control the mech-building and character-customization parts of the code.
#These functions allow the player to select from a library of mech parts which dynamically snap together to create top-level attributes for a mech, including both active and passive abilities that will be imported into the combat.
#The identities and positions of these parts are stored in a custom serializable data structure that can be saved to JSON and rebuilt on command. These JSONs are also imported into a separate interface, controlled in ProtocolsManagerScript.cs, in which the player selects individual characters to build a team.
