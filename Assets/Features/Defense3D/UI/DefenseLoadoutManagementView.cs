using ProjectZombie.Features.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Standalone saved-team editor; never selects a stage or launches a match.</summary>
    public sealed class DefenseLoadoutManagementView : DefensePreparationView
    {
        public override MetaScreenType ScreenType => MetaScreenType.LoadoutManagement;
        protected override bool IsLoadoutManagement => true;
    }
}
