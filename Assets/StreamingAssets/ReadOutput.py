import os

script_dir = os.path.dirname(__file__)

in_path = "Input\Instances\\1_param2.txt"

abs_in_path = os.path.join(script_dir, in_path)

f = open(abs_in_path,'r+')

print(f.read())
