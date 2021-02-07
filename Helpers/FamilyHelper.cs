using TaleWorlds.CampaignSystem;

namespace ExtendedFamily.Extension
{
    public static class FamilyHelper
    {
        public static Hero FindAncestorOf(Hero hero)
        {
            // Added in kingdom ruler's precedence by popular demand over just showing partilineal order. 
            // For example, it will show Rhagaea's bloodline if she so happened to marry someone of lower birth.
            // There are no houses in Bannerlord, only clans so this is the best I can do.
            if (hero.Father is not null && hero.Father == hero.Father?.MapFaction.Leader)
            {
                return FindAncestorOf(hero.Father);
            }
            if (hero.Mother is not null && hero.Mother == hero.Mother?.MapFaction.Leader)
            {
                return FindAncestorOf(hero.Mother);
            }
            if (hero.Father is not null)
            {
                return FindAncestorOf(hero.Father);
            }
            if (hero.Mother is not null)
            {
                return FindAncestorOf(hero.Mother);
            }
            return hero;
        }

        public static Hero FindOtherParent(Hero hero)
        {
            if (hero.Father is not null)
            {
                return hero.Mother;
            }
            return hero.Father;
        }
    }
}