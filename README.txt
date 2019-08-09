Complexity of Approximation and Human Decision Making
Unity 3D (C#) code.

Problems used in this program (from lowest to highest difficulty):
- FPTAS: Weight Constrained Shortest Path Problem (WCSPP) (instances start with "w")
- PTAS: Minimum Metric/Euclidean Travelling Salesman Problem (MTSP) (instances start with "m")
- Not in APX: Minimum Travelling Salesman Problem (TSP) (instances start with "t")

Game controls:
- Type your ID and click enter.
- Type your Randomisation ID (number, 1 to 50) and click enter.
- Click Start button or press space bar in order to start.
- Click on cities to select a path.
- Press UP arrow to submit answer.

Input and Output files stored in Assets/StreamingAssets
- time-param.txt sets the Time-related parameters
- Randomisation files and problem instances are stored in the "Instances" subfolder
- Randomisation files are called x_param2.txt where x is a number between 1 and 50.
- Instances start with a letter representing the problem type and followed by a number between 1 and 24.
- e.g. "t1" for the first TSP instance; "w20" for the 20th WCSPP instance

Here are the reasons why a click might be invalid....
TSP:
3=have already selected this city

WCSPP:
4=must click the 'Start' city first
5=have already selected the 'Start' city
6=have already reached the destination
7=have already selected this city
8=Weight Limit Exceeded
9=Invalid path, the two cities cannot be connected directly
10=BEWARE: this should not occur... any other error is a 10.