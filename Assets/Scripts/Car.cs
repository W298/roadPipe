using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Car : MonoBehaviour
{
    [Serialize] public List<Road> path = new List<Road>();
    public int currentRoadIndex = 0;
    public int currentPointIndex = 0;

    public Point start;
    public Point destination;

    public void PathFind()
    {
        Cell prev = start;
        Cell current = start.GetAdjacentCell()[0];
        while (current is not Point || (Point)current != destination)
        {
            path.Add((Road)current);
            var adj = current.GetAdjacentCell().Where(cell => cell != prev).ToArray();
            if (adj.Length == 0) break;
            prev = current;
            current = adj[0];

            Debug.Log(prev.isConnected(current).index.Item1 + " / " + prev.isConnected(current).index.Item2);
        }

        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        
        yield return new WaitForSeconds(1f);
        /*
        transform.position = path[currentRoadIndex].wayPoint.Item1.points[currentPointIndex].transform.position;

        currentPointIndex++;
        if (currentPointIndex >= path[currentRoadIndex].wayPoint.Item1.points.Length)
        {
            currentRoadIndex++;
            currentPointIndex = 0;
        }
        if (currentRoadIndex < path.Count) StartCoroutine(Move());
        */
    }
}
