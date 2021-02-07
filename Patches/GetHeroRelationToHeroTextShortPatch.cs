using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace ExtendedFamily.Patches
{
    [HarmonyPatch(typeof(CampaignUIHelper), "GetHeroRelationToHeroTextShort")]
    internal class GetHeroRelationToHeroTextShortPatch
    {
        private static List<string> _list;

        private static bool _related;

        private static Hero _queriedHero;

        private static Hero _baseHero;

        private static void Postfix(ref TextObject __result, Hero queriedHero, Hero baseHero)
        {
            _queriedHero = queriedHero;
            _baseHero = baseHero;

            _list = new List<string>();

            if (__result.Equals(FindText("str_wife_fatherinlaw")) || __result.Equals(FindText("str_husband_fatherinlaw")))
            {
                AddList(baseHero.IsFemale ? "str_new_wife_fatherinlaw" : "str_new_husband_fatherinlaw");
            }
            if (__result.Equals(FindText("str_wife_motherinlaw")) || __result.Equals(FindText("str_husband_motherinlaw")))
            {
                AddList(baseHero.IsFemale ? "str_new_wife_motherinlaw" : "str_new_husband_motherinlaw");
            }
            if (!__result.Equals(FindText("str_spouse")) && !__result.Equals(FindText("str_child")) && !__result.Equals(FindText("str_exspouse")) && !__result.Equals(FindText("str_brother")) && !__result.Equals(FindText("str_sister")) && !__result.Equals(FindText("str_wife_fatherinlaw")) && !__result.Equals(FindText("str_husband_fatherinlaw")) && !__result.Equals(FindText("str_wife_motherinlaw")) && !__result.Equals(FindText("str_husband_motherinlaw")))
            {
                _list.Add(__result.ToString());
            }
            __result = GetHeroRelationToHeroTextShort();
        }

        private static TextObject GetHeroRelationToHeroTextShort()
        {
            _related = false;

            // Parents
            if (_baseHero.Father == _queriedHero || _baseHero.Mother == _queriedHero)
            {
                _related = true;
            }
            if (_baseHero.Father is not null)
            {
                RelatedToParent(_baseHero.Father, _queriedHero);
            }
            if (_baseHero.Mother is not null)
            {
                RelatedToParent(_baseHero.Mother, _queriedHero);
            }

            // Siblings
            if (_baseHero.Siblings.Contains(_queriedHero))
            {
                if (_baseHero.Father == _queriedHero.Father && _baseHero.Mother == _queriedHero.Mother)
                {
                    AddList(_queriedHero.IsFemale ? "str_sister" : "str_brother");
                    _related = true;
                }
                else if (_baseHero.Father == _queriedHero.Father != (_baseHero.Mother == _queriedHero.Mother))
                {
                    AddList(_queriedHero.IsFemale ? "str_halfsister" : "str_halfbrother");
                    _related = true;
                }
            }
            if (_baseHero.Siblings.Any((Hero sibling) => sibling.Children.Contains(_queriedHero)))
            {
                AddList(_queriedHero.IsFemale ? "str_niece" : "str_nephew");
                _related = true;
            }

            // Children
            if (_baseHero.Children.Contains(_queriedHero))
            {
                AddList(_queriedHero.IsFemale ? "str_daughter" : "str_son");
                _related = true;
            }
            if (_baseHero.Children.Any((Hero child) => child.Children.Contains(_queriedHero)))
            {
                AddList(_queriedHero.IsFemale ? "str_granddaughter" : "str_grandson");
                _related = true;
            }
            if (_baseHero.Children.Any((Hero child) => child.Children.Any((Hero grandChild) => grandChild.Children.Contains(_queriedHero))))
            {
                AddList(_queriedHero.IsFemale ? "str_greatgranddaughter" : "str_greatgrandson");
                _related = true;
            }

            // Step-related
            if (!_related)
            {
                // Put in-laws here so they don't conflict with actual blood relatives
                if (_baseHero.Siblings.Any((Hero sibling) => sibling.Spouse == _queriedHero))
                {
                    AddList(_queriedHero.IsFemale ? "str_sisterinlaw" : "str_brotherinlaw");
                }
                if (_baseHero.Siblings.Any((Hero sibling) => sibling.ExSpouses.Contains(_queriedHero)))
                {
                    AddList(_queriedHero.IsFemale ? "str_ex_sisterinlaw" : "str_ex_brotherinlaw");
                }
                if (_baseHero.Children.Any((Hero child) => child.Spouse == _queriedHero))
                {
                    AddList(_queriedHero.IsFemale ? "str_daughterinlaw" : "str_soninlaw");
                }
                if (_baseHero.Children.Any((Hero child) => child.ExSpouses.Contains(_queriedHero)))
                {
                    AddList(_queriedHero.IsFemale ? "str_ex_daughterinlaw" : "str_ex_soninlaw");
                }
                if (_baseHero.Father is not null)
                {
                    foreach (Hero exSpouse in _baseHero.Father.ExSpouses)
                    {
                        // Method needs to find step parent
                        // Father's exspouse and exspouse's exspouses
                        StepRelatedToParent(exSpouse, _queriedHero, _baseHero);
                        foreach (Hero exSpouse2 in exSpouse.ExSpouses)
                        {
                            StepRelatedToParent(exSpouse2, _queriedHero, _baseHero);
                        }
                    }
                    // Also father's current spouse
                    if (_baseHero.Father.Spouse is not null)
                    {
                        StepRelatedToParent(_baseHero.Father.Spouse, _queriedHero, _baseHero);
                    }
                }
                if (_baseHero.Mother is not null)
                {
                    foreach (Hero exSpouse in _baseHero.Mother.ExSpouses)
                    {
                        StepRelatedToParent(exSpouse, _queriedHero, _baseHero);
                        foreach (Hero exSpouse2 in exSpouse.ExSpouses)
                        {
                            StepRelatedToParent(exSpouse2, _queriedHero, _baseHero);
                        }
                    }
                    if (_baseHero.Mother.Spouse is not null)
                    {
                        StepRelatedToParent(_baseHero.Mother.Spouse, _queriedHero, _baseHero);
                    }
                }
            }

            // Spouse
            if (_baseHero.Spouse == _queriedHero)
            {
                AddList(_queriedHero.IsFemale ? "str_wife" : "str_husband");
            }
            if (_baseHero.Spouse is not null)
            {
                if (_baseHero.Spouse.ExSpouses.Contains(_queriedHero))
                {
                    AddList(_baseHero.Spouse.IsFemale ? "str_wife_ex_husband" : "str_husband_ex_wife");
                }
                if (_baseHero.Spouse.Children.Contains(_queriedHero) && !_related)
                {
                    AddList(_queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                }
                if (_baseHero.Spouse.Children.Any((Hero child) => child.Children.Contains(_queriedHero)) && !_related)
                {
                    AddList(_queriedHero.IsFemale ? "str_stepgranddaughter" : "str_stepgrandson");
                }
                if (_baseHero.Spouse.Children.Any((Hero child) => child.Children.Any((Hero grandChild) => grandChild.Children.Contains(_queriedHero))) && !_related)
                {
                    AddList(_queriedHero.IsFemale ? "str_stepgreatgranddaughter" : "str_stepgreatgrandson");
                }
                foreach (Hero exSpouse in _baseHero.Spouse.ExSpouses)
                {
                    if (exSpouse.Children.Contains(_queriedHero) && !_related)
                    {
                        AddList(_queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                    }
                }
            }

            // Ex-spouse
            if (_baseHero.ExSpouses.Contains(_queriedHero))
            {
                AddList(_queriedHero.IsFemale ? "str_ex_wife" : "str_ex_husband");
            }
            if (_baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Spouse == _queriedHero))
            {
                AddList(_queriedHero.IsFemale ? "str_ex_husband_wife" : "str_ex_wife_husband");
            }
            if (_baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.ExSpouses.Contains(_queriedHero)))
            {
                AddList(_queriedHero.IsFemale ? "str_ex_husband_ex_wife" : "str_ex_wife_ex_husband");
            }
            if (_baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Children.Contains(_queriedHero)) && !_related)
            {
                AddList(_queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
            }

            if (_list.IsEmpty())
            {
                return TextObject.Empty;
            }

            _list = _list.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

            string result = string.Join(", ", _list);
            return new TextObject(result, null);
        }

        private static void RelatedToParent(Hero baseHeroParent, Hero queriedHero)
        {
            if (baseHeroParent is null)
            {
                return;
            }
            if (baseHeroParent.Siblings.Contains(queriedHero))
            {
                AddList(queriedHero.IsFemale ? "str_aunt" : "str_uncle");
                _related = true;
            }
            if (baseHeroParent.Siblings.Any((Hero auntUncle) => auntUncle.Children.Contains(queriedHero)))
            {
                AddList("str_cousin");
                _related = true;
            }
            if (baseHeroParent.Father == queriedHero)
            {
                AddList("str_grandfather");
                _related = true;
            }
            // If I wanted to eventually implement great-grandparents... Too cluttered already. Will lessen clutter to properly implement.
            if (baseHeroParent.Father is not null)
            {
                if (baseHeroParent.Father.Father == queriedHero)
                {
                    AddList("str_greatgrandfather");
                    _related = true;
                }
                if (baseHeroParent.Father.Mother == queriedHero)
                {
                    AddList("str_greatgrandmother");
                    _related = true;
                }
            }
            if (baseHeroParent.Mother == queriedHero)
            {
                AddList("str_grandmother");
                _related = true;
            }
            if (baseHeroParent.Mother is not null)
            {
                if (baseHeroParent.Mother.Father == queriedHero)
                {
                    AddList("str_greatgrandfather");
                    _related = true;
                }
                if (baseHeroParent.Mother.Mother == queriedHero)
                {
                    AddList("str_greatgrandmother");
                    _related = true;
                }
            }
        }

        // Step relatives
        private static void StepRelatedToParent(Hero baseHeroStepParent, Hero queriedHero, Hero baseHero)
        {
            if (baseHeroStepParent is null)
            {
                return;
            }
            if (baseHeroStepParent.Siblings.Contains(queriedHero))
            {
                AddList(queriedHero.IsFemale ? "str_stepaunt" : "str_stepuncle");
            }
            if (baseHeroStepParent.Siblings.Any((Hero auntuncle) => auntuncle.Children.Contains(queriedHero)))
            {
                AddList("str_stepcousin");
            }
            if (baseHeroStepParent.Father == queriedHero)
            {
                AddList("str_stepgrandfather");
            }
            if (baseHeroStepParent.Father is not null)
            {
                if (baseHeroStepParent.Father.Father == queriedHero)
                {
                    AddList("str_stepgreatgrandfather");
                }
                if (baseHeroStepParent.Father.Mother == queriedHero)
                {
                    AddList("str_stepgreatgrandmother");
                }
            }
            if (baseHeroStepParent.Mother == queriedHero)
            {
                AddList("str_stepgrandmother");
            }
            if (baseHeroStepParent.Mother is not null)
            {
                if (baseHeroStepParent.Mother.Father == queriedHero)
                {
                    AddList("str_stepgreatgrandfather");
                }
                if (baseHeroStepParent.Mother.Mother == queriedHero)
                {
                    AddList("str_stepgreatgrandmother");
                }
            }
            if (baseHeroStepParent == queriedHero)
            {
                AddList(queriedHero.IsFemale ? "str_stepmother" : "str_stepfather");
            }
            if (baseHeroStepParent.Children.Contains(queriedHero) && !baseHero.Siblings.Contains(queriedHero))
            {
                AddList(queriedHero.IsFemale ? "str_stepsister" : "str_stepbrother");
            }
            if (baseHeroStepParent.Children.Any((Hero stepsibling) => stepsibling.Children.Contains(queriedHero)))
            {
                AddList(queriedHero.IsFemale ? "str_stepniece" : "str_stepnephew");
            }
        }

        private static void AddList(string str)
        {
            _list.Add(FindText(str).ToString());
        }

        private static TextObject FindText(string id)
        {
            return GameTexts.FindText(id, null);
        }
    }
}