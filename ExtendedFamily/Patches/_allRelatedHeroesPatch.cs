using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;

namespace ExtendedFamily.Patches
{
    // Add more people into the encyclopedia
    [HarmonyPatch(typeof(EncyclopediaHeroPageVM), "_allRelatedHeroes", MethodType.Getter)]
    internal class _allRelatedHeroesPatch
    {
        private static bool _sameHero;

        private static Hero _baseHero;

        private static List<Hero> _list;

        private static IEnumerable<Hero> _tempResult;

        private enum Relation
        {
            Parent,
            Sibling,
            Child,
            Spouse
        }

        private static void Prefix(Hero ____hero, ref IEnumerable<Hero> __state)
        {
            if (_baseHero == ____hero)
            {
                _sameHero = true;
                return;
            }
            _baseHero = ____hero;
            _sameHero = false;

            __state = _allRelatedHeroes();
        }

        private static void Postfix(ref IEnumerable<Hero> __result, ref IEnumerable<Hero> __state)
        {
            if (_sameHero)
            {
                __result = _tempResult;
                return;
            }
            __result = __result.Union(__state);
            _tempResult = __result;
        }

        // Notes: Am getting great uncles and cousins and am I not sure why
        // They are getting confused with step uncles and cousins

        // Notes: steprelatives can only be determined if children
        // are from different spouses. Unable to ascertain here.
        private static List<Hero> _allRelatedHeroes()
        {
            _list = new List<Hero>();

            List<Hero> hero = new() { _baseHero };
            // Trace.WriteLine("Hero: " + hero.First().Name);

            var siblings = Dedupe(RelatedTo(hero, Relation.Sibling));
            Dedupe(RelatedTo(siblings, Relation.Child));            // Nieces/nephews 
            Dedupe(RelatedTo(siblings, Relation.Spouse));           // Sibling-in-law

            var parents = Dedupe(RelatedTo(hero, Relation.Parent));

            var auntsUncles = Dedupe(RelatedTo(parents, Relation.Sibling));
            Dedupe(RelatedTo(auntsUncles, Relation.Child));         // Cousins

            var grandParents = Dedupe(RelatedTo(parents, Relation.Parent));
            Dedupe(RelatedTo(grandParents, Relation.Spouse));       // Other grandparent

            var greatGrandParents = Dedupe(RelatedTo(grandParents, Relation.Parent));
            Dedupe(RelatedTo(greatGrandParents, Relation.Spouse));  // Other great-GP

            var otherParents = Dedupe(RelatedTo(parents, Relation.Spouse));
            Dedupe(RelatedTo(otherParents, Relation.Child));        // Other child

            // Goes into great-uncle/second-cousins
            // var otherAuntsUncles = Dedupe(RelatedTo(otherParents, Relation.Sibling));
            // Dedupe(RelatedTo(otherAuntsUncles, Relation.Child)); // Other cousin

            var otherGrandparent = Dedupe(RelatedTo(otherParents, Relation.Parent));
            Dedupe(RelatedTo(otherGrandparent, Relation.Parent));   // Other GGP

            var children = Dedupe(RelatedTo(hero, Relation.Child));
            var grandChildren = Dedupe(RelatedTo(children, Relation.Child));
            Dedupe(RelatedTo(grandChildren, Relation.Spouse));      // GC-in-law
            Dedupe(RelatedTo(grandChildren, Relation.Child));       // GGC

            var spouses = Dedupe(RelatedTo(hero, Relation.Spouse));
            Dedupe(RelatedTo(spouses, Relation.Parent));            // Parent-in-law
            Dedupe(RelatedTo(spouses, Relation.Sibling));           // Sibling-in-law
            Dedupe(RelatedTo(spouses, Relation.Child));             // Child-in-law

            var otherChildren = Dedupe(RelatedTo(spouses, Relation.Child));
            Dedupe(RelatedTo(otherChildren, Relation.Child));

            _list = Dedupe(_list);
            return _list;
        }

        private static List<Hero> Dedupe(IEnumerable<Hero> heroes)
        {
            return heroes.Distinct().ToList();
        }

        private static IEnumerable<Hero> RelatedTo(List<Hero> heroes, Relation relation)
        {
            foreach (Hero queriedHero in heroes)
            {
                // Trace.WriteLine("Hero: " + queriedHero.Name);
                switch (relation)
                {
                    case Relation.Spouse:
                        var spouse = queriedHero.Spouse;
                        if (spouse is not null)
                        {
                            // Trace.WriteLine("   Spouse: " + spouse.Name);
                            AddList(spouse);
                            yield return spouse;
                        }
                        foreach (Hero exSpouse in queriedHero.ExSpouses)
                        {
                            // Trace.WriteLine("   exSpouse: " + exSpouse.Name);
                            AddList(exSpouse);
                            yield return exSpouse;
                        }
                        break;
                    case Relation.Child:
                        foreach (Hero child in queriedHero.Children)
                        {
                            // Trace.WriteLine("   Child: " + child.Name);
                            AddList(child);
                            yield return child;
                        }
                        break;
                    case Relation.Sibling:
                        foreach (Hero sibling in queriedHero.Siblings)
                        {
                            // Trace.WriteLine("   Sibling: " + sibling.Name);
                            AddList(sibling);
                            yield return sibling;
                        }
                        break;
                    case Relation.Parent:
                        var father = queriedHero.Father;
                        var mother = queriedHero.Mother;
                        if (father is not null)
                        {
                            // Trace.WriteLine("   Father: " + father.Name);
                            AddList(father);
                            yield return father;
                        }
                        if (mother is not null)
                        {
                            // Trace.WriteLine("   Mother: " + mother.Name);
                            AddList(mother);
                            yield return mother;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void AddList(Hero queriedHero)
        {
            _list.AddRange(BaseHeroCheck(queriedHero));
        }

        private static IEnumerable<Hero> BaseHeroCheck(Hero queriedHero)
        {
            if (queriedHero != _baseHero)
            {
                yield return queriedHero;
            }
        }
    }
}

