using Bannerlord.UIExtenderEx;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace ExtendedFamily
{
    internal class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            var extender = new UIExtender("ExtendedFamily");
            extender.Register(typeof(SubModule).Assembly);
            extender.Enable();

            new Harmony("mod.bannerlord.family.extended").PatchAll();
        }
    }
}