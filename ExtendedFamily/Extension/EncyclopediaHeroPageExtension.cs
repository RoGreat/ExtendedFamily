using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace ExtendedFamily.Extension
{
    [PrefabExtension("EncyclopediaHeroPage", "descendant::GridWidget[@Id='FamilyGrid']")]
    internal class EncyclopediaFamilyGridExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Replace;

        [PrefabExtensionFileName]
        public string FamilyGridResize => "EncyclopediaFamilyGrid";
    }
}
