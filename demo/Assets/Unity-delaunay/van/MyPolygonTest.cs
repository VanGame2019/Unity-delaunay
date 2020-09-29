﻿using System;
using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using van.map;
using Random = UnityEngine.Random;

public class MyPolygonTest : MonoBehaviour
{
	[SerializeField]
	private int
		m_pointCount = 300;

	private List<Vector2> m_points;
	private float m_mapWidth = 100;
	private float m_mapHeight = 50;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private List<LineSegment> m_test;
	private List<Vector2> m_vector2;
	
	private List<List<Vector2>> m_regions;
	
	void Awake ()
	{
		Demo ();
	}

	void Update ()
	{
//		if (Input.anyKeyDown) {
//			Demo ();
//		}
		if (Input.GetMouseButtonDown(0))
		{
			var mousePosition = Input.mousePosition; //获取屏幕坐标
			mousePosition.z = -Camera.main.transform.position.z;
			var mouseWorldPos = Camera.main.ScreenToWorldPoint (mousePosition); //屏幕坐标转世界坐标

			var distance = float.MaxValue;
			MapCenter center = null;
			var p = new Vector2(mouseWorldPos.x,mouseWorldPos.y);
			foreach (var mapCenter in mapCenters)
			{
				var dis = Vector2.Distance(p, mapCenter.position);
				if (distance > dis)
				{
					distance = dis;
					center = mapCenter;
				}
			}
			Debug.Log(center.position);
		}
		
	}

	private Voronoi v;
	private void Demo ()
	{
				
		List<uint> colors = new List<uint> ();
		m_points = new List<Vector2> ();
			
		for (int i = 0; i < m_pointCount; i++) {
			colors.Add (0);
			m_points.Add (new Vector2 (
					UnityEngine.Random.Range (2, m_mapWidth-2),
					UnityEngine.Random.Range (2, m_mapHeight-2))
			);
		}
		v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = v.VoronoiDiagram ();
			
		m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation ();

		m_regions = v.Regions();

//		m_test = v.VoronoiBoundaryForSite(m_points[0]);
//		m_vector2 = v.HullPointsInOrder();
//		m_test = v.HullEdgesTest();
		m_test = v.Hull();

//		render();
		generateData();
		renderMapData();

//		var p = m_points[0];
//		var list_point = v.VoronoiBoundaryForSite(p);
//		foreach (var lineSegment in list_point)
//		{
//			Debug.Log($"({lineSegment.p0.Value.x},{lineSegment.p0.Value.y})({lineSegment.p1.Value.x},{lineSegment.p1.Value.y})");
//		}
//		
//		var center = getMapCenter(p);
//		var list_vec = center.edges;
//		foreach (var mapEdge in list_vec)
//		{
//			Debug.Log($"e({mapEdge.p0.x},{mapEdge.p0.y})({mapEdge.p1.x},{mapEdge.p1.y})");
//
//		}
//
//		var list_p =sortBound(list_vec);
//		foreach (var vector2 in list_p)
//		{
//			Debug.Log(vector2);
//		}
	}

	private Dictionary<Vector2, MapCenter> dic_centers;
	private Dictionary<(Vector2, Vector2), MapEdge> dic_MapEdge;
	private List<MapCenter> mapCenters;
	private List<MapEdge> mapEdges;
	private void generateData()
	{
		dic_centers = new Dictionary<Vector2, MapCenter>();
		dic_MapEdge = new Dictionary<(Vector2, Vector2), MapEdge>();
		
		mapCenters = new List<MapCenter>();
		mapEdges = new List<MapEdge>();

		foreach (var vec in m_points)
		{
			var p = vec;
			var center = getMapCenter(p);
			var list_p = v.NeighborSitesForSite(p);
			foreach (var vector21 in list_p)
			{
				center.neighbours.Add(getMapCenter(vector21));
			}

			var list_edge = v.VoronoiBoundaryForSite(p);
			foreach (var lineSegment in list_edge)
			{
				var p00 = lineSegment.p0;
				var p11 = lineSegment.p1;
				Vector2 p0 = p00 ?? Vector2.zero;
				Vector2 p1 = p11 ?? Vector2.zero;
				var edge = getMapEdge(p0, p1);
				if (edge != null)
				{
					center.edges.Add(edge);
					edge.belongs.Add(center);
				}
				else
				{
					center.isOcean = true;
				}
			}
			mapCenters.Add(center);
		}

	}

	private MapCenter getMapCenter(Vector2 v)
	{
		if (!dic_centers.ContainsKey(v))
		{
			dic_centers.Add(v,new MapCenter(v));
		}
		return dic_centers[v];
	}

	private MapEdge getMapEdge(Vector2 p0, Vector2 p1)
	{
		if (p0.Equals(Vector2.zero) || p1.Equals(Vector2.zero))
			return null;
		var key = (p0, p1);
		if (!dic_MapEdge.ContainsKey(key))
		{
			dic_MapEdge.Add(key,new MapEdge(p0,p1));
		}
		return dic_MapEdge[key];
	}

	private List<Vector2> sortBound(List<MapEdge> list)
	{
		list = new List<MapEdge>(list);
		Vector2 findNext(Vector2 point,List<MapEdge> candidates)
		{
			foreach (var candidate in candidates)
			{
				if (candidate.p0.MyEquals(point))
				{
					candidates.Remove(candidate);
					return candidate.p1;
				}

				if (candidate.p1.MyEquals(point))
				{
					candidates.Remove(candidate);
					return candidate.p0;
				}
			}
			return Vector2.zero;
		}

		List<Vector2> list_result = new List<Vector2>();
		var begin = list[0].p0;
		var start = begin;
		var next = list[0].p1;
		list_result.Add(start);
		list_result.Add(next);
		list.Remove(list[0]);
		do
		{
			next = findNext(next,list);
			if(next.Equals(Vector2.zero))
				break;
			list_result.Add(next);
		} while (list.Count>1);
		
		
		return list_result;
	}

	//筛选最外圈的
	private void checkOutMost(MapCenter mapCenter)
	{
		//去除已经是海的
		if (mapCenter.isOcean)
			return;
		
		var list = new List<MapEdge>(mapCenter.edges);

		//去除边角数量小于2的
		if (list.Count <= 2)
		{
			mapCenter.isOcean = true;
			return;
		}

		//去除有边角在外边沿上的
		foreach (var mapCenterEdge in mapCenter.edges)
		{
			float[] arr = new[] {0, m_mapHeight, m_mapHeight};
			foreach (var f in arr)
			{
				if (mapCenterEdge.p0.AnyMatch(f) || mapCenterEdge.p1.AnyMatch(f))
				{
					mapCenter.isOcean = true;
					return;
				}
			}
		}
		
		
		Vector2 findNext(Vector2 point,List<MapEdge> candidates)
		{
			foreach (var candidate in candidates)
			{
				if (candidate.p0.MyEquals(point))
				{
					candidates.Remove(candidate);
					return candidate.p1;
				}

				if (candidate.p1.MyEquals(point))
				{
					candidates.Remove(candidate);
					return candidate.p0;
				}
			}
			return Vector2.zero;
		}

		List<Vector2> list_result = new List<Vector2>();
		var begin = list[0].p0;
		var start = begin;
		var next = list[0].p1;
		list_result.Add(start);
		list_result.Add(next);
		list.Remove(list[0]);
		do
		{
			next = findNext(next,list);
			if(next.Equals(Vector2.zero))
				break;
			list_result.Add(next);
		} while (list.Count>1);

		if (list.Count > 1)
		{
			mapCenter.isOcean = true;
		}
		else
		{
			var last = list[0];
			if (!last.p0.MyEquals(begin) &&!last.p1.MyEquals(begin))
			{
				mapCenter.isOcean = true;
			}
		}
	}
	
	private void renderMapData()
	{
		foreach (var mapCenter in mapCenters)
		{
			var center = mapCenter;
			checkOutMost(center);
			if(center.isOcean)
				continue;
			Color c = Random.ColorHSV(0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f);
			var list = center.edges;
			var list_vec = sortBound(list);
			DrawTool.DrawPolygon(list_vec, c);
		}
	}
	
	private void render()
	{
		foreach (var mRegion in m_regions)
		{
			var list = mRegion;
			Color c = Random.ColorHSV(0.0f, 1f, 0.0f, 1f, 0.0f, 1f, 0.5f, 0.5f);
			DrawTool.DrawPolygon(list, c);
		}
	}
	
	
	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red;
		if (m_points != null) {
			for (int i = 0; i < m_points.Count; i++) {
				Gizmos.DrawSphere (m_points [i], 0.2f);
			}
		}

		if (m_test != null) {
			Gizmos.color = Color.green;
			for (int i = 0; i< m_test.Count; i++)
			{
				LineSegment seg = m_test[i];
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
		return;
		
		if (m_vector2 != null)
		{
			Gizmos.color = Color.black;
			for (int i = 0; i < m_vector2.Count-1; i++)
			{
                Gizmos.DrawLine ((Vector3)m_vector2[i], (Vector3)m_vector2[i+1]);
			}
		}

		return;
		if (m_edges != null) {
			Gizmos.color = Color.white;
			for (int i = 0; i< m_edges.Count; i++) {
				break;
				Vector2 left = (Vector2)m_edges [i].p0;
				Vector2 right = (Vector2)m_edges [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}

		Gizmos.color = Color.magenta;
		if (m_delaunayTriangulation != null) {
			for (int i = 0; i< m_delaunayTriangulation.Count; i++) {
				break;
				Vector2 left = (Vector2)m_delaunayTriangulation [i].p0;
				Vector2 right = (Vector2)m_delaunayTriangulation [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}

		if (m_spanningTree != null) {
			Gizmos.color = Color.green;
			for (int i = 0; i< m_spanningTree.Count; i++) {
				LineSegment seg = m_spanningTree [i];				
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
	}
}