using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
// This describes the number of buffer elements that should be reserved
// in chunk data for each instance of a buffer. 
[InternalBufferCapacity(500)]
public struct PathPosition : IBufferElementData
{

    public int2 position;

}
