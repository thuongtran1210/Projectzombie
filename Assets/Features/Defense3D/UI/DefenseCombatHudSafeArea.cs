using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Only updates the authored HUD safe-area anchors when the screen changes.</summary>
    public sealed class DefenseCombatHudSafeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform _area;
        private Rect _last;
        private Vector2Int _screen;
        private void LateUpdate()
        {
            if(_area==null||Screen.width<=0||Screen.height<=0)return;
            var screen=new Vector2Int(Screen.width,Screen.height);var area=Screen.safeArea;
            if(screen==_screen&&area==_last)return;
            _screen=screen;_last=area;
            if(area.width<=0||area.height<=0)area=new Rect(0,0,screen.x,screen.y);
            _area.anchorMin=new Vector2(Mathf.Clamp01(area.xMin/screen.x),Mathf.Clamp01(area.yMin/screen.y));
            _area.anchorMax=new Vector2(Mathf.Clamp01(area.xMax/screen.x),Mathf.Clamp01(area.yMax/screen.y));
            _area.offsetMin=_area.offsetMax=Vector2.zero;
        }
    }
}
