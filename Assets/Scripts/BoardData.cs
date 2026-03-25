using System;
using UnityEngine;

[Serializable]
public class BoardSpaceData
{
    public string SpaceName;
    public BoardSpaceType SpaceType;

    [TextArea(2, 4)]
    public string Description;

    [Tooltip("Only used when SpaceType is Jump.")]
    public int JumpAmount;

    public BoardSpaceData()
    {
        SpaceName = "";
        SpaceType = BoardSpaceType.Go;
        Description = "";
        JumpAmount = 0;
    }

    public BoardSpaceData(string spaceName, BoardSpaceType spaceType)
    {
        SpaceName = spaceName;
        SpaceType = spaceType;
        Description = "";
        JumpAmount = 0;
    }

    public BoardSpaceData(string spaceName, BoardSpaceType spaceType, int jumpAmount)
    {
        SpaceName = spaceName;
        SpaceType = spaceType;
        Description = "";
        JumpAmount = jumpAmount;
    }
}