PixelCatcher - Custom Easter Egg Sprites
========================================

PixelCatcher can run animated 2D pixel-art sprites across pinned screenshot windows.
You can place your own custom animated sprites into this folder.

Folder Location:
  %APPDATA%\PixelCatcher\Sprites
  (You can open this folder anytime from the PixelCatcher tray icon menu -> "Open Sprites Folder...")

How it works:
1. Create a transparent PNG image that contains a horizontal strip of animation frames.
   Example: If each animation frame is 32x32 pixels and you have 4 frames, the image will be 128x32 pixels.
2. Save your PNG file with any name, for example: "my_runner.png".
3. Create a matching JSON file with the exact same base name: "my_runner.json".
4. Copy both files into this Sprites folder.

JSON Configuration Options:
---------------------------
- "frameWidth"     : (integer) Width of each frame in pixels (default: height of image).
- "frameHeight"    : (integer) Height of each frame in pixels (default: height of image).
- "numberOfFrames" : (integer) Total number of frames in the horizontal strip.
- "moveSpeed"      : (integer) Speed in pixels per tick at which the sprite moves across windows (default: 5).
- "frameSpeed"     : (integer) Animation playback speed (default: 5).
                     * 1 to 39: Interpreted as frames per second (e.g. 8 = 8 fps).
                     * 40 or greater: Interpreted as milliseconds per frame (e.g. 100 = 100ms per frame).

Supported Alternate Field Names:
- Frame Width: frameWidth, frame_width, width
- Frame Height: frameHeight, frame_height, height
- Frames: numberOfFrames, number_of_frames, frameCount, frames
- Movement Speed: moveSpeed, move_speed, movespeed, movementSpeed, speed
- Animation Speed: frameSpeed, frame_speed, framespeed, animationSpeed, animSpeed, speed

See "template.json" for a minimal configuration example.
