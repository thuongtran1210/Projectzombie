using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Lightning feedback wiring; owns only this cast's handles and camera impulse.</summary>
    public sealed class DefenseLightningSignaturePresenter3D : MonoBehaviour
    {
        private DefenseLightningSignatureEffect3D _source;
        private DefenseHeroHealth3D _health;
        private DefenseHeroMotor3D _motor;
        private DefenseSignatureEffectSetSO _style;
        private IDefenseGameplayContext _gameplay;
        private DefenseSignatureFeedbackAdapter _adapter;
        private ProjectZombie.Features.Player.Camera3DFollower _camera;
        public bool IsBound => _adapter != null;
        public void Bind(DefenseLightningSignatureEffect3D source, IDefenseGameplayContext gameplay, DefenseEffectService3D effects,
            DefenseSignatureEffectSetSO style, Camera camera, DefenseIndicatorSurface3D surface)
        {
            Unbind();
            // Cosmetic authoring must never prevent the player from entering a valid match.
            if (source == null || gameplay == null || effects == null || style == null || !style.IsValid) return;
            if (!source.TryGetComponent<DefenseHeroHealth3D>(out _) || !source.TryGetComponent<DefenseHeroMotor3D>(out _)) return;
            _source=source;_gameplay=gameplay;_style=style;
            _health=source.GetComponent<DefenseHeroHealth3D>();_motor=source.GetComponent<DefenseHeroMotor3D>();
            if(camera!=null)camera.TryGetComponent(out _camera);
            _adapter=new DefenseSignatureFeedbackAdapter(source.Feedback,gameplay,effects,style,source,source.transform,
                ()=>isActiveAndEnabled&&_source!=null&&_source.isActiveAndEnabled&&_health!=null&&!_health.IsDown&&_motor!=null&&!_motor.IsDashing,
                (point,radius)=>surface!=null?surface.Place(point,.035f,radius):point+Vector3.up*.035f,true);
            source.Feedback.Released+=OnReleased;_health.Defeated+=Interrupt;_motor.DashFeedback.Started+=OnDash;
            gameplay.GameplayGateChanged+=OnGate;gameplay.RunCleanup+=ClearImpulse;
            effects.Prewarm(style.Cast,2);effects.Prewarm(style.Release,2);effects.Prewarm(style.Hit,2);
        }
        private void OnReleased(DefenseSignatureFeedback cue)
        {
            if(_camera!=null&&_gameplay!=null&&_gameplay.AdvancesGameplay&&ReferenceEquals(cue.SessionIdentity,_gameplay.SessionIdentity)&&cue.RunGeneration==_gameplay.RunGeneration)
                _camera.PlayImpulse(_style.CameraImpulseAmplitude,_style.CameraImpulseDuration);
        }
        private void OnDash(DefenseDashFeedback cue)=>Interrupt();
        private void Interrupt(){_source?.CancelCasting();_adapter?.Clear();ClearImpulse();}
        private void OnGate(bool active){if(!active)Interrupt();}
        private void ClearImpulse(){if(_camera!=null)_camera.ClearImpulse();}
        public void Unbind()
        {
            if(_source!=null)_source.Feedback.Released-=OnReleased;
            if(_health!=null)_health.Defeated-=Interrupt;
            if(_motor!=null)_motor.DashFeedback.Started-=OnDash;
            if(_gameplay!=null){_gameplay.GameplayGateChanged-=OnGate;_gameplay.RunCleanup-=ClearImpulse;}
            _adapter?.Dispose();_adapter=null;ClearImpulse();_source=null;_gameplay=null;_health=null;_motor=null;_camera=null;
        }
        private void OnDisable()=>Interrupt();
        private void OnDestroy()=>Unbind();
    }
}
