import os
import csv
import math
import numpy as np
import xml.etree.ElementTree as ET
import random

quadrantSize = 30

def gethashmapkey(x, y):
    return math.floor(x / float(quadrantSize)) + 1000 * math.floor(y / float(quadrantSize))


os.chdir(r'E:\step9\Documents\unity_projects\ECS-pandemic-simulation\Assets\Conf\Maps')
file = "Turin_FullMap.tmx"
tree = ET.parse(file)
root = tree.getroot()
# parsing using the string.
layer = root.find("layer")

width = int(layer.get("width"))
height = int(layer.get("height"))
print(width)
print(height)
data = layer.find("data")
# printing the root.
csvtext = data.text

lines = csvtext.split('\n')
# print(lines)
curline = height
array2dmap = np.zeros([height, width], int)
for line in lines:

    if curline == height:
        curline -= 1
        continue

    values = line.split(',')
    i = int(0)
    for item in values:
        if len(item) != 0:
            if item[0] != '\r\n' and item[0] != ' ' and item[0] != '  ':
                array2dmap[curline, i] = item
                i += 1



    curline -= 1

currentkey = -1
for x in range(0, width, quadrantSize):
    for y in range(0, height, quadrantSize):
        hashmapkey = gethashmapkey(x, y)
        if(hashmapkey == currentkey):
            print("errore quadranti")
            print(hashmapkey,x,y)
            exit(1)
        counters = np.zeros([9], int)
        for j in range(x, x+quadrantSize):
            if(j == width):
                break
            for k in range(y, y+quadrantSize):
                if k == height:
                    break
                currentkey = gethashmapkey(j, k)
                if currentkey == hashmapkey: #SE SONO NELLO STESSO QUADRANTE
                    if array2dmap[k, j] == 9: #HOME
                        counters[0] += 1
                    if array2dmap[k, j] == 2: #PARK
                        counters[1] += 1
                    if array2dmap[k, j] == 11: #GYM
                        counters[2] += 1
                    if array2dmap[k, j] == 1: #PUB
                        counters[3] += 1
                    if array2dmap[k, j] == 10: #HOSPITAL
                        counters[4] += 1
                    if array2dmap[k, j] == 12: #SCHOOL
                        counters[5] += 1
                    if array2dmap[k, j] == 3: #OFFICE
                        counters[6] += 1
                    if array2dmap[k, j] == 8: #SUPERMARKET
                        counters[7] += 1
                    if array2dmap[k, j] == 6: #HOME
                        counters[8] += 1

        if(counters[0] > 0 or counters[8] > 0): #Se ci sono case, ci sono agenti
            for i in range(len(counters)):
                if(counters[i] == 0):
                    randx = random.randint(x, j -1 )
                    randy = random.randint(y, k -1)
                    while array2dmap[randy,randx] != 14 and array2dmap[randy,randx] != 9:
                        randx = random.randint(x, j-1 )
                        randy = random.randint(y, k-1 )


                    if(i == 1):
                        array2dmap[randy,randx] = 2
                    if (i == 2):
                        array2dmap[randy, randx] = 11
                    if (i == 3):
                        array2dmap[randy, randx] = 1
                    if (i == 4):
                        array2dmap[randy, randx] = 10
                    if (i == 5):
                        array2dmap[randy, randx] = 12
                    if (i == 6):
                        array2dmap[randy, randx] = 3
                    if (i == 7):
                        array2dmap[randy, randx] = 8

text = ""
for h in range(height-1, -1, -1):
    for m in range(width):
        if m == width-1:
            text += str(array2dmap[h, m]) + ',' + '\r\n'
        else:
            text += str(array2dmap[h, m]) + ','

text = '\r\n' + text
text = text[:-3]
data.text = text
data.set('updated', 'yes')
tree.write("Turin_FullMap2.tmx")

