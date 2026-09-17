using System.Collections;
using UnityEngine;

namespace VectorRush
{
    /// <summary>
    /// Keeps the imported original-map districts out of the protected lighting
    /// field, then reveals them for the post-field portion of the shared course.
    /// </summary>
    public sealed class HybridRouteTransition : MonoBehaviour
    {
        [SerializeField] GameObject routeRoot;
        [SerializeField] float routeStart=.3228f;
        [SerializeField] float routeEnd=.975f;
        [SerializeField] float fadeToBlackSeconds=.16f;
        [SerializeField] float fadeFromBlackSeconds=.24f;
        float overlayAlpha;
        bool transitioning;

        public GameObject RouteRoot=>routeRoot;
        public float RouteStart=>routeStart;
        public float RouteEnd=>routeEnd;

        public void Configure(GameObject root,float start,float end)
        {
            routeRoot=root;
            routeStart=start;
            routeEnd=end;
            Apply(false);
        }

        void Awake()=>Apply(false);

        void Update()
        {
            var bootstrap=VectorBootstrap.Instance;
            var player=bootstrap&&bootstrap.Director?bootstrap.Director.Player:null;
            if(!player)return;
            float progress=player.TrackProgress;
            bool shouldShow=progress>=routeStart&&progress<routeEnd;
            if(!transitioning&&routeRoot&&routeRoot.activeSelf!=shouldShow)StartCoroutine(SwitchRoute(shouldShow));
        }

        IEnumerator SwitchRoute(bool visible)
        {
            transitioning=true;
            for(float elapsed=0;elapsed<fadeToBlackSeconds;elapsed+=Time.unscaledDeltaTime)
            {
                overlayAlpha=Mathf.Clamp01(elapsed/fadeToBlackSeconds);
                yield return null;
            }
            overlayAlpha=1;
            Apply(visible);
            yield return null;
            for(float elapsed=0;elapsed<fadeFromBlackSeconds;elapsed+=Time.unscaledDeltaTime)
            {
                overlayAlpha=1-Mathf.Clamp01(elapsed/fadeFromBlackSeconds);
                yield return null;
            }
            overlayAlpha=0;
            transitioning=false;
        }

        void OnGUI()
        {
            if(overlayAlpha<=0)return;
            GUI.depth=-1000;
            Color old=GUI.color;
            GUI.color=new Color(0,0,0,overlayAlpha);
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
            GUI.color=old;
        }

        void Apply(bool visible)
        {
            if(routeRoot&&routeRoot.activeSelf!=visible)routeRoot.SetActive(visible);
        }
    }
}
