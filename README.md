YourVRUI
--------

This plugin will allow the programmers who have to deal with UI elements in VR to ease their lifes 
to bring their screen and interfaces to the VR world.

This plugin is more about to enjoy a readable code that follow some long text instructions, 
so I'll keep it short.

If you get really stuck somewhere along the way get in touch with me in the email address: esteban@yourvrexperience.com

TUTORIAL
--------
You can check video the tutorials here:

https://www.youtube.com/playlist?list=PLPtjK_bez3T7E7yiXsYs0Ro9nShG1dBAP

1-Create an empty project

2-Download and import the lastest Google VR SDK (https://developers.google.com/vr/unity/download)

3-Import the package YourVRUI.unitypackage

4-Load the Google Demo scene "GoogleVR\Demos\Scenes\GVRDemo"

5-Place in the scene the singleton "YourVRUI\prefabs\YourVRUIScreenController"

6-Place the example objects in scene "YourVRUI\prefabs\Example_Objects"

7-Add the script "PlayerMovementController" to the GameObject with the camera called "Player"

8-Play run an get close to the objects in order to trigger their dialogs

	-The safebox, the picture and Michael UI elements are automatically triggered by distance
	-The lamp and the girl UI elements are triggered when you are inside its range looking at them and you press action button

9-With the keys "O" and "P" ("Start","Options" in Xbox Controller)  you will make a appear an inventory screen 
	where you will be able to interact with scrollrect items

INPUTS
------

-Move the player: AWSD Move/Controller/Touchpad Daydream
-Navigate through UI elements: Gaze/Laser Daydream/Arrow Keys/Digital Keypad/Touchpad Daydream
-Action buttons: 
	ACTION: "CTRL" (PC), "A" BUTTON XBOX CONTROLLER, "ACTION" DAYDREAM
	CANCEL: "BACKSPACE","ESC" (PC), "B" BUTTON XBOX CONTROLLER, "BACK" DAYDREAM

XBOX CONTROLLER
---------------

Some controls should be defined through "Edit->Project Settings->Input"

http://www.yourvrexperience.com/ProjectSettingsInputRightStick.png

Name: RightHorizontal
Type: Joystick Axis
Axis: 4th axis

Name: RightVertical
Type: Joystick Axis
Axis: 5th axis

http://www.yourvrexperience.com/ProjectSettingsInputDigitalKeypad.png

Name: DigitalHorizontal
Type: Joystick Axis
Axis: 6th axis

Name: DigitalVertical
Type: Joystick Axis
Axis: 7th axis

	
COMPONENTS
----------

YourVRController: Main singleton responsible of the creation of the screen in the VR world, 
				it's fully parametrizable to allow multiple configurations.

PlayerRaycasterController: You should attach this script to your camera in order to detect
				the GameObject that should display an UI element.

PlayerMovementController (THIS IS A DEBUG CLASS FOR DEVELOPMENT PURPOSES, YOU DON'T NEED IT IN YOUR PROJECT)

KeyEventInputController: Class that manages the key input, just customize it to your needs, but remember that
				the we are using the digital joysticks/arrow keys to navigate through the elements inside
				the screens.

InteractionController: This is the script you have to attach to the object in the world in order
				for the system to display a UI element.

