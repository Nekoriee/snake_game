using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SnakeBody
{
    public abstract Heading GetHeading();
    public abstract void SetHeading(Heading heading);

    public abstract Vector3 GetPosition();
    public abstract void SetPosition(Vector3 position);
    public abstract void UpdateSprite(SnakeState state);
}
