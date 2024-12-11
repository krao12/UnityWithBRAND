import subprocess
'''
This is essentially a "dummy" BRAND node, since we are not inherting the BRANDNode
class or interacting with redis directly. This "node" exclusively launches
the built executable of the unity game (see build.sh). In the supergraph,
under the parameter section of this node, you can include relevant game parameters
or redis input streams within this node. Upon theinitialization of a BRANDNode 
in Unity, a method called in the constructor will parse the supergraph, 
look for a specific node (passed in via constructor), and store all parameters.
This is essentially how Game Params can be edited and sent to the UnityGame via the
BRAND GUI
'''
def launch_unity_executable():
    # first build the Unity Game using Build.cs which can be found in Assets/Editor of the unity game
    # replace the file path below with one that points the executable
    executable_path = '-- insert path here --'

    result = subprocess.run([executable_path], capture_output=True, text=True)

if __name__ == "__main__":
   launch_unity_executable()
   #a = 1;
