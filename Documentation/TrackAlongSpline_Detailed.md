# TrackAlongSpline Documentation

### UML Diagram
![Track Along Spline Generator (UML Diagram)](UML_Diagrams/TrackAlongSplineGenerator_UML.png)

#### UML Key
- **Blue**   -  Driver Class
- **Green**  -  ScriptableObject
- **Yellow** -  GameObject Creation Class
- **Grey**   -  Data Container Class

---

#### Function Descriptions

---

- **MeshData**
`Static data container`

MeshData objects are used to store values generated during procedural generation. 

---

- **TrackConstraintsData**
`Static data container`

TrackConstraintsData objects are used to store the constraints that define the geometry of generated track vertices.

---

- **TrackEndcapData**
`Active data container`

TrackEndcapData objects require a reference to an instance of TrackConstraintsData during its instantiation. Values
from TrackConstraintsData are used to define the geometry of the endcap. 
*GenerateEndcapAtPoint(worldPosition: Vector3, forward: Vector3, up: Vector3)* Function parameters are used to define 
the orientation and world position of the endcap. This function will generate and store all MeshData fields associated 
with the rail and base mesh components of the endcap at the given point.
For any given Endcap GameObject created, this class should only be called once to populate this data. 

---

- **TrackRingsData**
`Active data container`

TrackRingsData objects require a reference to an instance of TrackConstraintsData during its instantiation. Values
from TrackConstraintsData are used to define the geometry of each ring of vertices. 
**GenerateRingAtPoint(worldPosition: Vector3, forward: Vector3, up: Vector3) : void** Function parameters are used to define 
the orientation and world position of a ring at the given point. This function will be called at every interval in which 
the spline is being sampled to create each ring of a track segment. 

---

- **TrackEndcap**
`GameObject creation class`

TrackEndcap objects require references to all materials and MeshData instances required to generate an Endcap GameObject.
**Generate() : GameObject** Creates and returns a new Endcap GameObject with materials and mesh filters defined by the MeshData 
fields, with the corresponding materials applied. Collision is then assigned for all generated meshes.

---

- **TrackSegment**
`GameObject creation class`

TrackSegment objects require references to all materials and MeshData instances required to generate a TrackSegment GameObject.
**Generate() : GameObject** Creates and returns a new TrackSegment GameObject with materials and mesh filters defined by the MeshData 
fields, with the corresponding materials applied. Collision is then assigned for all generated meshes.

---

- **TrackConfig : ScriptableObject**
`ScriptableObject`

Only one TrackConfig asset exists in the project. Values stored in this asset can be changed in the inspector to set 
preferred default specifications for track geometry. This script is referenced by default by all track generation driver classes
to maintain consistent track geometry among all tools and all track related GameObjects. Users can set _useConfig to false in the
inspector for any generation script to customize individual tracks without altering other tracks in their project. 

---

- **TrackAlongSplineGenerator : MonoBehaviour**
`Driver class`

TrackAlongSplineGenerator acts as the orchestrator of track generation along a user-defined spline. By default, serialized fields
will be set to the constraints defined in TrackConfig. The user has the option to disable this by setting _useConfig to false. 
This will allow the user to define track geometry and materials manually that differ from the global default for tracks for full
customization. 

The bool GenerateTrack is available in the editor and acts as a button for the user to tell this class to create a new track along
the spline. Once pressed, this script will destroy any track components that it had previously generated. The script will then perform 
the following:

It will check if the user has disabled generation of either the start or end Endcap by checking the _generateStartEndcap and 
_generateEndEndcap fields in the inspector. For all expected endcaps, the script will create an instance of TrackEndcapData and 
GenerateEndcapAtPoint at either the start or end of the spline as needed. For each TrackEndcapData, the corresponding MeshData will 
then be used to create an instance of TrackEndcap. TrackEndcap.Generate() will then be called for each instance of TrackEndcap and 
the created GameObject(s) are stored in generatedGameObjects.

Then it will determine the number of TrackSegments to generate based on TrackConstraintsData. 

NOTE: To ensure performance and avoid unintended behavior, each track segment will consist of no more than 6000 vertices. This means 
that, during any track generation, any number of TrackSegment objects may be created to accommodate this. 

For each required track segment there will be a corresponding instance of TrackRingsData. This script will traverse the spline and 
sample the world position, forward, and up vectors at intervals defined by the TrackConstraintsData. At each point sampled, TrackRingsData 
will be called to compile MeshData at each point. Once a given TrackSegment's data has been compiled, the script will create a new instance
of TrackSegment and generate the required segment GameObject. This process will continue until the end of the spline is reached. 
