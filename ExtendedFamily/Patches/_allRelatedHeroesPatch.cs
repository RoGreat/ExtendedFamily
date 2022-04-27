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
        private static List<Hero> _allRelatedHeroes()
        {
            _list = new List<Hero>();

            List<Hero> hero = new() { _baseHero };
            // Trace.WriteLine("Hero: " + hero.First().Name);

            var siblings = RelatedTo(hero, Relation.Sibling).ToList();
            RelatedTo(siblings, Relation.Child).ToList();                                       // Nieces/nephews

            var parents = RelatedTo(hero, Relation.Parent).ToList();
            var auntsUncles = RelatedTo(parents, Relation.Sibling).ToList();                    // Aunts/uncles

            RelatedTo(auntsUncles, Relation.Child).ToList();                                    // Cousins

            var grandParents = RelatedTo(parents, Relation.Parent).ToList();
            var greatGrandParents = RelatedTo(grandParents, Relation.Parent).ToList();

            RelatedTo(grandParents, Relation.Spouse, true).ToList();                            // Step grandparents
            RelatedTo(greatGrandParents, Relation.Spouse, true).ToList();                       // Step great-grandparents

            var stepParents = RelatedTo(parents, Relation.Spouse, true).ToList();               // Stepparents
            var stepAuntsUncles = RelatedTo(stepParents, Relation.Sibling, true).ToList();      // Stepaunts/uncles

            RelatedTo(stepParents, Relation.Child, true).ToList();                              // Stepsiblings
            RelatedTo(stepAuntsUncles, Relation.Child, true).ToList();                          // Stepcousins

            var children = RelatedTo(hero, Relation.Child).ToList();
            var grandChildren = RelatedTo(children, Relation.Child).ToList();
            var greatGrandChildren = RelatedTo(grandChildren, Relation.Child).ToList();

            RelatedTo(grandChildren, Relation.Spouse, true).ToList();                           // Step grandchildren
            RelatedTo(greatGrandChildren, Relation.Spouse, true).ToList();                      // Step great-grandchildren

            var spouses = RelatedTo(hero, Relation.Spouse).ToList();

            RelatedTo(spouses, Relation.Parent).ToList();                                       // Father/mother-in-law
            RelatedTo(spouses, Relation.Sibling).ToList();                                      // Brother/sister-in-law 
            RelatedTo(spouses, Relation.Child).ToList();                                        // Son/daughter-in-law 

            var stepChildren = RelatedTo(spouses, Relation.Child, true).ToList();               // Stepchildren

            RelatedTo(stepChildren, Relation.Child, true).ToList();                             // Stepnieces/nephews

            _list = _list.Distinct().ToList();
            return _list;
        }

        private static IEnumerable<Hero> RelatedTo(List<Hero> heroes, Relation relation, bool stepRelated = false)
        {
            foreach (Hero queriedHero in heroes)
            {
                // Trace.WriteLine("RelatedTo: " + queriedHero.Name);
                switch (relation)
                {
                    case Relation.Spouse:
                        var spouse = queriedHero.Spouse;
                        if (spouse is not null)
                        {
                            AddList(spouse, stepRelated);
                            yield return spouse;
                        }
                        foreach (Hero exSpouse in queriedHero.ExSpouses)
                        {
                            AddList(exSpouse, stepRelated);
                            yield return exSpouse;
                        }
                        break;
                    case Relation.Child:
                        foreach (Hero child in queriedHero.Children)
                        {
                            AddList(child, stepRelated);
                            yield return child;
                        }
                        break;
                    case Relation.Sibling:
                        foreach (Hero sibling in queriedHero.Siblings)
                        {
                            AddList(sibling, stepRelated);
                            yield return sibling;
                        }
                        break;
                    case Relation.Parent:
                        var father = queriedHero.Father;
                        var mother = queriedHero.Mother;
                        if (father is not null)
                        {
                            AddList(father, stepRelated);
                            yield return father;
                        }
                        if (mother is not null)
                        {
                            AddList(mother, stepRelated);
                            yield return mother;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void AddList(Hero queriedHero, bool stepRelated)
        {
            if (stepRelated)
            {
                // Character sees steprelative as long as they were born before they died
                if (_baseHero.BirthDay < queriedHero.DeathDay)
                {
                    _list.AddRange(BaseHeroCheck(queriedHero));
                }
            }
            else
            {
                _list.AddRange(BaseHeroCheck(queriedHero));
            }
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

