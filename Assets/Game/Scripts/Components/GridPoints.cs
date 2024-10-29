using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridPoints
{
    static public List<Vector2> GetPointGrid(float offsetX, float offsetY, float regionWidth, float regionHieght, float pointSpacing)
    {
        List<Vector2> points = new List<Vector2>(); // can use arrays instead of lists
        float halfWidth = regionWidth / 2;
        float halfHeight = regionHieght / 2;
        for (float y = -halfHeight; y < halfHeight; y += pointSpacing)
        {
            for (float x = -halfWidth; x < halfWidth; x += pointSpacing)
            {
                Vector2 randomOffset = Random.insideUnitCircle * pointSpacing / 2;
                points.Add(new Vector2(x + offsetX, y + offsetY) + randomOffset);
            }
        }
        return points;
    }
}
