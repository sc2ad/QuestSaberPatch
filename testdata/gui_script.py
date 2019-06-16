import json
import os
import subprocess

# path_to_jsonapp2 = "../jsonApp2/jsonApp2.csproj"
path_to_jsonapp2 = "jsonApp2.exe"
d = "temp.json"
command = "dotnet run -p " + path_to_jsonapp2 +  " < " + d if path_to_jsonapp2.endswith(".csproj") else path_to_jsonapp2 + " < " + d

def getSongFolder(top_dir):
    if "info.dat" in os.listdir(top_dir):
        return top_dir
    return getSongFolder([os.path.join(top_dir, item) for item in os.listdir(top_dir) if os.path.isdir(os.path.join(top_dir, item))][0])

def getColorVal(item, c):
    _ = input("Input " + c + " color component for color" + item + " or press enter to use default colors: ")
    return float(_) if "" != _ else None

path_to_apk = input("Please enter the path to your APK: ")
assert os.path.exists(path_to_apk), "That path does not exist! Please try again."

inp = {}

# All songs need to already be converted, but that shouldn't really be too much of an issue.
# Just use the songeconverter or call it before this line
all_songs_folders = input("Please input the path to all of your songs: ")
assert os.path.exists(all_songs_folders), "That path does not exist! Please try again."

levels = {}
packs = []
pack = {}
pack['id'] = "CustomLevels"
pack['name'] = "Custom Songs"
pack['levelIDs'] = []

for folder in os.listdir(all_songs_folders):
    if not os.path.isdir(os.path.join(all_songs_folders, folder)):
        continue
    f = getSongFolder(os.path.join(all_songs_folders, folder))
    # print(f + " " + os.path.basename(f))
    levelID = os.path.basename(f)
    levels[levelID] = os.path.abspath(f)
    pack['levelIDs'].append(levelID)
    print("Added: " + levelID)
packs.append(pack)

pack['coverImagePath'] = input("Input the path to the cover image for the pack (.jpg, .png): ")
assert os.path.exists(pack['coverImagePath']), "That path does not exist! Please try again."

colors = {}
for item in ['A', 'B']:
    colors['color' + item] = {}
    for c in ['r', 'g', 'b', 'a']:
        v = getColorVal(item, c)
        if not v:
            colors['color' + item] = None
            break
        assert v >= 0 and v <= 1.0, "Color selection must be between 0 and 1!"
        colors['color' + item][c] = v

swapLanguage = {}

swapLanguage['swap'] = bool(input("Would you like to swap languages? (enter anything for yes): "))
if swapLanguage['swap']:
    swapLanguage['languageToSwapTo'] = input("Enter the language you would like to swap to: ")
else:
    swapLanguage['languageToSwapTo'] = "ENGLISH"

text = {}

while 1:
    k = input("Enter the key of the text you would like to replace (or press enter for nothing): ")
    if k == "":
        break
    v = input("Enter the value of the text you would like: ")
    if v == "":
        break
    text[k] = v

soundEffects = []

while 1:
    path = input("Enter the path to a custom sound effect or press enter to use default sounds: ")
    if path == "":
        break
    soundEffects.append(path)

inp['apkPath'] = path_to_apk
inp['patchSignatureCheck'] = True
inp['sign'] = True
inp['levels'] = levels
inp['packs'] = packs
inp['colors'] = colors
inp['replacementLanguage'] = "ENGLISH"
inp['swapLanguage'] = swapLanguage
inp['replaceText'] = text
inp['soundEffectFiles'] = soundEffects

# input_json = json.dumps(inp)
with open(d, 'w') as f:
    json.dump(inp, f)

# subprocess.call("dotnet run -p " + path_to_jsonapp2 + " < " + d)
print("\nRun the following command: " + command)