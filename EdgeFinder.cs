using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace CakeDev
{
    [RequireComponent(typeof(MeshCollider))]
    public class EdgeFinder : MonoBehaviour
    {
        [SerializeField] private Spline.Type defaultSplineType = Spline.Type.CatmullRom;
        [SerializeField] private string splineLayer = "Spline"; // Unity doesn't like setting layermasks during editor mode, leaving this as a string for now
        [SerializeField] private float ledgeColliderRadius = 0.1f;
        [SerializeField] private bool drawAllColoredVertices = false;
        [SerializeField] private bool drawContiguousPoints = false;
        [SerializeField] private bool drawLedgeMesh = false;
        [SerializeField] private Material debugMaterial;

        private static readonly string PREFIX = "EdgeFinder-Tool";

        private List<GameObject> GeneratedObjects => GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains(PREFIX) && t.gameObject != gameObject)
            .Select(t => t.gameObject)
            .ToList(); // I hate this, but I'm not sure how to handle object resets between Unity play mode -> editor mode (i.e. too lazy)

        private MeshFilter _meshFilter;
        private bool _started;
        private Vector3[] vertices;

        [Button("Generate Spline")] // Remove this if you don't have NaughtyAttributes asset
        public void GenerateSplines()
        {
            _meshFilter = GetComponent<MeshFilter>();
            ClearGeneratedObjects();

            Mesh mesh = _meshFilter.sharedMesh;
            vertices = mesh.vertices.Select(it => transform.TransformPoint(it)).ToArray();
            Color[] vertexColors = mesh.colors;

            // Find uniquely marked climbable edges:
            HashSet<Color> uniqueVertexColors = new HashSet<Color>(
                vertexColors.Distinct().Where(color => color != Color.white).ToList()
            );

            if (uniqueVertexColors.Count == 0)
            {
                Debug.LogError(
                    $"ERROR! GameObject={name} has no unique vertex colors were found on the mesh! Please make sure your mesh has at least 1 vertex-colored edge.",
                    this
                );
            }

            // Group edges by their vertex color:
            Dictionary<Color, List<Edge>> edgeLoops = new Dictionary<Color, List<Edge>>();
            foreach (Color color in uniqueVertexColors)
            {
                var edgeLoop = FindBoundaryEdges(mesh.triangles)
                    .Where(edge => vertexColors[edge.v1] == color && vertexColors[edge.v2] == color)
                    .ToList();
                edgeLoops.Add(color, edgeLoop);
            }

            // Go through each vertex-colored edge loop and draw them:
            foreach (var entry in edgeLoops)
            {
                GenerateEdgeLoopSpline(entry.Key, entry.Value);
            }
        }

        [Button("Draw Spline Tangents")]
        public void DrawTangents()
        {
            if (GeneratedObjects.Count == 0)
            {
                GenerateSplines();
            }

            List<SplineComputer> splines = GeneratedObjects
                .Select(it => it.GetComponent<SplineComputer>())
                .NotNull()
                .ToList();
            foreach (SplineComputer spline in splines)
            {
                SampleCollection samples = new SampleCollection();
                spline.GetSamples(samples);
                foreach (SplineSample sample in samples.samples)
                {
                    DebugUtilities.DrawArrow(sample.position, sample.forward);
                }
            }
        }

        [Button("Reset")]
        public void ClearGeneratedObjects()
        {
            foreach (GameObject go in GeneratedObjects)
            {
                DestroyImmediate(go);
            }

            GeneratedObjects.Clear();
        }

        private void GenerateEdgeLoopSpline(Color color, List<Edge> edges)
        {
            GameObject parent = new GameObject($"{PREFIX}.{ColorUtility.ToHtmlStringRGB(color)}");
            parent.transform.SetParent(transform);
            parent.transform.localPosition = Vector3.zero;

            if (drawAllColoredVertices)
            {
                int vIndex = 0;
                GameObject vertexGameObject = new GameObject("Vertices");
                vertexGameObject.transform.SetParent(parent.transform);
                foreach (var edge in edges)
                {
                    Vector3 p1 = vertices[edge.v1];
                    Vector3 p2 = vertices[edge.v2];
                    CreateSphere(p1, vIndex, debugMaterial, vertexGameObject.transform, "Vertex.p1");
                    CreateSphere(p2, vIndex, debugMaterial, vertexGameObject.transform, "Vertex.p2");
                    vIndex++;
                }
            }

            // Get all unique edges:
            HashSet<Edge> uniqueEdges = GetUniqueEdges(edges);

            // Build out contiguous points:
            LinkedList<Vector3> contiguousPoints = BuildContiguousPoints(uniqueEdges.ToList());

            if (drawContiguousPoints)
            {
                GameObject edgeVertexObject = new GameObject("Edges Vertices");
                edgeVertexObject.transform.SetParent(parent.transform);
                edgeVertexObject.transform.localPosition = Vector3.zero;
                int n = 0;
                foreach (Vector3 contiguousPoint in contiguousPoints)
                {
                    CreateSphere(contiguousPoint, n, debugMaterial, edgeVertexObject.transform);
                    n++;
                }
            }

            // Create a spline object using our contiguous points:
            GameObject splineObject = new GameObject("Spline");
            splineObject.layer = LayerMask.NameToLayer(splineLayer);
            splineObject.transform.SetParent(parent.transform);

            SplineComputer spline = splineObject.AddComponent<SplineComputer>();
            spline.type = defaultSplineType;
            SplinePoint[] points = contiguousPoints
                .Select(CreateSplinePoint)
                .ToArray();
            spline.SetPoints(points);

            // Create the edge colliders:
            Mesh edgeMesh = BuildEdgeMesh(splineObject);

            MeshCollider edgeCollider = splineObject.AddComponent<MeshCollider>();
            edgeCollider.convex = false;
            edgeCollider.sharedMesh = edgeMesh;

            if (drawLedgeMesh)
            {
                MeshFilter mf = splineObject.AddComponent<MeshFilter>();
                MeshRenderer mr = splineObject.AddComponent<MeshRenderer>();
                mr.materials = new[] {debugMaterial};
                mf.mesh = edgeMesh;
            }
        }

        private HashSet<Edge> GetUniqueEdges(List<Edge> edges)
        {
            HashSet<Edge> uniqueEdges = new();
            foreach (Edge edge in edges)
            {
                bool isUnique = true;
                foreach (Edge other in uniqueEdges)
                {
                    if (edge != other && !HasUniquePointsFrom(edge, other))
                    {
                        isUnique = false;
                    }
                }

                if (isUnique)
                {
                    uniqueEdges.Add(edge);
                }
            }

            return uniqueEdges;
        }

        private LinkedList<Vector3> BuildContiguousPoints(List<Edge> uniqueEdgesList)
        {
            HashSet<Edge> usedEdges = new HashSet<Edge>();
            LinkedList<Vector3> contiguousPoints = new LinkedList<Vector3>();

            Edge first = uniqueEdgesList[0];
            Vector3 start = vertices[first.v1];
            Vector3 end = vertices[first.v2];
            usedEdges.Add(first);
            contiguousPoints.AddFirst(start);
            contiguousPoints.AddLast(end);

            for (int i = 0; i < uniqueEdgesList.Count; i++)
            {
                Edge nextEdge = uniqueEdgesList[i];
                if (usedEdges.Contains(nextEdge)) continue;

                start = contiguousPoints.First.Value;
                end = contiguousPoints.Last.Value;

                Vector3 nextV1 = vertices[nextEdge.v1];
                Vector3 nextV2 = vertices[nextEdge.v2];

                if (start == nextV1)
                {
                    contiguousPoints.AddFirst(nextV2); // Check for overlaps with the starting point:
                    usedEdges.Add(nextEdge);
                    i = 0;
                }
                else if (start == nextV2) 
                {
                    contiguousPoints.AddFirst(nextV1);
                    usedEdges.Add(nextEdge);
                    i = 0;
                }
                else if (end == nextV1) // Check for overlaps with ending point:
                {
                    contiguousPoints.AddLast(nextV2);
                    usedEdges.Add(nextEdge);
                    i = 0;
                }
                else if (end == nextV2)
                {
                    contiguousPoints.AddLast(nextV1);
                    usedEdges.Add(nextEdge);
                    i = 0;
                }
            }

            return contiguousPoints;
        }

        private Mesh BuildEdgeMesh(GameObject parent)
        {
            TubeGenerator tubeGenerator = parent.AddComponent<TubeGenerator>();
            tubeGenerator.size = ledgeColliderRadius;
            tubeGenerator.sides = 6;
            tubeGenerator.capMode = TubeGenerator.CapMethod.Flat;

            Mesh tubeMesh = parent.GetComponent<MeshFilter>().sharedMesh;

            // Clean up TubeGenerator Components:
            DestroyImmediate(parent.GetComponent<TubeGenerator>());
            DestroyImmediate(parent.GetComponent<MeshRenderer>());
            DestroyImmediate(parent.GetComponent<MeshFilter>());

            return tubeMesh;
        }

        private bool HasUniquePointsFrom(Edge current, Edge other)
        {
            // if we overlap on the v1 point:
            if (vertices[current.v1] == vertices[other.v1])
            {
                return vertices[current.v2] != vertices[other.v2];
            }

            // if we overlap on the v2 point:
            if (vertices[current.v2] == vertices[other.v2])
            {
                return vertices[current.v1] != vertices[other.v1];
            }

            // if the v1 point overlaps the other v2 point:
            if (vertices[current.v1] == vertices[other.v2])
            {
                return vertices[current.v2] != vertices[other.v1];
            }

            // if the v2 point overlaps the other v1 point:
            if (vertices[current.v2] == vertices[other.v1])
            {
                return vertices[current.v1] != vertices[other.v2];
            }

            // If no points overlap, it's unrelated so default to true:
            return true;
        }

        private SplinePoint CreateSplinePoint(Vector3 position)
        {
            SplinePoint point = new SplinePoint();
            point.position = position;
            point.normal = Vector3.up;
            point.size = 1f;
            point.color = Color.white;

            return point;
        }

        // Credit for this goes to some dude I found unity forums:
        private List<Edge> FindBoundaryEdges(int[] triangles)
        {
            // edge(a,b) as key - value as unique flag
            var edges = new Dictionary<(int, int), bool>();

            for (var i = 0; i < triangles.Length; i += 3)
            {
                // technically not bounds safe but can safely
                // assume triangle count will be multiple of 3.
                var t0 = triangles[i];
                var t1 = triangles[i + 1];
                var t2 = triangles[i + 2];

                // always define edges with lowest value first so
                // we get predictable collisions in dictionary
                var e0 = t0 < t1 ? (t0, t1) : (t1, t0);
                var e1 = t1 < t2 ? (t1, t2) : (t2, t1);
                var e2 = t2 < t0 ? (t2, t0) : (t0, t2);

                // Can use indexer when writing but not reading hence ContainsKey.
                edges[e0] = !edges.ContainsKey(e0);
                edges[e1] = !edges.ContainsKey(e1);
                edges[e2] = !edges.ContainsKey(e2);
            }

            // filter on value (true if unique, false otherwise), then select edge tuple.
            return edges.Where(e => e.Value).Select(e => new Edge(e.Key.Item1, e.Key.Item2)).ToList();
        }

        private void CreateSphere(Vector3 position, int v1, Material material, Transform parent,
            string sphereName = "Vertex")
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"{sphereName}.{v1}";
            sphere.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() {material});
            sphere.transform.SetParent(parent);
            sphere.transform.position = position;
            sphere.transform.localScale *= 0.1f;
        }

        public class Edge
        {
            public int v1;
            public int v2;

            public Edge(int aV1, int aV2)
            {
                v1 = aV1;
                v2 = aV2;
            }
        }
    }
}