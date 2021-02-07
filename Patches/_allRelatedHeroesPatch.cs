using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;

namespace ExtendedFamily.Patches
{
    [HarmonyPatch(typeof(EncyclopediaHeroPageVM), "_allRelatedHeroes", MethodType.Getter)]
    internal class _allRelatedHeroesPatch
    {
        private static bool _sameHero;

        private static Hero _baseHero;

        private static List<Hero> _list;

        private static IEnumerable<Hero> _tempResult;

        private static bool _inLawsCare;

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

        public static List<Hero> _allRelatedHeroes()
        {
            _list = new List<Hero>();

            // Father
            Hero father = _baseHero.Father;
            if (father is not null)
            {
                RelatedTo(father, "father");             // Grandfather
                RelatedTo(father, "mother");             // Grandmother
                RelatedTo(father, "siblings");           // Aunts and uncles
                RelatedTo(father, "spouse");             // Stepmothers
                RelatedTo(father, "exspouses");          // Dead mother/stepmothers
                RelatedTo(father, "siblingschildren");   // Cousins

                // Step relatives
                foreach (Hero fathersExSpouse in father.ExSpouses)
                {
                    StepRelatedTo(stepParent: fathersExSpouse);
                }
                if (father.Spouse is not null)
                {
                    StepRelatedTo(stepParent: father.Spouse);
                }

                // Great-grandparents
                if (father.Father is not null)
                {
                    RelatedTo(father.Father, "father");
                }
                if (father.Mother is not null)
                {
                    RelatedTo(father.Mother, "mother");
                }
            }

            // Mother
            Hero mother = _baseHero.Mother;
            if (mother is not null)
            {
                RelatedTo(mother, "father");              // Grandfather
                RelatedTo(mother, "mother");              // Grandmother
                RelatedTo(mother, "siblings");            // Aunts and uncles
                RelatedTo(mother, "spouse");              // Stepfathers
                RelatedTo(mother, "exspouses");           // Dead father/stepfathers
                RelatedTo(mother, "siblingschildren");    // Cousins

                // Step relatives
                foreach (Hero mothersExSpouse in mother.ExSpouses)
                {
                    StepRelatedTo(stepParent: mothersExSpouse);
                }
                if (mother.Spouse is not null)
                {
                    StepRelatedTo(stepParent: mother.Spouse);
                }

                // Great-grandparents
                if (mother.Father is not null)
                {
                    RelatedTo(mother.Father, "father");   // Great-grandfather
                }
                if (mother.Mother is not null)
                {
                    RelatedTo(mother.Mother, "mother");   // Great-grandmother
                }
            }

            // Spouse
            Hero spouse = _baseHero.Spouse;
            if (spouse is not null)
            {
                RelatedTo(spouse, "father");      // Father-in-law
                RelatedTo(spouse, "mother");      // Mother-in-law
                RelatedTo(spouse, "siblings");    // Sibling-in-laws
                RelatedTo(spouse, "exspouses");   // Spouse's ex-spouses

                // Step relatives
                // Remember that some children have no mothers or fathers... Weird.
                foreach (Hero exSpouse in spouse.ExSpouses)
                {
                    StepRelatedTo(stepParent: null, spouse: exSpouse);
                    foreach (Hero exSpouse2 in exSpouse.ExSpouses)
                    {
                        StepRelatedTo(stepParent: null, spouse: exSpouse2);
                    }
                }
                StepRelatedTo(stepParent: null, spouse: spouse);
            }

            // Sibling
            foreach (Hero sibling in _baseHero.Siblings)
            {
                // Your nieces and nephews
                RelatedTo(sibling, "children");                  // Nieces and nephews
                RelatedTo(sibling, "spouse");                    // Sibling-in-law

                // Still keep in touch with sibling's ex-spouse if there is a child 
                // Try to only see only the relevant exspouse if possible
                if (sibling.Children.Any())
                {
                    RelatedTo(sibling, "relevantexspouse");      // Former sibling-in-law
                }
            }

            // Children
            foreach (Hero child in _baseHero.Children)
            {
                RelatedTo(child, "children");                // Grandchildren
                foreach (Hero child2 in child.Children)
                {
                    RelatedTo(child2, "children");           // Great-Grandchildren
                }
                RelatedTo(child, "spouse");                  // Child-in-law

                // Still keep in touch with child's ex-spouse if there is a grandchild
                // Try to only see only the relevant exspouse if possible
                if (child.Children.Any())
                {
                    _inLawsCare = true;
                    RelatedTo(child, "relevantexspouse");    // Former child-in-law
                }
            }

            // Ex-spouses
            foreach (Hero exSpouse in _baseHero.ExSpouses)
            {
                RelatedTo(exSpouse, "exspouses");     // Ex-spouse's ex-spouses
                RelatedTo(exSpouse, "children");      // Ex-spouse's children
                if (_inLawsCare)
                {
                    RelatedTo(exSpouse, "father");        // Former father-in-law
                    RelatedTo(exSpouse, "mother");        // Former mother-in-law
                }
            }
            _inLawsCare = false;
            
            _list = _list.Distinct().ToList<Hero>();
            return _list;
        }

        private static void RelatedTo(Hero queriedHero, string relatedTo)
        {
            switch (relatedTo)
            {
                case "spouse":
                    _list.AddRange(BaseHeroCheck(queriedHero.Spouse));
                    break;
                case "children":
                    foreach (Hero child in queriedHero.Children)
                    {
                        _list.AddRange(BaseHeroCheck(child));
                    }
                    break;
                case "siblings":
                    foreach (Hero sibling in queriedHero.Siblings)
                    {
                        _list.AddRange(BaseHeroCheck(sibling));
                    }
                    break;
                case "father":
                    _list.AddRange(BaseHeroCheck(queriedHero.Father));
                    break;
                case "mother":
                    _list.AddRange(BaseHeroCheck(queriedHero.Mother));
                    break;
                case "exspouses":
                    foreach (Hero exSpouse in queriedHero.ExSpouses)
                    {
                        _list.AddRange(BaseHeroCheck(exSpouse));
                    }
                    break;
                case "relevantexspouse":
                    foreach (Hero exSpouse in queriedHero.ExSpouses)
                    {
                        foreach (Hero child in exSpouse.Children)
                        {
                            if (queriedHero.Children.Contains(child))
                            {
                                _list.AddRange(BaseHeroCheck(exSpouse));
                            }
                        }
                    }
                    break;
                case "siblingschildren":
                    foreach (Hero sibling in queriedHero.Siblings)
                    {
                        foreach (Hero child in sibling.Children)
                        {
                            _list.AddRange(BaseHeroCheck(child));
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        // If I want to implement more step relatives...
        private static void StepRelatedTo(Hero stepParent, Hero spouse = null)
        {
            if (stepParent is not null)
            {
                //Trace.WriteLine("B-day " + baseHero.Name + " baseHero: ");
                //Trace.WriteLine(baseHero.BirthDay.ToDays.ToString());
                //Trace.WriteLine("D-day " + stepParent.Name + " stepParent: \n");
                //Trace.WriteLine(stepParent.DeathDay.ToDays.ToString());
                // If your stepparent is alive during your birth, then care about stepparent's lineage
                if (_baseHero.BirthDay < stepParent.DeathDay)
                {
                    // Step siblings
                    foreach (Hero stepParentSpouse in stepParent.ExSpouses)
                    {
                        foreach (Hero child in stepParentSpouse.Children)
                        {
                            if (stepParent.Children.Contains(child))
                            {
                                _list.AddRange(BaseHeroCheck(stepParentSpouse));
                            }
                        }
                    }
                    foreach (Hero stepSibling in stepParent.Children)
                    {
                        _list.AddRange(BaseHeroCheck(stepSibling));
                        // Step nieces and nephews
                        foreach (Hero stepNieceNephew in stepSibling.Children)
                        {
                            _list.AddRange(BaseHeroCheck(stepNieceNephew));
                        }
                    }
                    // Step aunts and uncles
                    foreach (Hero stepAuntUncle in stepParent.Siblings)
                    {
                        _list.AddRange(BaseHeroCheck(stepAuntUncle));
                        // Step cousins
                        foreach (Hero stepCousin in stepAuntUncle.Children)
                        {
                            _list.AddRange(BaseHeroCheck(stepCousin));
                        }
                    }
                    // Step grandparents
                    _list.AddRange(BaseHeroCheck(stepParent.Father));
                    _list.AddRange(BaseHeroCheck(stepParent.Mother));

                    // Step great-grandparents
                    if (stepParent.Father is not null)
                    {
                        _list.AddRange(BaseHeroCheck(stepParent.Father.Father));
                        _list.AddRange(BaseHeroCheck(stepParent.Father.Mother));
                    }
                    if (stepParent.Mother is not null)
                    {
                        _list.AddRange(BaseHeroCheck(stepParent.Mother.Father));
                        _list.AddRange(BaseHeroCheck(stepParent.Mother.Mother));
                    }
                }
            }
            // Step children
            if (spouse is not null)
            {
                foreach (Hero stepChild in spouse.Children)
                {
                    //Trace.WriteLine("D-day " + baseHero.Name + " baseHero: ");
                    //Trace.WriteLine(baseHero.DeathDay.ToDays.ToString());
                    //Trace.WriteLine("B-day " + stepChild.Name + " stepChild: \n");
                    //Trace.WriteLine(stepChild.BirthDay.ToDays.ToString());
                    // If you are alive when a stepchild was birthed, then care about stepchild's lineage
                    if (_baseHero.DeathDay > stepChild.BirthDay)
                    {
                        _list.AddRange(BaseHeroCheck(stepChild));

                        foreach (Hero stepGrandChild in stepChild.Children)
                        {
                            _list.AddRange(BaseHeroCheck(stepGrandChild));

                            foreach (Hero stepGreatGrandChild in stepGrandChild.Children)
                            {
                                _list.AddRange(BaseHeroCheck(stepGreatGrandChild));
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<Hero> BaseHeroCheck(Hero queriedHero)
        {
            if (queriedHero is not null)
            {
                if (queriedHero != _baseHero)
                {
                    yield return queriedHero;
                }
            }
        }
    }
}

