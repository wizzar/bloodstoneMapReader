# bloodstoneMapReader
Reads bloodstone minimap files and exports them as PNG.

## Structure

### File info
Every minimap file begins with the file coordinates in the world map (4 bytes, being 2 for X and 2 for Y) and the floor (1 byte for Z).

### Exemple
The file minimap_270401_7.map begins with the bytes `10 04 20 04 07`.
The `X` coordinate is `0x0410`, the Y coordinate is `0x0420`, and the floor is `07`.

### Pixels
After the first 5 bytes we begin reading the colors for each pixel, starting from left to right, top to bottom.
Each pixel is composed of 3 bytes of information.

(NOT SURE, BUT LOOKS CORRECT) The first byte indicates the type of the tile. The types are `01` (can't walk), `04` (unexcplored), `05` (can walk), and `07` (objects that changes player's elevation, and all sorts of stairs, holes, etc.)

The second byte indicates the color of the pixel. The value corresponds to a unique color value stored on a table (colorTable.bin file). There are 251 possible colors.

The meaning of the third byte is unknown at the moment, but it's probably related to some sort of flag for special objects or textures on a tile. Here is a list of known third bytes:

FF
64
78
82
7D
6E
87
8C
B4
C8
96
FE
FA
9D
F5


### The color table
I was able to dig out a table of colors from the executable and find a pattern in how they are loaded. Each entry on the table is 24 bytes long, but only a few of the bytes represents the actual color.
Here is a snippet:

`01 00 00 00 | FF FF 00 00 | 00 00 00 00 | 00 00 00 00 | 90 67 D5 05 | 01 00 00 00 |`

`Unknown.....| al al re re | gr gr bl bl | Unknown..................................`

For some reason the colors are 64-bits, even though they are converted to 32-bit rgb in the game.
