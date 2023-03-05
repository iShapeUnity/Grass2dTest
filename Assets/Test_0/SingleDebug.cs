using iShape.Grass2d;
using iShape.Mesh2d;
using iShape.Spline;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Test_0 {
    
    [ExecuteInEditMode]
    public class Single : MonoBehaviour {

        [Range(0.05f, 1)]
        public float Width = 0.1f;
        
        [Range(1, 20)]
        public int Count = 6;
        
        [Range(0.01f, 0.5f)]
        public float StrokeWidth = 0.02f;
        
        [Range(0, 1)]
        public float Elasticity = 0.1f;
        
        [Range(0, 1)]
        public float Friction = 0.9f;
        
        [Range(0, 180)]
        public float MaxAngle = 90f;
        public Vector2 wind = new Vector2(0, 0);
        
        public Vector2 head = new Vector2(1, 4);
        public Vector2 root = new Vector2(0, 0);

        private Mesh mesh;
        private NativeColorMesh colorMesh;

        private Stalk stalk;

        private void Awake() {
#if UNITY_EDITOR
            OnEditorAwake();
#else
            OnPlayAwake();    
#endif
        }
        private void Update() {
#if !UNITY_EDITOR
    OnPlayUpdate();
#endif
        }

        private void OnDestroy() {
#if !UNITY_EDITOR
    OnPlayDestroy();
#endif
        }

        private void OnPlayAwake() {
            mesh = new Mesh();
            mesh.MarkDynamic();
            this.GetComponent<MeshFilter>().mesh = mesh;
            
            colorMesh = new NativeColorMesh(256, Allocator.Persistent);
            
            var matrix = new Matrix(this.transform.localToWorldMatrix);

            float length = 1.2f * (head - root).magnitude;
            stalk = new Stalk(matrix, head, root, new float2(0, 1), length, Width);            
        }
        
        private void OnEditorAwake() {
            var meshRenderer = this.GetComponent<MeshRenderer>();
            if (meshRenderer == null) {
                meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            }

            if (meshRenderer.sharedMaterial == null) {
                meshRenderer.sharedMaterial = Resources.Load<Material>("VerColor2d");    
            }

            var meshFilter = this.GetComponent<MeshFilter>();
            if (meshFilter == null) {
                this.gameObject.AddComponent<MeshFilter>();
            }
        }

        private void OnPlayUpdate() {
            var constitution = new PhysicData(Friction, Elasticity, Mathf.Deg2Rad * MaxAngle, true);
            var environment = new Environment(float2.zero, wind);

            var context = new StepContext(transform.localToWorldMatrix, Time.deltaTime, constitution.friction);
            
            stalk.Update(context, environment, constitution);

            // target
            var headMesh = MeshGenerator.Circle(stalk.targetHead, 0.25f, 24, 0.1f, Allocator.Temp);
            var stalkMesh = MeshGenerator.StrokeForEdge(stalk.targetHead, stalk.root, new StrokeStyle(0.2f), 0.0f, Allocator.Temp);

            colorMesh.Clear();
            
            // colorMesh.Add(headMesh, new Color(0.025f, 0.025f,0.025f, 0.5f));
            // colorMesh.Add(stalkMesh, new Color(0.01f, 0.01f,0.01f,0.5f));

            // real
            var rHeadMesh = MeshGenerator.Circle(stalk.head, 0.25f, 24, 0.1f, Allocator.Temp);
            var rStalkMesh = MeshGenerator.StrokeForEdge(stalk.head, stalk.root, new StrokeStyle(0.2f), 0.0f, Allocator.Temp);
            
            // colorMesh.Add(rHeadMesh, new Color(0.8f, 0.025f,0.025f, 1f));
            // colorMesh.Add(rStalkMesh, new Color(0.8f, 0.01f,0.01f,1f));
            
            // grass
            var spline = new CubicSpline(stalk.root, stalk.head, stalk.anchor);
            var points = spline.GetPoints(10, Allocator.Temp);
            var grassMesh = MeshGenerator.StrokeByPath(points, false, new StrokeStyle(0.2f), 0.1f, Allocator.Temp);
            
            // colorMesh.Add(grassMesh, new Color(0.3f, 0.7f,0.3f, 1f));
            
            // Mesh
            var shapeColor = new Color(0, 1, 0, 0.6f);
            var strokeColor = new Color(1, 0.5f, 0, 1);
            var drawStyle = DrawStyle.Linear( shapeColor, strokeColor, StrokeWidth, Count, Allocator.Temp); 
            var drawData = stalk.BuildMesh(drawStyle, Allocator.Temp);
            var strokeMesh = MeshGenerator.StrokeByPath(drawData.strokePath, false, new StrokeStyle(drawStyle.strokeWidth), 0, Allocator.Temp); 
            
            colorMesh.Add(drawData.shapeMesh, drawStyle.color);
            colorMesh.AddAndDispose(strokeMesh, drawStyle.strokeColor);
            
            drawData.Dispose();
            
            colorMesh.Fill(mesh);
        }

        private void OnPlayDestroy() {
            colorMesh.Dispose();
        }

    }

}
