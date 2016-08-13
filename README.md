# UWPChip8

Working Chip-8 interpreter / emulator running under the Universal Windows Platform with Win2D

The core (Chip8.Core) is a PCL project which can be copied into any solution that supports a PCL compliant project such as Xamarin Forms etc and then just implement your own renderer and input method, in the case of XF for example you could render the cpu's displaybuffer to a Bitmap encoded image and display it as a source to an Image element. Note that input handling is another thing entirely - this would be the bulk of your code although relatively simple enough.


#### Description

This project demonstrates the CanvasAnimatedControl update cycle and drawing pixels as rectangles with DrawRectangle as well as picking files with FileOpenPicker.

Pull requests are always welcome.


Note that this does not include any Chip-8 ROMs. These can be had from many places, for example : http://www.zophar.net/pdroms/chip8.html


#### Keys

1, 2, 3, 4

q, w, e, r

a, s, d, f

z, x, c, v

You'll have to work out which is which per game. Generally '2' and 'w' are 'fire' and either side of that are directional input but don't hold me to that.


#### Credits

Joseph Weisbecker. 

CowGod's Technical Reference that this was based on : http://devernay.free.fr/hacks/chip8/C8TECH10.HTM

Alexander's superb JS Chip-8 Emulator : https://github.com/alexanderdickson/Chip-8-Emulator

David Winter : http://www.pong-story.com/chip8/

![Alt text](/Images/ibm.png?raw=true "IBM")

