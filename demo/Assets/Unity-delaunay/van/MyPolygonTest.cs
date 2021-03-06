﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Delaunay;
using Delaunay.Geo;
using ETModel;
using TMPro;
using UnityEngine.SceneManagement;
using van;
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
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

	public LineRenderer lineRenderer;
	public TextMeshPro textMeshPro;
	public GameObject goName;
	
	private Dictionary<Vector2, MapCenter> dic_centers;
	private Dictionary<(Vector2, Vector2), MapEdge> dic_MapEdge;
	private List<MapCenter> mapCenters;
	private List<MapEdge> mapEdges;
	private void generateData()
	{
		NoisyEdge.init();
		
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

		//去除最外圈
		foreach (var mapCenter in mapCenters)
		{
			checkOutMost(mapCenter);
		}
		
		generateZongmen();
	}

	private void generateZongmen()
	{
		var list = new List<MapCenter>(this.mapCenters);
		list = list.Where((c) => !c.isOcean).ToList();
		
		//生成大宗门
		list = MyModelTool.RandomList(list);
		
		int count_big = Mathf.CeilToInt(list.Count / 20f);
		for (int i = 0; i < count_big; i++)
		{
			list[i].rank = eCenterRank.Big;
		}
		var list_big = list.Take(count_big).ToArray();
		
		//生成小宗门 大家族 
		list = list.Skip(count_big).ToList();
		list = MyModelTool.RandomList(list);
		int count_mid = Mathf.CeilToInt(list.Count / 4f);
		for (int i = 0; i < count_mid; i++)
		{
			list[i].rank = eCenterRank.Mid;
		}

		//生成村落 小家族
		//剩下都是 默认为小
		
		//为除大宗门外的其他进行上级分配
		//先中级

		var list_mid = list.Take(count_mid).ToList();
		foreach (var mid in list_mid)
		{
			var minDst = float.MaxValue;
			foreach (var big in list_big)
			{
				var distance = Vector2.Distance(mid.position, big.position);
				if (distance < minDst)
				{
					minDst = distance;
					mid.boss = big;
				}
			}
			mid.boss.underList.Add(mid);
		}
		
		//再低级
		var list_small = list.Skip(count_mid).ToList();
		foreach (var small in list_small)
		{
			var minDst = float.MaxValue;
			foreach (var mid in list_mid)
			{
				var distance = Vector2.Distance(small.position, mid.position);
				if (distance < minDst)
				{
					minDst = distance;
					small.boss = mid;
				}
			}
			small.boss.underList.Add(small);
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

	private List<MapEdge> getOutLine(List<MapCenter> list)
	{
		Dictionary<MapEdge,int> dic_edges = new Dictionary<MapEdge, int>();

		foreach (var mapCenter in list)
		{
			foreach (var mapCenterEdge in mapCenter.edges)
			{
				if (dic_edges.ContainsKey(mapCenterEdge))
				{
					dic_edges[mapCenterEdge]++;
				}
				else
				{
					dic_edges.Add(mapCenterEdge,1);
				}
			}
		}

		List<MapEdge> list_edge = new List<MapEdge>();
		foreach (var item in dic_edges)
		{
			if (item.Value == 1)
			{
				list_edge.Add(item.Key);
			}
		}

		return list_edge;
	}

	private MapCenter getCenterCenter(List<MapCenter> list,MapCenter belong)
	{
		Bounds bounds = new Bounds(list[0].position,Vector3.one);
		for (int i = 1; i < list.Count; i++)
		{
			bounds.Encapsulate(list[i].position);
		}

		belong.bounds = bounds;		
		
		var centerPoint = bounds.center;
		MapCenter center = list[0];
		var minDst = float.MaxValue;
		foreach (var mapCenter in list)
		{
			var distance = Vector2.Distance(centerPoint, mapCenter.position);
			if (distance < minDst)
			{
				minDst = distance;
				center = mapCenter;
			}

		}
		
		return center;
	}

	private List<Vector2> getFontPosition(List<MapCenter> list, MapCenter belong, int num)
	{
		List<Vector2> list_result = new List<Vector2>();
		
		Bounds bounds = new Bounds(list[0].position,Vector3.one);
		for (int i = 1; i < list.Count; i++)
		{
			bounds.Encapsulate(list[i].position);
		}

		belong.bounds = bounds;		
		
		var centerPoint = bounds.center;
		var size = bounds.size;
		var cut = num;
		var isX = size.x >= size.y;
		if (isX)
		{
			var gap = size.x / cut;
			var xStart = centerPoint.x - size.x / 2;
			for (int i = 0; i < num; i++)
			{
				var xLeft = xStart + i * gap;
				var xRight = xStart + (i + 1) * gap;

				Bounds bounds_part = new Bounds(Vector3.zero, Vector3.one);
				for (int j = 0; j < list.Count; j++)
				{
					var cent = list[j];
					if (cent.position.x >= xLeft && cent.position.x <= xRight)
					{
						if (bounds_part.center == Vector3.zero)
						{
							bounds_part = new Bounds(cent.position,Vector3.one);
						}
						else
						{
							bounds_part.Encapsulate(cent.position);
						}
					}
				}
				if(bounds_part.center != Vector3.zero)
				list_result.Add(bounds_part.center);
			}
		}
		else
		{
			var gap = size.y / cut;
			var xStart = centerPoint.y - size.y / 2;
			for (int i = num-1; i >= 0; i--)
			{
				var xLeft = xStart + i * gap;
				var xRight = xStart + (i + 1) * gap;

				Bounds bounds_part = new Bounds(Vector3.zero, Vector3.one);
				for (int j = 0; j < list.Count; j++)
				{
					var cent = list[j];
					if (cent.position.y >= xLeft && cent.position.y <= xRight)
					{
						if (bounds_part.center == Vector3.zero)
						{
							bounds_part = new Bounds(cent.position,Vector3.one);
						}
						else
						{
							bounds_part.Encapsulate(cent.position);
						}
					}
				}
				if(bounds_part.center != Vector3.zero)
				list_result.Add(bounds_part.center);
			}
		}

		//平均中间的字的位子
		if (isX)
		{
			if (list_result.Count > 2)
			{
				var gap = list_result[list_result.Count - 1].x - list_result[0].x;
				gap /= list_result.Count - 1;
				for (int i = 1; i < list_result.Count-1; i++)
				{
					list_result[i] = new Vector2(list_result[0].x + gap * i,list_result[i].y);
				}
			}
		}
		else
		{
			if (list_result.Count > 2)
			{
				var gap = list_result[list_result.Count - 1].y - list_result[0].y;
				gap /= list_result.Count - 1;
				for (int i = 1; i < list_result.Count-1; i++)
				{
					list_result[i] = new Vector2(list_result[i].x,list_result[0].y + gap * i);
				}
			}
		}
		
		return list_result;
	}
	private List<string> list_country_names = new List<string>(){
		"越国","天罗国","紫金国","风都国","列元国","十三族盟","吴国","火梁国","碧平国","幻楚国","西韩国","黄周国","仙齐帝国",
		"什么国","冬临国","大金国","乱星海"};
	private void renderMapData()
	{
		var list_country_name_temp = new List<string>(list_country_names);
		foreach (var mapCenter in mapCenters)
		{
			var center = mapCenter;
			if(center.isOcean)
				continue;
			Color c = Random.ColorHSV(0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0.5f, 0.5f);
			var list = center.edges;

			foreach (var mapEdge in list)
			{
				var noisyEdge = NoisyEdge.getNoisyEdge(mapEdge);
				var listP0 = new List<Vector2>(noisyEdge.path0);
				var listP1 = new List<Vector2>(noisyEdge.path1);

				var l0 = new List<Vector2>(listP0);
				var l1 = new List<Vector2>(listP1);
				l0.Add(center.position);
				l1.Add(center.position);
				DrawTool.DrawPolygon(l0, mapCenter.color);
				DrawTool.DrawPolygon(l1, mapCenter.color);
				
//				var line = Instantiate(this.lineRenderer, Vector3.zero,Quaternion.identity);
//				var list_v3 = Array.ConvertAll<Vector2, Vector3>(listP0.ToArray(), v => v);
//				line.positionCount = listP0.Count;
//				line.SetPositions(list_v3);
//				
//				line = Instantiate(this.lineRenderer, Vector3.zero,Quaternion.identity);
//				list_v3 = Array.ConvertAll<Vector2, Vector3>(listP1.ToArray(), v => v);
//				line.positionCount = listP1.Count;
//				line.SetPositions(list_v3);
			}
		}

		var list_big = mapCenters.Where((c) => c.rank == eCenterRank.Big).ToList();
		var list_mid = mapCenters.Where((c => c.rank == eCenterRank.Mid)).ToList();
		var list_small = mapCenters.Where((c) => c.rank == eCenterRank.Small).ToList();

		this.gizmosCenter = new List<Vector2>();
		foreach (var big in list_big)
		{
			var list_all = big.getAllUnderlist();
			
			var outline_big = getOutLine(list_all);
//			var outline_mid = getOutLine(list_mid).Except(outline_big).ToList();
//			var outline_small = getOutLine(list_small).Except(outline_mid).Except(outline_big).ToList();

			foreach (var mapEdge in outline_big)
			{
				var noisyEdge = NoisyEdge.getNoisyEdge(mapEdge);
				var listP0 = new List<Vector2>(noisyEdge.path0);
				var listP1 = new List<Vector2>(noisyEdge.path1);

				var line = Instantiate(this.lineRenderer, Vector3.zero,Quaternion.identity);
				line.transform.localScale = Vector3.one*4f;
				var list_v3 = Array.ConvertAll<Vector2, Vector3>(listP0.ToArray(), v => v);
				line.positionCount = listP0.Count;
				line.SetPositions(list_v3);
				
				line = Instantiate(this.lineRenderer, Vector3.zero,Quaternion.identity);
				line.transform.localScale = Vector3.one*4f;
				list_v3 = Array.ConvertAll<Vector2, Vector3>(listP1.ToArray(), v => v);
				line.positionCount = listP1.Count;
				line.SetPositions(list_v3);
			
			}


			var center = getCenterCenter(list_all,big);
			this.gizmosCenter.Add(center.position);
			
			var name = MyModelTool.RandomElement(list_country_name_temp);
			list_country_name_temp.Remove(name);

			var name_count = name.Length;
			
			var list_font = getFontPosition(list_all, big, name_count);
			if (list_font.Count != name_count)
			{
				var tmp = Instantiate(this.textMeshPro, Vector3.zero, Quaternion.identity);
				tmp.transform.position = center.position;
				tmp.fontSize = Mathf.Lerp(20, 50, (big.BoundsSize) / 20);
				tmp.text = name;
				tmp.color = center.colorWithoutAlpha;
			}
			else
			{
				
//				List<string> arr = new List<string>(){"国","家","名"};
				for (int i = 0; i < name.Length; i++)
				{
					var tmp = Instantiate(this.textMeshPro, Vector3.zero, Quaternion.identity);
					tmp.transform.position = list_font[i];
					tmp.fontSize = Mathf.Lerp(20, 50, (big.BoundsSize) / 20);
					tmp.text = name.Substring(i,1);
					tmp.color = center.colorWithoutAlpha;
				}
			}
			
			

//			var goname = Instantiate(this.goName, Vector3.zero, Quaternion.identity);
//			goname.transform.position = big.position;
//			goname.GetComponentInChildren<TextMeshPro>().text = "宗门";
		}

		foreach (var mid in list_mid)
		{
			var midGroup = mid.getAllUnderlist();
			
			var outline_all_big = getOutLine(list_big);
			var outline_all_mid = getOutLine(midGroup);
			var outline_toDraw = outline_all_mid.Distinct();
//			outline_toDraw = outline_toDraw.Except(outline_all_big).ToList();

			foreach (var mapEdge in outline_toDraw)
			{
				var noisyEdge = NoisyEdge.getNoisyEdge(mapEdge);
				var listP0 = new List<Vector2>(noisyEdge.path0);
				var listP1 = new List<Vector2>(noisyEdge.path1);

				var line = Instantiate(this.lineRenderer, Vector3.zero,Quaternion.identity);
				line.transform.localScale = Vector3.one*2f;
				var list_v3 = Array.ConvertAll<Vector2, Vector3>(listP0.ToArray(), v => v);
				line.positionCount = listP0.Count;
				line.SetPositions(list_v3);
				
				line = Instantiate(this.lineRenderer, Vector3.zero,Quaternion.identity);
				line.transform.localScale = Vector3.one*2f;
				list_v3 = Array.ConvertAll<Vector2, Vector3>(listP1.ToArray(), v => v);
				line.positionCount = listP1.Count;
				line.SetPositions(list_v3);
			}
		}
		
		

	}

	private List<Vector2> gizmosCenter;
	
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

		Gizmos.color = Color.magenta;
		if (gizmosCenter != null)
		{
			for (int i = 0; i < gizmosCenter.Count; i++)
			{
				Gizmos.DrawSphere(gizmosCenter[i],1f);
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