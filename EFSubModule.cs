using Bannerlord.UIExtenderEx;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ExtendedFamily
{
    internal class EFSubModule : MBSubModuleBase
    {
        private UIExtender _extender;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            _extender = new UIExtender("ExtendedFamily");
            _extender.Register(typeof(EFSubModule).Assembly);
            _extender.Enable();

            new Harmony("mod.bannerlord.family.extended").PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarter;
                campaignGameStarter.LoadGameTexts(BasePath.Name + "Modules/ExtendedFamily/ModuleData/ef_module_strings.xml");
            }
        }
    }
}