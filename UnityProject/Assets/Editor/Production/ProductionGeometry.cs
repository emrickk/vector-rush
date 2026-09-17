using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush.Editor
{
    // Authoring geometry is persisted by ProductionSceneSetup; no generator runs in the player.
    public sealed class ProductionGeometry
    {
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<Vector2> uv = new List<Vector2>();
        readonly List<int> triangles = new List<int>();

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float tile = 4)
        {
            int n = vertices.Count;
            vertices.AddRange(new[] { a, b, c, d });
            float w = Vector3.Distance(a,b)/tile, h = Vector3.Distance(b,c)/tile;
            uv.AddRange(new[] { Vector2.zero, new Vector2(w,0), new Vector2(w,h), new Vector2(0,h) });
            triangles.AddRange(new[] { n,n+1,n+2,n,n+2,n+3 });
        }

        public void Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int n=vertices.Count;vertices.AddRange(new[]{a,b,c});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.up});triangles.AddRange(new[]{n,n+1,n+2});
        }

        public void Box(Vector3 center, Vector3 size, Quaternion rotation)
        {
            Vector3 x=rotation*Vector3.right*size.x*.5f, y=rotation*Vector3.up*size.y*.5f, z=rotation*Vector3.forward*size.z*.5f;
            Quad(center-x-y-z,center-x+y-z,center+x+y-z,center+x-y-z);
            Quad(center+x-y+z,center+x+y+z,center-x+y+z,center-x-y+z);
            Quad(center-x-y+z,center-x+y+z,center-x+y-z,center-x-y-z);
            Quad(center+x-y-z,center+x+y-z,center+x+y+z,center+x-y+z);
            Quad(center-x+y-z,center-x+y+z,center+x+y+z,center+x+y-z);
            Quad(center-x-y+z,center-x-y-z,center+x-y-z,center+x-y+z);
        }

        public void Beam(Vector3 a, Vector3 b, float width, float depth)
            => Box((a+b)*.5f,new Vector3(width,depth,Vector3.Distance(a,b)),Quaternion.LookRotation(b-a));

        public void Cylinder(Vector3 center,float radius,float height,int sides=32)
        {
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                Vector3 p=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius),q=new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius);
                Vector3 y=Vector3.up*height*.5f;
                Quad(center+p-y,center+p+y,center+q+y,center+q-y);
                Triangle(center+y,center+q+y,center+p+y);
                Triangle(center-y,center+p-y,center+q-y);
            }
        }

        public Mesh Mesh(string name)
        {
            var mesh=new Mesh {name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }

        public static Mesh Ribbon(TrackPath track,float left,float right,float height,bool faceDown=false)
        {
            if(left>right){float swap=left;left=right;right=swap;}
            int n=track.SmoothRoad?3840:960;int columns=right-left>5?12:1,stride=columns+1;
            var vertices=new Vector3[(n+1)*stride];var uv=new Vector2[vertices.Length];var triangles=new int[n*columns*6];
            for(int i=0;i<=n;i++)
            {
                var f=track.Evaluate(i/(float)n);
                for(int j=0;j<=columns;j++)
                {
                    float across=j/(float)columns;int a=i*stride+j;
                    vertices[a]=f.Position+f.Right*Mathf.Lerp(left,right,across)+f.Up*height;
                    uv[a]=new Vector2(across,i/(float)n*240f);
                    if(i==n||j==columns)continue;
                    int k=(i*columns+j)*6;
                    triangles[k]=a;triangles[k+1]=a+stride;triangles[k+2]=a+1;
                    triangles[k+3]=a+1;triangles[k+4]=a+stride;triangles[k+5]=a+stride+1;
                }
            }
            if(faceDown)for(int i=0;i<triangles.Length;i+=3){int swap=triangles[i+1];triangles[i+1]=triangles[i+2];triangles[i+2]=swap;}
            var mesh=new Mesh{name="Persistent running profile",vertices=vertices,uv=uv,triangles=triangles};
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }

        public static Mesh Barrier(TrackPath track,int side)
        {
            int n=track.SmoothRoad?3840:960;var v=new Vector3[(n+1)*4];var uv=new Vector2[v.Length];var t=new List<int>();
            for(int i=0;i<=n;i++)
            {
                var f=track.Evaluate(i/(float)n);
                for(int j=0;j<4;j++){float x=(j<2?11.7f:12.1f)*side,y=j==0||j==3?-.3f:2;
                    v[i*4+j]=f.Position+f.Right*x+f.Up*y;uv[i*4+j]=new Vector2(j,i*.2f);}
                if(i==n)continue;
                for(int j=0;j<4;j++){int a=i*4+j,b=i*4+(j+1)%4,c=a+4,d=b+4;
                    t.AddRange(side>0?new[]{a,c,b,b,c,d}:new[]{a,b,c,b,d,c});}
            }
            var mesh=new Mesh{name="Persistent collision barrier",vertices=v,uv=uv,triangles=t.ToArray()};
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }
    }
}
