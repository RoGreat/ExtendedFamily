using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace ExtendedFamily.Patches
{
    // Give familial titles to people
    [HarmonyPatch(typeof(CampaignUIHelper), "GetHeroRelationToHeroTextShort")]
    internal class GetHeroRelationToHeroTextShortPatch
    {
        private static List<string> _list;

        private static bool _related;

        private static bool _stepRelated;

        private static bool Prefix(ref TextObject __result, Hero queriedHero, Hero baseHero)
        {
            _list = new List<string>();
            _related = false;
            _stepRelated = true;

            // Parents
            if (baseHero.Father == queriedHero)
            {
                AddList("str_father");
                _related = true;
            }
            else if (baseHero.Mother == queriedHero)
            {
                AddList("str_mother");
                _related = true;
            }
            if (baseHero.Father is not null)
            {
                RelatedToParent(baseHero.Father, queriedHero);
            }
            if (baseHero.Mother is not null)
            {
                RelatedToParent(baseHero.Mother, queriedHero);
            }

            // Siblings
            if (baseHero.Siblings.Contains(queriedHero))
            {
                if (baseHero.Father == queriedHero.Father && baseHero.Mother == queriedHero.Mother)
                {
                    if (baseHero.Age == queriedHero.Age)
                    {
                        AddList("str_twin");
                        _related = true;
                    }
                    else
                    {
                        AddList(queriedHero.IsFemale ? "str_sister" : "str_brother");
                        _related = true;
                    }
                }
                else if (baseHero.Father == queriedHero.Father != (baseHero.Mother == queriedHero.Mother))
                {
                    AddList(queriedHero.IsFemale ? "str_halfsister" : "str_halfbrother");
                    _related = true;
                }
            }

            // Children
            if (baseHero.Children.Contains(queriedHero))
            {
                AddList(queriedHero.IsFemale ? "str_daughter" : "str_son");
                _related = true;
            }
            if (baseHero.Siblings.Any((Hero child) => child.Children.Contains(queriedHero)))
            {
                AddList(queriedHero.IsFemale ? "str_niece" : "str_nephew");
                _related = true;
            }
            if (baseHero.Children.Any((Hero child) => child.Children.Contains(queriedHero)))
            {
                AddList(queriedHero.IsFemale ? "str_granddaughter" : "str_grandson");
                _related = true;
            }
            if (baseHero.Children.Any((Hero child) => child.Children.Any((Hero grandChild) => grandChild.Children.Contains(queriedHero))))
            {
                AddList(queriedHero.IsFemale ? "str_greatgranddaughter" : "str_greatgrandson");
                _related = true;
            }

            // Spouse
            if (baseHero.Spouse == queriedHero)
            {
                AddList(queriedHero.IsFemale ? "str_wife" : "str_husband");
                _related = true;
            }
            if (baseHero.Spouse is not null)
            {
                if (baseHero.Spouse.ExSpouses.Contains(queriedHero))
                {
                    AddList(baseHero.Spouse.IsFemale ? "str_wife_ex_husband" : "str_husband_ex_wife");
                    _related = true;
                }
            }
            // Ex-spouse
            if (baseHero.ExSpouses.Contains(queriedHero))
            {
                AddList(queriedHero.IsFemale ? "str_ex_wife" : "str_ex_husband");
                _related = true;
            }
            if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Spouse == queriedHero))
            {
                AddList(queriedHero.IsFemale ? "str_ex_husband_wife" : "str_ex_wife_husband");
                _related = true;
            }
            if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.ExSpouses.Contains(queriedHero)))
            {
                AddList(queriedHero.IsFemale ? "str_ex_husband_ex_wife" : "str_ex_wife_ex_husband");
                _related = true;
            }

            // Unrelated
            if (!_related)
            {
                // Put in-laws here so they don't conflict with actual blood relatives
                // Adapted from the original method
                if ((baseHero.Spouse?.Father) == queriedHero)
                {
                    AddList(baseHero.IsFemale ? "str_new_husband_fatherinlaw" : "str_new_wife_fatherinlaw");
                    _stepRelated = false;
                }
                else if ((baseHero.Spouse?.Mother) == queriedHero)
                {
                    AddList(baseHero.IsFemale ? "str_new_husband_motherinlaw" : "str_new_wife_motherinlaw");
                    _stepRelated = false;
                }
                if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Mother == queriedHero))
                {
                    AddList(baseHero.IsFemale ? "str_ex_husband_motherinlaw" : "str_ex_wife_motherinlaw");
                    _stepRelated = false;
                }
                else if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Father == queriedHero))
                {
                    AddList(baseHero.IsFemale ? "str_ex_husband_fatherinlaw" : "str_ex_wife_fatherinlaw");
                    _stepRelated = false;
                }
                if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Siblings.Contains(queriedHero)))
                {
                    if (baseHero.IsFemale)
                    {
                        AddList(queriedHero.IsFemale ? "str_ex_husband_sisterinlaw" : "str_ex_husband_brotherinlaw");
                        _stepRelated = false;
                    }
                    else
                    {
                        AddList(queriedHero.IsFemale ? "str_ex_wife_sisterinlaw" : "str_ex_wife_brotherinlaw");
                        _stepRelated = false;
                    }
                }
                // From previous mod version
                if (baseHero.Siblings.Any((Hero sibling) => sibling.Spouse == queriedHero))
                {
                    AddList(queriedHero.IsFemale ? "str_sisterinlaw" : "str_brotherinlaw");
                    _stepRelated = false;
                }
                else if (baseHero.Siblings.Any((Hero sibling) => sibling.ExSpouses.Contains(queriedHero)))
                {
                    AddList(queriedHero.IsFemale ? "str_ex_sisterinlaw" : "str_ex_brotherinlaw");
                    _stepRelated = false;
                }
                if (baseHero.Children.Any((Hero child) => child.Spouse == queriedHero))
                {
                    AddList(queriedHero.IsFemale ? "str_daughterinlaw" : "str_soninlaw");
                    _stepRelated = false;
                }
                else if (baseHero.Children.Any((Hero child) => child.ExSpouses.Contains(queriedHero)))
                {
                    AddList(queriedHero.IsFemale ? "str_ex_daughterinlaw" : "str_ex_soninlaw");
                    _stepRelated = false;
                }

                // Steprelatives
                // Criteria: not an in-law and not directly related
                if (baseHero.Father is not null)
                {
                    foreach (Hero exSpouse in baseHero.Father.ExSpouses)
                    {
                        // Method needs to find step parent
                        // Father's exspouse and exspouse's exspouses
                        StepRelatedToParent(exSpouse, queriedHero, baseHero);
                        foreach (Hero exSpouse2 in exSpouse.ExSpouses)
                        {
                            StepRelatedToParent(exSpouse2, queriedHero, baseHero);
                        }
                    }
                    // Also father's current spouse
                    if (baseHero.Father.Spouse is not null)
                    {
                        StepRelatedToParent(baseHero.Father.Spouse, queriedHero, baseHero);
                    }
                }
                if (baseHero.Mother is not null)
                {
                    foreach (Hero exSpouse in baseHero.Mother.ExSpouses)
                    {
                        StepRelatedToParent(exSpouse, queriedHero, baseHero);
                        foreach (Hero exSpouse2 in exSpouse.ExSpouses)
                        {
                            StepRelatedToParent(exSpouse2, queriedHero, baseHero);
                        }
                    }
                    if (baseHero.Mother.Spouse is not null)
                    {
                        StepRelatedToParent(baseHero.Mother.Spouse, queriedHero, baseHero);
                    }
                }
                if (baseHero.Spouse is not null)
                {
                    if (baseHero.Spouse.Children.Contains(queriedHero))
                    {
                        AddList(queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                    }
                    if (baseHero.Spouse.Children.Any((Hero child) => child.Children.Contains(queriedHero)))
                    {
                        AddList(queriedHero.IsFemale ? "str_stepgranddaughter" : "str_stepgrandson");
                    }
                    if (baseHero.Spouse.Children.Any((Hero child) => child.Children.Any((Hero grandChild) => grandChild.Children.Contains(queriedHero))))
                    {
                        AddList(queriedHero.IsFemale ? "str_stepgreatgranddaughter" : "str_stepgreatgrandson");
                    }
                    foreach (Hero exSpouse in baseHero.Spouse.ExSpouses)
                    {
                        if (exSpouse.Children.Contains(queriedHero))
                        {
                            AddList(queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                        }
                    }
                }
                if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Children.Contains(queriedHero)))
                {
                    AddList(queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                }
            }
            // Companion
            if (queriedHero.CompanionOf == baseHero.Clan)
            {
                AddList("str_companion");
            }
            // Relative if all else fails...
            if (_list.IsEmpty())
            {
                AddList("str_relative_of_player");
            }
            // Filter dupes just in case
            _list = _list.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            // Join titles
            string result = string.Join(", ", _list);
            TextObject textObject = new(result, null);

            // Adapted from original method
            if (queriedHero != null)
            {
                textObject.SetCharacterProperties("NPC", queriedHero.CharacterObject, false);
            }
            __result = textObject;

            // Skipping this method
            return false;
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
            if (!_stepRelated)
            {
                return;
            }
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