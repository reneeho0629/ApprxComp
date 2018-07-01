TSP: Travelling salesman Task. Unity 3D (C#) code.

Game controls:
- Type your ID and click enter.
- Click space bar in order to start.
- Click on cities to select a path.
- Press UP arrow to go to answer submission screen.
- Press LEFT / RIGHT Arrow to choose an answer.


Input/Output data folders are located in Assets/DataInf. This folder has to be added manually to the game after building. 

This is the structure of the folder:
- DataInf
	- Output
	- Input 
		- layoutParam.txt
		- param.txt
		- Instances
			- i1.txt
			- i2.txt 
				…
			- 1_param2.txt
			- 2_param2.txt
				…

Description of INPUT files:


Input Files: param.txt, n_param2.txt(n=1,2,…), layoutParam.txt, Instances/i1.txt…

The main structure of these files is: 
NameOfTheVariable1:Value1
NameOfTheVariable2:Value2
…

layoutParam.txt
Relevant Parameters for the layout of the screen. All Variables must be INTEGERS.
columns:=number of columns in the grid were to lay randomly the items.
rows:= number of rows in the grid were to lay randomly the items.

param.txt
Relevant Parameters of the task. All Variables must be INTEGERS or vectors of INTEGERS.
timeRest1:=Time for the inter-trial Break.
timeRest2:=Time for the inter-blocks Break.
timeQuestion:=Time given for each trial (The total time the items are shown.
timeAnswer:=Time given to answer.

param2.txt 
Variables can be allocated between param.txt and param2.txt with no effect on the game; however there must not be repeated definitions of variables. The distinction is done because param2.txt is an output from the instance selection program (e.g python).
numberOfInstances:=Number of instances to be imported. The files uploaded are 			automatically i1-i”numberOfInstances”
numberOfBlocks:=Number of blocks.
numberOfTrials:=Number of trials in each block.
instanceRandomization:=Sequence of instances to be randomised. The vector must have length: 	trials*blocks. E.g. [1,3,2,3,1,3,1,2,3,1] for 2 blocks of 5 trials.


Instances/i1.txt,Instances/i2.txt,…
Instance information. Each file is a different instance of the problem. 
Files must be added sequentially (i.e. 1,2,3,…). Except for “param” all Variables must not be floats (i.e. integers, strings…)


cities:[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19]
coordinatesx:[208,890,260,12,994,756,515,445,313,37,357,387,346,176,335,492,618,595,977,834]
coordinatesy:[716,194,132,997,620,337,796,722,83,382,490,408,623,991,662,360,778,738,639,207]
ncities:20
maxdistance:4695
distancevector:[0,858,586,342,791,666,317,237,641,375,270,356,166,276,138,455,414,387,772,806,858,0,633,1189,438,195,709,690,587,873,609,546,692,1070,725,431,644,618,453,57,586,633,0,899,881,536,711,618,72,335,370,303,498,863,535,325,738,692,878,578,342,1189,899,0,1051,994,541,512,962,615,613,698,501,164,465,797,644,637,1029,1140,791,438,881,1051,0,369,510,558,867,986,650,642,648,898,660,565,407,416,25,442,666,195,536,994,369,0,518,494,510,720,427,375,499,874,531,265,462,432,374,151,317,709,711,541,510,518,0,101,741,632,344,408,241,391,224,436,104,98,487,669,237,690,618,512,558,494,101,0,652,531,248,319,140,380,125,365,181,150,538,645,641,587,72,962,867,510,741,652,0,406,409,333,541,918,579,329,758,713,866,535,375,873,335,615,986,720,632,531,406,0,337,350,391,624,408,455,703,661,974,815,270,609,370,613,650,427,344,248,409,337,0,87,133,532,173,187,388,343,637,554,356,546,303,698,642,375,408,319,333,350,87,0,218,620,259,115,436,390,633,490,166,692,498,501,648,499,241,140,541,391,133,218,0,405,40,300,313,274,631,641,276,1070,863,164,898,874,391,380,918,624,532,620,405,0,365,705,490,489,874,1023,138,725,535,465,660,531,224,125,579,408,173,259,40,365,0,340,305,270,642,675,455,431,325,797,565,265,436,365,329,455,187,115,300,705,340,0,436,391,559,374,414,644,738,644,407,462,104,181,758,703,388,436,313,490,305,436,0,46,384,610,387,618,692,637,416,432,98,150,713,661,343,390,274,489,270,391,46,0,394,582,772,453,878,1029,25,374,487,538,866,974,637,633,631,874,642,559,384,394,0,455,806,57,578,1140,442,151,669,645,535,815,554,490,641,1023,675,374,610,582,455,0]
solution:1
param:1.05

