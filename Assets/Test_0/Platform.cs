using iShape.Mesh2d;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Test_0 {

    public class Platform : MonoBehaviour {

        public Vector2 size;
        public Color color = Color.gray;
        public bool isMoveX = true;
        public bool isMoveY = true;
        
        [Range(0.01f, 0.1f)]
        public float speedX = 0.02f;
        
        [Range(0.0f, 1.0f)]
        public float speedY = 0.5f;
        
        
        [Range(0.0f, 5.0f)]
        public float Ampl = 1.0f;
        
        private Vector3 vel = Vector3.zero;
    
        private void OnValidate() {
            var meshRenderer = this.GetComponent<MeshRenderer>();
            if (meshRenderer == null) {
                meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            }

            if (meshRenderer.sharedMaterial == null) {
                meshRenderer.sharedMaterial = Resources.Load<Material>("VerColor2d");    
            }

            var meshFilter = this.GetComponent<MeshFilter>();
            if (meshFilter == null) {
                meshFilter = this.gameObject.AddComponent<MeshFilter>();
            }

            var mesh = MeshGenerator.StrokeForRect(float2.zero, size, new StrokeStyle(0.1f), -0.01f, Allocator.Temp);
            meshFilter.mesh = mesh.Convert(color);
            vel = new Vector3(speedX, 0f, 0f);
        }
        
        void Start() {
            vel = new Vector3(speedX, 0f, 0f);
        }

        private void Update() {
            if (isMoveX || isMoveY) {
                var pos = this.transform.position;
            
                if (isMoveX) {
                    pos += vel;
                    if (pos.x > 6) {
                        pos.x = 6;
                        vel = new Vector3(-speedX, 0f, 0f);
                    } else if (pos.x < -6) {
                        pos.x = -6;
                        vel = new Vector3(speedX, 0f, 0f);
                    }
                }

                if (isMoveY) {
                    pos.y = Ampl * Mathf.Sin(speedY * Time.time);
                }
                this.transform.position = pos;
            }
        }
    }

}