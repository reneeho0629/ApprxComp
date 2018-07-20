from random import shuffle

x = [i for i in range(1,25)]
print(x)

y = ["t", "w", "m"]
print(y)

for j in range(1, 3):
    f = open("%r_param2.txt" % j,"w+")

    shuffle(y)
    probs = "problemOrder:[" + ",".join(char for char in y) + "]\n"
    print(probs)
    f.write(probs)

    f.write("numberOfTrials:8\n")
    f.write("numberOfBlocks:3\n")

    shuffle(x)
    tsp = "tspRandomization:[" + ",".join(str(num) for num in x) + "]\n"
    print(tsp)
    f.write(tsp)

    shuffle(x)
    wcspp = "wcsppRandomization:[" + ",".join(str(num) for num in x) + "]\n"
    print(wcspp)
    f.write(wcspp)

    shuffle(x)
    mtsp = "mRandomization:[" + ",".join(str(num) for num in x) + "]"
    print(mtsp)
    f.write(mtsp)

    f.close()
