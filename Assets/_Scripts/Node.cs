using UnityEngine;

public class Node
{
    public int x, y;
    public Vector3 worldPosition;

    public override bool Equals(object obj)
    {
        if (obj is Node other)
        {
            return x == other.x && y == other.y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return x * 31 + y;
    }
}
