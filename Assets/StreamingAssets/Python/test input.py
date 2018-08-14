dist = input("Distance matrix: ")

soln = input("Solution: ")

dist_list = [int(i) for i in dist.split(',')]
soln_list = [int(i) for i in soln.split('-')]


#print(dist_list, soln_list)

total_dist = 0

for i in soln_list[:-1]:
    next_soln = soln_list[soln_list.index(i)+1]

    dist_btn = dist_list[next_soln+(i-1)*int((len(dist_list)**0.5))-1]
    
    print("dist from %r to %r is %r" % (i, next_soln, dist_btn))
    total_dist += dist_btn

print("Total distance is: ", total_dist)
