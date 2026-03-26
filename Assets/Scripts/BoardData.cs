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

    [Tooltip("Drag the invisible point object here.")]
    public Transform WorldPosition;

    public BoardSpaceData()
    {
        SpaceName = "";
        SpaceType = BoardSpaceType.Go;
        Description = "";
        JumpAmount = 0;
        WorldPosition = null;
    }
}