using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Core.Architecture
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AppLoadingSealGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); float r=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.46f;
            Ring(vh,r,r*.98f,new Color(.78f,.63f,.36f,.65f));
            Ring(vh,r*.82f,r*.8f,new Color(.33f,.75f,.64f,.8f));
            Ring(vh,r*.42f,r*.4f,new Color(.83f,.7f,.4f,1));
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI*2/16;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),s=new Vector2(-d.y,d.x);
                Tri(vh,d*r*.67f-s*r*.045f,d*r*.67f+s*r*.045f,d*r*.9f,new Color(.86f,.7f,.38f,.9f));
            }
            Tri(vh,new Vector2(-r*.24f,0),new Vector2(0,r*.32f),new Vector2(r*.24f,0),new Color(.25f,.9f,.71f));
            Tri(vh,new Vector2(-r*.24f,0),new Vector2(r*.24f,0),new Vector2(0,-r*.32f),new Color(.09f,.48f,.41f));
        }
        private static void Tri(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color color)
        {int n=v.currentVertCount;v.AddVert(a,color,Vector2.zero);v.AddVert(b,color,Vector2.zero);v.AddVert(c,color,Vector2.zero);v.AddTriangle(n,n+1,n+2);}
        private static void Ring(VertexHelper v,float outer,float inner,Color color)
        {
            for(int i=0;i<96;i++)
            {float a=i*Mathf.PI*2/96,b=(i+1)*Mathf.PI*2/96;Vector2 x=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),y=new Vector2(Mathf.Cos(b),Mathf.Sin(b));Tri(v,x*inner,x*outer,y*outer,color);Tri(v,x*inner,y*outer,y*inner,color);}
        }
    }
}
