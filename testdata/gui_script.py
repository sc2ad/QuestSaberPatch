import json
import os
import subprocess

def getSongFolder(top_dir):
    if "info.dat" in os.listdir(top_dir):
        return top_dir
    return getSongFolder([item for item in os.listdir(top_dir) if os.path.isdir(os.path.join(top_dir, item))][0])

def getColorVal(item, c):
    _ = input("Input " + c + " color component for color" + item + " or press enter to use default colors")
    return _ if "\n" != _ else None

inp = {}

apk = "path/to/apk"
# All songs need to already be converted, but that shouldn't really be too much of an issue.
# Just use the songeconverter or call it before this line
all_songs_folders = input("Please input the path to all of your songs: ")
assert os.path.exists(all_songs_folders), "That path doesn't exist! Please try again."

levels = {}
packs = {}
packs['id'] = "CustomLevels"
packs['name'] = "Custom Songs"
packs['coverImagePath'] = input("Input the path to the cover image for the pack (.jpg, .png): ")
packs['levelIDs'] = []
assert os.path.exists(packs['coverImagePath'])

for folder in all_songs_folders:
    levelID = os.path.dirname(getSongFolder(folder))
    levels[levelID] = getSongFolder(folder)
    packs['levelIDs'].append(levelID)

colors = {}
for item in ['A', 'B']:
    colors['color' + item] = {}
    for c in ['r', 'g', 'b', 'a']:
        v = getColorVal(item, c)
        if not v:
            colors['color' + item] = None
            break

text = {}

while 1:
    k = input("Enter the key of the text you would like to replace (or press enter for nothing): ")
    if k == "\n":
        break
    v = input("Enter the value of the text you would like: ")
    if v == "\n":
        break
    text[k] = v

soundEffects = []

while 1:
    path = input("Enter the path to a custom sound effect or press enter to use default sounds: ")
    if path == "\n":
        break
    soundEffects.append(path)

inp['apkPath'] = apk
inp['patchSignatureCheck'] = True
inp['sign'] = True
inp['levels'] = levels
inp['packs'] = packs
inp['colors'] = colors
inp['replaceText'] = text
inp['soundEffectFiles'] = soundEffects

input_json = json.dumps(inp)

subprocess.call("path/to/jsonApp2.exe", input_json)