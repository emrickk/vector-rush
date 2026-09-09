using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    // Metric material study on two existing road regions. No new road geometry or collision.
    public sealed class OpeningRoadFinish : MonoBehaviour
    {
        const int Segments=960,Columns=12,Stride=Columns+1;
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Texture2D> textures=new List<Texture2D>();
        WorldBuilder world;
        TrackPath track;
        Color[] controlBase,controlNormal,controlMask;
        int controlSize;

        public void Configure(WorldBuilder builder,TrackPath path,Color[] albedo,Color[] normal,Color[] mask,int size)
        {
            world=builder;track=path;controlBase=albedo;controlNormal=normal;controlMask=mask;controlSize=size;
        }

        public void Build(Mesh original,MeshRenderer renderer)
        {
            // Keep original normals/tangents across all material boundaries and retain the untouched
            // source mesh on MeshCollider. Only the renderer's triangle membership is partitioned.
            int openingFirst=Mathf.FloorToInt(.115f*Segments),openingLast=Mathf.CeilToInt(.34f*Segments);
            int warmFirst=Mathf.FloorToInt(.86f*Segments),warmLast=Mathf.CeilToInt(.902f*Segments);
            var triangles=original.triangles;var remaining=new List<int>(triangles.Length);
            for(int row=0;row<Segments;row++){
                if((row>=openingFirst&&row<openingLast)||(row>=warmFirst&&row<warmLast))continue;
                int first=row*Columns*6;
                for(int k=0;k<Columns*6;k++)remaining.Add(triangles[first+k]);
            }
            var control=Instantiate(original);control.name="Running surface / unchanged regions";
            control.SetTriangles(remaining,0);meshes.Add(control);renderer.GetComponent<MeshFilter>().sharedMesh=control;
            Region(original,openingFirst,openingLast,"Opening metric satin study",2048);
            Region(original,warmFirst,warmLast,"Warm gallery metric satin study",512);
            controlBase=null;controlNormal=null;controlMask=null;
            Debug.Log("Opening surface preview: two renderer regions, original collision retained; opening="+
                (openingFirst/(float)Segments).ToString("F6")+".."+(openingLast/(float)Segments).ToString("F6")+
                ", warm="+(warmFirst/(float)Segments).ToString("F6")+".."+(warmLast/(float)Segments).ToString("F6")+
                "; smoothness mask .35..50 x .90; direct lighting only, environment reflections unchanged.");
        }

        void Region(Mesh original,int first,int last,string name,int textureHeight)
        {
            int rows=last-first,count=(rows+1)*Stride;
            var sourceVertices=original.vertices;var sourceNormals=original.normals;var sourceTangents=original.tangents;
            var vertices=new Vector3[count];var normals=new Vector3[count];var tangents=new Vector4[count];var uv=new Vector2[count];
            var triangles=new int[rows*Columns*6];
            for(int row=0;row<=rows;row++)for(int column=0;column<=Columns;column++){
                int index=row*Stride+column,source=(first+row)*Stride+column;
                vertices[index]=sourceVertices[source];normals[index]=sourceNormals[source];tangents[index]=sourceTangents[source];
                uv[index]=new Vector2(column/(float)Columns,row/(float)rows);
                if(row<rows&&column<Columns){
                    int k=(row*Columns+column)*6;
                    triangles[k]=index;triangles[k+1]=index+Stride;triangles[k+2]=index+1;
                    triangles[k+3]=index+1;triangles[k+4]=index+Stride;triangles[k+5]=index+Stride+1;
                }
            }
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.normals=normals;mesh.tangents=tangents;mesh.uv=uv;
            mesh.triangles=triangles;mesh.RecalculateBounds();meshes.Add(mesh);
            var material=world.MakeMaterial(name,new Color(.13f,.145f,.165f),.9f,0,templateName:"RoadSurface");
            Maps(material,first/(float)Segments,last/(float)Segments,textureHeight);
            var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }

        void Maps(Material material,float first,float last,int height)
        {
            const int width=512;
            var albedo=new Color[width*height];var normals=new Color[albedo.Length];var masks=new Color[albedo.Length];
            float length=(last-first)*track.Length;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++){
                float u=x/(float)(width-1),v=y/(float)(height-1),progress=Mathf.Lerp(first,last,v);
                float across=(u-.5f)*22f,along=progress*track.Length;
                // Broad service-polish regions follow the physical deck, not camera or lamp positions.
                // Wandering paths break into 8–25 m islands; the outer maintenance margins stay matte.
                float wander=Mathf.Sin(along*.057f)*1.25f+Mathf.Sin(along*.121f)*.45f;
                float left=1-Ramp(1.1f,3.8f,Mathf.Abs(across+4.3f-wander));
                float right=1-Ramp(1.3f,4f,Mathf.Abs(across-4.1f+wander*.65f));
                float patch=Mathf.PerlinNoise(across*.19f+43f,along*.047f+17f);
                float breakup=Ramp(.32f,.67f,Mathf.PerlinNoise(across*.27f+81f,along*.088f+31f));
                float margin=1-Ramp(8.6f,10.8f,Mathf.Abs(across));
                float polish=Mathf.Clamp01(.13f+Mathf.Max(left,right)*breakup*.75f+Ramp(.3f,.72f,patch)*.24f)*margin;
                float shade=.83f+.13f*(patch*.4f+polish*.6f);
                var newBase=new Color(shade,shade,shade,1);
                // Small continuous relief has a metric scale, without pixel noise as the headline.
                const float delta=.12f;
                float dx=(Relief(across+delta,along)-Relief(across-delta,along))/(2*delta);
                float dy=(Relief(across,along+delta)-Relief(across,along-delta))/(2*delta);
                var normal=new Vector3(-dx,-dy,1).normalized;
                var newNormal=new Color(normal.x*.5f+.5f,normal.y*.5f+.5f,normal.z*.5f+.5f,1);
                var newMask=new Color(0,0,0,Mathf.Lerp(.35f,.5f,polish));
                // A 12 m transition returns to the original sampled maps at either end of each study.
                float blend=Ramp(0,12,Mathf.Min(v*length,(1-v)*length));
                int index=y*width+x;float oldU=u*3f,oldV=progress*240f;
                albedo[index]=Color.Lerp(SampleControl(controlBase,oldU,oldV),newBase,blend);
                normals[index]=Color.Lerp(SampleControl(controlNormal,oldU,oldV),newNormal,blend);
                masks[index]=Color.Lerp(SampleControl(controlMask,oldU,oldV),newMask,blend);
            }
            material.SetTexture("_BaseMap",Texture(material.name+" / broad aggregate",width,height,albedo,false));
            material.SetTexture("_BumpMap",Texture(material.name+" / metric relief",width,height,normals,true));
            material.SetTexture("_MetallicGlossMap",Texture(material.name+" / broken service polish",width,height,masks,true));
            material.SetTextureScale("_BaseMap",Vector2.one);
            material.SetFloat("_BumpScale",.25f);
        }

        static float Relief(float x,float y)
        {
            return Mathf.PerlinNoise(x*.55f+71f,y*.24f+13f)*.055f+
                Mathf.PerlinNoise(x*1.7f+37f,y*1.4f+29f)*.007f;
        }
        static float Ramp(float low,float high,float value){return Mathf.SmoothStep(0,1,Mathf.InverseLerp(low,high,value));}
        Color SampleControl(Color[] pixels,float u,float v)
        {
            float x=Mathf.Repeat(u,1)*controlSize-.5f,y=Mathf.Repeat(v,1)*controlSize-.5f;
            int ix=Mathf.FloorToInt(x),iy=Mathf.FloorToInt(y);float fx=x-ix,fy=y-iy;
            int x0=(ix+controlSize)%controlSize,x1=(x0+1)%controlSize,y0=(iy+controlSize)%controlSize,y1=(y0+1)%controlSize;
            return Color.Lerp(Color.Lerp(pixels[y0*controlSize+x0],pixels[y0*controlSize+x1],fx),
                Color.Lerp(pixels[y1*controlSize+x0],pixels[y1*controlSize+x1],fx),fy);
        }
        Texture2D Texture(string name,int width,int height,Color[] pixels,bool linear)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,true,linear){name=name,wrapMode=TextureWrapMode.Clamp,
                filterMode=FilterMode.Trilinear,anisoLevel=8};
            texture.SetPixels(pixels);texture.Apply(true,true);textures.Add(texture);return texture;
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);foreach(var texture in textures)if(texture)Destroy(texture);}
    }
}
