# FIP4DCS
DCS BIOS for the Saitek / Logitech Flight Instrument Panel

This program can connect to the DCS BIOS server and display HTML with C# enhanced (CSHTML) Websites and graphics onto the FIP from Logitech or Saitek.

This is a first Alpha, it has probably not all features and bugfixes needed but it does work with current DCS BIOS.

Take the gauges/Demo as an example in how to create websites for the FIP and the .json file in how to use the buttons and rotary knobs.
The demo displays the KA-50s vertical speed as a gauge and text, FPS of the FIP and can use the first 5 Buttons as an ABRIS Kontrol and the left rotary knob as the ABRIS rotary knob. The 6th button presses the ABRIS rotary button.

Requirements:
- A 64bit Windows 7/10 Operating system
- (optional) a Saitek/Logitech Flight Instrument Panel
- (optional, not with FIP) the current 64Bit drivers of the Logitech Flight Instrument Panel
- DCS BIOS in the Export.lua

Usage:
Install the FIP drivers and this Software.
If you want to use the FIP have it in an USB-Port of your choosing before starting the Software.
Start the Software, select the profile you want to use, then you can ether start the FIP or the Profile or both in whatever order.
As soon as DCS is started and you're running a module (not in pause) then you should be able to read the data in the preview and FIP.
the preview window works even if no FIP is installed.

ToDO: Insert manual to prepare graphics, cshtml and json files

The graphics work by ordering the files from the "background" up. _still_ in the name implies a static background (it can be omited) _rot_ on the other hand means that the image can be rotated by the corresponding DCS BIOS data in the fip.json.
The pointer image should be roughly half the backgrounds size, quadratic (meaning the height is the same as the width) and point to the 0 or idle position of the gauge. the _x_ and _y_ coordinates, which you can include in the image file name, are the top left corner of that image, in relation to the background images top left corner.
