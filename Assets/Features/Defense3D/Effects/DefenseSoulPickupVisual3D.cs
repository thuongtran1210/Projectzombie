using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Presentation only. The existing wood-drop pool owns pickup lifetime and movement.</summary>
    [DisallowMultipleComponent]
    public sealed class DefenseSoulPickupVisual3D : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        private DefenseWoodDrop3D _drop;
        private MaterialPropertyBlock _properties;
        private float _startedAt,_phase;
        private static readonly int AnimationTime=Shader.PropertyToID("_AnimTime");
        private static readonly int Phase=Shader.PropertyToID("_Phase");
        private void Cache()
        {
            if(_renderer==null)_renderer=GetComponent<MeshRenderer>();
            if(_drop==null)_drop=GetComponent<DefenseWoodDrop3D>();
            if(_properties==null)_properties=new MaterialPropertyBlock();
        }
        public void Begin(float sessionTime,Vector3 position)
        {
            Cache();_startedAt=sessionTime;
            _phase=Mathf.Repeat(position.x*1.73f+position.z*2.19f,Mathf.PI*2);
            if(_renderer!=null)_renderer.enabled=true;
            RenderAt(0f);
        }
        public void RenderAt(float elapsed)
        {
            Cache();if(_renderer==null)return;
            _properties.SetFloat(AnimationTime,Mathf.Max(0f,elapsed));_properties.SetFloat(Phase,_phase);
            _renderer.SetPropertyBlock(_properties);
        }
        public void ResetPresentation()
        {
            Cache();_startedAt=0f;_phase=0f;_properties.Clear();
            if(_renderer!=null){_renderer.SetPropertyBlock(null);_renderer.enabled=false;}
        }
        private void OnEnable(){Cache();if(_renderer!=null)_renderer.enabled=true;RenderAt(0f);}
        private void LateUpdate(){if(_drop!=null)RenderAt(_drop.PresentationTime-_startedAt);}
        private void OnDisable()=>ResetPresentation();
    }
}
