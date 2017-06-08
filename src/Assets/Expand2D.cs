using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expand2D : MonoBehaviour
{
    public PolygonCollider2D polygonCol;
    public Camera cam = null;

    public List<Vector2> edgePoints = new List<Vector2>() {
        new Vector2(1,0), new Vector2(1,1), new Vector2(0, 1),
        new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1),
        new Vector2(0, -1), new Vector2(1, -1) };

    public float GrowMultiplier = 2.0f;

    void Start()
    {
        if (!polygonCol) polygonCol = GetComponent<PolygonCollider2D>();
        if (!cam) cam = Camera.main;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
        { // Grow

            // Closest Edge
            Vector2 worldMouse = cam.ScreenToWorldPoint(Input.mousePosition);

            int near, far;
            WorldToClosestEdgeIndices(worldMouse, out near, out far);

            int target = far;

            if (!IsEdgeWithinDistanceThreshold(2, near, far))
            {
                SplitEdge(near, far);
            }

            Vector2 localMouse = transform.InverseTransformPoint(worldMouse);
            Vector2 dir = (localMouse - edgePoints[target]).normalized;

            edgePoints[target] += dir * Time.deltaTime * GrowMultiplier;

            if (IsEdgeWithinDistanceThreshold(0.3f, near, far))
            {
                dir = (edgePoints[far] - edgePoints[near]).normalized;
                edgePoints[target] += dir * Time.deltaTime * GrowMultiplier;
                //JoinEdge(near, far);
            }

            if (IsEdgeWithinDistanceThreshold(0.25f, near, far))
            {
                JoinEdge(near, far);
            }
            polygonCol.SetPath(0, edgePoints.ToArray());
        }
    }

    void OnDrawGizmos()
    {
        if (edgePoints.Count < 2) return;

        // Edges
        Gizmos.color = Color.blue;
        for (int e = 1; e < edgePoints.Count; e++)
        {
            Gizmos.DrawLine(transform.TransformPoint(edgePoints[e - 1]), transform.TransformPoint(edgePoints[e]));
        }
        int last = edgePoints.Count - 1;

        Gizmos.DrawLine(transform.TransformPoint(edgePoints[0]), transform.TransformPoint(edgePoints[last]));

        // Closest Edge
        Vector2 worldMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        int near, far;
        WorldToClosestEdgeIndices(worldMouse, out near, out far);
        Vector2 p1 = transform.TransformPoint(edgePoints[near]);
        Vector2 p2 = transform.TransformPoint(edgePoints[far]);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(p1, p2);

        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(worldMouse, 0.25f);

    }

    static Vector2 ClosestPointOnLine(Vector2 _p1, Vector2 _p2, Vector2 _point)
    {
        Vector2 line = (_p2 - _p1);
        float len = line.magnitude;
        line.Normalize();

        Vector2 v = _point - _p1;
        float d = Vector3.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);
        return _p1 + line * d;
    }

    void ClosestEdge(int _nearIndex, Vector2 _worldPoint, out int _farIndex)
    {
        Vector2 localpoint = transform.InverseTransformPoint(_worldPoint);

        // Left
        int lSiblingVertex = ShiftLower(_nearIndex);
        Vector2 lClosest = ClosestPointOnLine(edgePoints[lSiblingVertex], edgePoints[_nearIndex], localpoint);
        float lDstSqr = SqrDistance(lClosest, localpoint);

        //Right 
        int rSiblingVertex = ShiftHigher(_nearIndex);
        Vector2 rClosest = ClosestPointOnLine(edgePoints[rSiblingVertex], edgePoints[_nearIndex], localpoint);
        float rDstSqr = SqrDistance(rClosest, localpoint);

        // Found edge
        _farIndex = rDstSqr < lDstSqr ? rSiblingVertex : lSiblingVertex;
    }

    void WorldToClosestEdgeIndices(Vector2 _point, out int _close, out int _far)
    {
        int closestVertex = -1;
        Vector2 localPoint = transform.InverseTransformPoint(_point);
        // scan all vertices to find nearest
        float minDistanceSqr = Mathf.Infinity;
        for (int v = 0; v < edgePoints.Count; v++)
        {
            float distSqr = SqrDistance(edgePoints[v], localPoint);
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closestVertex = v;
            }
        }

        // Found closest
        _close = closestVertex;

        ClosestEdge(closestVertex, _point, out _far);
    }

    bool IsEdgeWithinAngleThreshold(float _degreeThreshold, int _firstIndex, int _secondIndex)
    {
        if (Vector2.Angle(edgePoints[_firstIndex].normalized, edgePoints[_secondIndex].normalized) < _degreeThreshold) return true;
        return false;
    }

    bool IsEdgeWithinDistanceThreshold(float _sqrDistanceThreshold, int _firstIndex, int _secondIndex)
    {
        if (SqrDistance(edgePoints[_firstIndex], edgePoints[_secondIndex]) <= _sqrDistanceThreshold) return true;
        return false;
    }

    int WorldToClosestVertexIndex(Vector2 _point)
    {
        int closestVertex = -1;

        // scan all vertices to find nearest
        float minDistanceSqr = Mathf.Infinity;
        for (int v = 0; v < edgePoints.Count; v++)
        {
            float distSqr = SqrDistance(edgePoints[v], _point);
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closestVertex = v;
            }
        }

        // Found closest
        return closestVertex;
    }

    int SplitEdge(int _near, int _far)
    {
        Debug.Log("Split!");

        Vector2 mid = Vector2.Lerp(edgePoints[_near], edgePoints[_far], 0.5f);

        int index = Mathf.Max(_near, _far);
        if (index == edgePoints.Count - 1 && Mathf.Min(_near, _far) == 0)
            edgePoints.Add(mid);
        else
            edgePoints.Insert(index, mid);

        return index;
    }

    void JoinEdge(int _near, int _far)
    {
        int highIndex = Mathf.Max(_near, _far);
        int lowIndex = ShiftLower(highIndex);

        Vector2 mid = Vector2.Lerp(edgePoints[lowIndex], edgePoints[highIndex], 0.5f);

        edgePoints.RemoveAt(highIndex);
        edgePoints.RemoveAt(lowIndex);
        edgePoints.Insert(lowIndex, mid);
        Debug.Log("Join!");
    }

    int ShiftLower(int _index)
    {
        return _index - 1 == -1 ? edgePoints.Count - 1 : _index - 1;
    }

    int ShiftHigher(int _index)
    {
        return _index + 1 == edgePoints.Count ? 0 : _index + 1;
    }


    static float SqrDistance(Vector2 _from, Vector2 _to)
    {
        return (_to - _from).sqrMagnitude;
    }

}
