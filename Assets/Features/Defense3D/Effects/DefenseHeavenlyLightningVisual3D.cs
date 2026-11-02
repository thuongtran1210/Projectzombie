using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public sealed class DefenseHeavenlyLightningVisual3D : DefenseEffectVisual3D
    {
        [SerializeField, Range(0,2)] private int _phase;
        [SerializeField] private LineRenderer[] _bolts;
        [SerializeField] private MeshRenderer _ground, _flash;
        private Vector3[][] _points;
        private MaterialPropertyBlock _properties;
        private float _radius;
        private Color _color;
        public override bool IsConfigured => _bolts != null && _bolts.Length > 0 && _ground != null && _flash != null;
        private void Cache()
        {
            if (_properties != null) return;
            _properties = new MaterialPropertyBlock(); _points = new Vector3[_bolts.Length][];
            for (int i=0;i<_bolts.Length;i++)
            { _points[i]=new Vector3[_bolts[i].positionCount]; _bolts[i].GetPositions(_points[i]); }
        }
        public override void Prepare(Color color,float diameter)
        {
            Cache(); _radius=Mathf.Max(.05f,diameter*.5f);_color=color;
            _ground.transform.localScale=new Vector3(diameter,diameter,1);
            _flash.transform.localScale=Vector3.one*(_radius*(_phase==2?2f:.85f));
            _flash.transform.localPosition=Vector3.up*(_phase==2?.05f:.25f);
            Render(0);
        }
        public override void Render(float progress)
        {
            Cache();float p=Mathf.Clamp01(progress);
            float energy=_phase==0?Mathf.Lerp(.3f,1,p):Mathf.Exp(-p*12)+.7f*Mathf.Exp(-Mathf.Pow((p-.2f)*20,2))+.35f*Mathf.Exp(-Mathf.Pow((p-.42f)*22,2));
            float height=_phase==1?Mathf.Clamp(_radius*2.4f,5,12):_phase==0?1.2f:1f;
            for(int i=0;i<_bolts.Length;i++)
            {
                var line=_bolts[i];var points=_points[i];
                for(int j=0;j<points.Length;j++)
                {
                    var point=points[j];float weight=j==0||j==points.Length-1?0f:1f;
                    float jitter=Mathf.Sin(j*19.7f+i*7.3f+Mathf.Floor(p*24)*2.1f)*.035f*weight;
                    line.SetPosition(j,new Vector3((point.x+jitter)*_radius,point.y*height,(point.z-jitter*.7f)*_radius));
                }
                line.widthMultiplier=(_phase==1?(i==0?.65f:i<3?.28f:.12f):.075f)*Mathf.Clamp(_radius/3f,.7f,1.4f)*(1f-p*.55f);
                var color=_color;color.a*=Mathf.Clamp01(energy);line.startColor=line.endColor=color;
                _properties.Clear();_properties.SetFloat("_Progress",p);_properties.SetFloat("_Intensity",energy);line.SetPropertyBlock(_properties);
                line.enabled=(_phase==0||p<.7f)&&energy>.015f;
            }
            _properties.Clear();_properties.SetFloat("_Progress",p);_properties.SetFloat("_Phase",_phase);
            _properties.SetFloat("_Intensity",_phase==0?Mathf.Lerp(.25f,1,p):Mathf.Pow(1-p,1.5f));
            _properties.SetColor("_Tint",_color);_ground.SetPropertyBlock(_properties);_ground.enabled=p<1;
            _properties.SetFloat("_Progress",_phase==0?1-p:p);
            _flash.SetPropertyBlock(_properties);_flash.enabled=(_phase==0?p>.55f:p<.8f);
        }
        public override void ResetVisual()
        {
            if(_bolts!=null)foreach(var line in _bolts)if(line!=null){line.enabled=false;line.widthMultiplier=0;line.SetPropertyBlock(null);}
            if(_ground!=null){_ground.enabled=false;_ground.SetPropertyBlock(null);}
            if(_flash!=null){_flash.enabled=false;_flash.SetPropertyBlock(null);}
        }
    }
}
