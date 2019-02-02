﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    Mesh _mesh;
    // 顶点的列表
    List<Vector3> vertices;
    // 三角的列表
    List<int> triangles;
    // 颜色表
    List<Color> colors;

    MeshCollider meshCollider;

    private void Awake() {
        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        _mesh.name   = "Hex Mesh";
        vertices     = new List<Vector3>();
        triangles    = new List<int>();
        colors       = new List<Color>();
    }

    // 三角化
    public void Triangulate(HexCell[] cells)
    {
        _mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        _mesh.vertices  = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.colors    = colors.ToArray();
        _mesh.RecalculateNormals();

        meshCollider.sharedMesh = _mesh;
    }

    private void Triangulate(HexCell cell)
    {
        // Vector3 center = cell.transform.localPosition;
        // for (int i = 0; i < HexMetrics.corners.Length; i++)
        // {
        //     int curIndex = i;
        //     int nextIndex = i + 1 == HexMetrics.corners.Length ? 0 :  i + 1;
        //     AddTriangle(
        //         center,
        //         center + HexMetrics.corners[curIndex],
        //         center + HexMetrics.corners[nextIndex]
        //     );
        //     AddTriangleColor(cell.color);
        // }
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    // private void Triangulate(HexDirection direction, HexCell cell)
    // {
    //     /**
    //         --d---f--
    //          \|   |/
    //           b---c
    //            \ /
    //             a
    //      */ 
    //     Vector3 center = cell.transform.localPosition;
    //     Vector3 a = center;
    //     Vector3 b = center + HexMetrics.GetFirstSolidCorner(direction);
    //     Vector3 c = center + HexMetrics.GetSecondSolidCorner(direction);

    //     AddTriangle(a, b, c);
    //     AddTriangleColor(cell.color);

    //     Vector3 bridge = HexMetrics.GetBridge(direction);
    //     Vector3 d = b + bridge;
    //     Vector3 f = c + bridge;

    //     AddQuad(b, c, d, f);

    //     HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
    //     HexCell neighbor     = cell.GetNeighbor(direction) ?? cell;
    //     HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;

    //     Color bridgeColor = (cell.color + neighbor.color) * 0.5f;
    //     AddQuadColor(cell.color, bridgeColor);

    //     // 填充空隙
    //     // 1. 第一个三角形
    //     AddTriangle(b, center + HexMetrics.GetFirstCorner(direction), d);
    //     AddTriangleColor(
    //         cell.color,
    //         (cell.color + prevNeighbor.color + neighbor.color) / 3f,
    //         bridgeColor
    //     );
    //     // 2. 第二个三角形
    //     AddTriangle(c, f, center + HexMetrics.GetSecondCorner(direction));
    //     AddTriangleColor(
    //         cell.color,
    //         bridgeColor,
    //         (cell.color + neighbor.color + nextNeighbor.color) / 3f
    //     );
    // }


    private void Triangulate(HexDirection direction, HexCell cell)
    {
        /**
            --d---f--
             \|   |/
              b---c
               \ /
                a
         */ 

        Vector3 center = cell.transform.localPosition;
        Vector3 a = center;
        Vector3 b = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 c = center + HexMetrics.GetSecondSolidCorner(direction);

        AddTriangle(a, b, c);
        AddTriangleColor(cell.color);

        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, b, c);
        }

    }

    // 三角化连接处的两个桥组成的长方形
    private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 b, Vector3 c)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
        {
            return;
        }
        Vector3 bridge = HexMetrics.GetBridge(direction);

        Vector3 d = b + bridge;
        Vector3 f = c + bridge;

        d.y = f.y = neighbor.Elevation * HexMetrics.elevationStep;

        TriangulateEdgeTerraces(b, c, cell, d, f, neighbor);

        // AddQuad(b, c, d, f);
        // AddQuadColor(cell.color, neighbor.color);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 e = c + HexMetrics.GetBridge(direction.Next());
            e.y = nextNeighbor.Elevation * HexMetrics.elevationStep;
            AddTriangle(c, f, e);
            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
    }

    private void TriangulateEdgeTerraces(Vector3 beginLeft, Vector3 beginRight, HexCell beginCell, Vector3 endLeft, Vector3 endRight, HexCell endCell)
    {

        Vector3 d = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 f = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        Color color = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

        AddQuad(beginLeft, beginRight, d, f);
        AddQuadColor(beginCell.color, color);


        AddQuad(d, f, endLeft, endRight);
        AddQuadColor(color, endCell.color);
    }


    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {

        Debug.DrawLine(a, b, Color.black);
        Debug.DrawLine(b, c, Color.black);
        Debug.DrawLine(c, a, Color.black);

        int vertexIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    private void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    private void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }


    private void AddQuad(Vector3 b, Vector3 c, Vector3 d, Vector3 f)
    {
        /**
            d-----f
            |\    |
            | \   |
            |  \  |
            |   \ |
            |    \|
            b-----c             
         */
        Debug.DrawLine(b, c, Color.black);
        Debug.DrawLine(c, d, Color.black);
        Debug.DrawLine(d, f, Color.black);
        Debug.DrawLine(f, b, Color.black);
        int vertexIndex = vertices.Count;
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        vertices.Add(f);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    private void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);   
        colors.Add(c2);   
    }

}
