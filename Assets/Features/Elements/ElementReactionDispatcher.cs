using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Routes a resolved reaction to its effect owner.</summary>
    internal sealed class ElementReactionDispatcher
    {
        private readonly HuoHoanReaction _huoHoan;
        private readonly bool _canExecuteBocHoi;

        public ElementReactionDispatcher(FireSpreadSettings fireSettings, SteamSlipReactionSettings steamSlipSettings)
        {
            if (fireSettings != null)
                _huoHoan = new HuoHoanReaction(fireSettings);
            _canExecuteBocHoi = steamSlipSettings != null;
        }

        public bool CanExecute(ElementReactionType reaction) =>
            (reaction == ElementReactionType.BocHoi && _canExecuteBocHoi) ||
            (reaction == ElementReactionType.HuoHoan && _huoHoan != null);

        public bool TryExecute(ElementReactionType reaction, Enemy target, DamageData incomingDamage)
        {
            switch (reaction)
            {
                case ElementReactionType.BocHoi:
                    if (target == null || !_canExecuteBocHoi) return false;
                    SteamSlipPatchManager.CreatePatch(target);
                    return true;

                case ElementReactionType.HuoHoan:
                    return _huoHoan != null && _huoHoan.TryExecute(target, incomingDamage);

                default:
                    return false;
            }
        }
    }
}
