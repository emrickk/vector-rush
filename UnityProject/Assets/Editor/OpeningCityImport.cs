using System;
using UnityEditor;
using UnityEngine;

namespace VectorRush.Editor
{
    public sealed class OpeningCityImport : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if(!assetPath.StartsWith("Assets/Resources/OpeningCity/",StringComparison.Ordinal))return;
            var importer=(ModelImporter)assetImporter;
            importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;
            importer.addCollider=false;importer.isReadable=false;
            importer.importNormals=ModelImporterNormals.Import;
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            importer.importAnimation=false;
        }
        public static void Build()
        {
            foreach(var asset in new[]{"MeridianExchange","TransitTerraces","RiversideOffices","CanalWorks"})
            {
                var source=Resources.Load<GameObject>("OpeningCity/"+asset);
                if(!source)throw new InvalidOperationException("Missing opening asset "+asset);
                var instance=UnityEngine.Object.Instantiate(source);
                try
                {
                    var bounds=new Bounds(Vector3.zero,Vector3.zero);
                    foreach(var renderer in instance.GetComponentsInChildren<Renderer>())bounds.Encapsulate(renderer.bounds);
                    if(bounds.min.y<-.1f||bounds.max.y<15||bounds.max.y>160||bounds.size.z>100||bounds.size.x>110)
                        throw new InvalidOperationException("Opening import units/axis invalid: "+asset+" "+bounds);
                    if(instance.GetComponentsInChildren<Collider>().Length>0)throw new InvalidOperationException("Scenery collider forbidden: "+asset);
                    Debug.Log("VR_OPENING_IMPORT "+asset+" bounds="+bounds);
                }
                finally{UnityEngine.Object.DestroyImmediate(instance);}
            }
            AAAReviewBuild.BuildSolstice();
        }
    }
}
