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

        private static bool Prefix(ref TextObject __result, Hero queriedHero, Hero baseHero)
        {
            _list = new List<string>();
            bool related = false;

            // Siblings
            if (baseHero.Siblings.Contains(queriedHero))
            {
                if (baseHero.Father == queriedHero.Father && baseHero.Mother == queriedHero.Mother)
                {
                    if (baseHero.Age == queriedHero.Age)
                    {
                        related = AddList("str_twin");
                    }
                    else
                    {
                        related = AddList(queriedHero.IsFemale ? "str_sister" : "str_brother");
                    }
                }
                else if (baseHero.Father == queriedHero.Father != (baseHero.Mother == queriedHero.Mother))
                {
                    related = AddList(queriedHero.IsFemale ? "str_halfsister" : "str_halfbrother");
                }
            }
            // Children
            else if (baseHero.Children.Contains(queriedHero))
            {
                related = AddList(queriedHero.IsFemale ? "str_daughter" : "str_son");
            }
            else if (baseHero.Siblings.Any((Hero sibling) => sibling.Children.Contains(queriedHero)))
            {
                related = AddList(queriedHero.IsFemale ? "str_niece" : "str_nephew");
            }
            else if (baseHero.Children.Any((Hero child) => child.Children.Contains(queriedHero)))
            {
                related = AddList(queriedHero.IsFemale ? "str_granddaughter" : "str_grandson");
            }
            else if (baseHero.Children.Any((Hero child) => child.Children.Any((Hero grandChild) => grandChild.Children.Contains(queriedHero))))
            {
                related = AddList(queriedHero.IsFemale ? "str_greatgranddaughter" : "str_greatgrandson");
            }
            else
            {   
                // Parents
                if (baseHero.Father == queriedHero)
                {
                    related = AddList("str_father");
                }
                if (baseHero.Mother == queriedHero)
                {
                    related = AddList("str_mother");
                }
                if (!related)
                {
                    RelatedToParent(baseHero.Father, queriedHero, ref related);
                    if (!related)
                    {
                        RelatedToParent(baseHero.Mother, queriedHero, ref related);
                    }
                }
            }

            // Spouse
            if (baseHero.Spouse == queriedHero)
            {
                related = AddList(queriedHero.IsFemale ? "str_wife" : "str_husband");
            }
            else if (baseHero.Spouse?.ExSpouses.Contains(queriedHero) ?? false)
            {
                related = AddList(baseHero.Spouse.IsFemale ? "str_wife_ex_husband" : "str_husband_ex_wife");
            }
            // Ex-spouse
            else if (baseHero.ExSpouses.Contains(queriedHero))
            {
                related = AddList(queriedHero.IsFemale ? "str_ex_wife" : "str_ex_husband");
            }
            else if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Spouse == queriedHero))
            {
                related = AddList(queriedHero.IsFemale ? "str_ex_husband_wife" : "str_ex_wife_husband");
            }
            else if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.ExSpouses.Contains(queriedHero)))
            {
                related = AddList(queriedHero.IsFemale ? "str_ex_husband_ex_wife" : "str_ex_wife_ex_husband");
            }

            // Unrelated
            if (!related)
            {
                bool isRelated = false;

                // Put unrelated relatives here so they don't conflict with actual blood relatives
                // Attached to family first
                if (baseHero.Spouse?.Father == queriedHero)
                {
                    isRelated = AddList("str_fatherinlaw");
                }
                else if (baseHero.Spouse?.Mother == queriedHero)
                {
                    isRelated = AddList("str_motherinlaw");
                }
                else if (baseHero.Spouse?.Siblings.Contains(queriedHero) ?? false)
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_sisterinlaw" : "str_brotherinlaw");
                }
                else if (baseHero.Spouse?.Father?.Siblings.Contains(queriedHero) ?? false)
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_auntinlaw" : "str_uncleinlaw");
                }
                else if (baseHero.Spouse?.Mother?.Siblings.Contains(queriedHero) ?? false)
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_auntinlaw" : "str_uncleinlaw");
                }
                else if (baseHero.Spouse?.Father?.Father == queriedHero)
                {
                    isRelated = AddList("str_grandfatherinlaw");
                }
                else if (baseHero.Spouse?.Mother?.Father == queriedHero)
                {
                    isRelated = AddList("str_grandfatherinlaw");
                }
                else if (baseHero.Spouse?.Father?.Mother == queriedHero)
                {
                    isRelated = AddList("str_grandmotherinlaw");
                }
                else if (baseHero.Spouse?.Mother?.Mother == queriedHero)
                {
                    isRelated = AddList("str_grandmotherinlaw");
                }
                else if (baseHero.Siblings.Any((Hero sibling) => sibling.Spouse == queriedHero))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_sisterinlaw" : "str_brotherinlaw");
                }
                else if (baseHero.Siblings.Any((Hero sibling) => sibling.Children.Any((Hero nieceNephew) => nieceNephew.Spouse == queriedHero)))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_nieceinlaw" : "str_nephewinlaw");
                }
                else if (baseHero.Children.Any((Hero child) => child.Spouse == queriedHero))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_daughterinlaw" : "str_soninlaw");
                }
                else if (baseHero.Children.Any((Hero child) => child.Children.Any((Hero grandChild) => grandChild.Spouse == queriedHero)))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_granddaughterinlaw" : "str_grandsoninlaw");
                }
                // Detached from family last
                else if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Mother == queriedHero))
                {
                    isRelated = AddList("str_ex_motherinlaw");
                }
                else if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Father == queriedHero))
                {
                    isRelated = AddList("str_ex_fatherinlaw");
                }
                else if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Siblings.Contains(queriedHero)))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_ex_sisterinlaw" : "str_ex_brotherinlaw");
                }
                else if (baseHero.Siblings.Any((Hero sibling) => sibling.ExSpouses.Contains(queriedHero)))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_ex_sisterinlaw" : "str_ex_brotherinlaw");
                }
                else if (baseHero.Siblings.Any((Hero sibling) => sibling.Children.Any((Hero nieceNephew) => nieceNephew.ExSpouses.Contains(queriedHero))))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_ex_nieceinlaw" : "str_ex_nephewinlaw");
                }
                else if (baseHero.Children.Any((Hero child) => child.ExSpouses.Contains(queriedHero)))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_ex_daughterinlaw" : "str_ex_soninlaw");
                }
                else if (baseHero.Children.Any((Hero child) => child.Children.Any((Hero grandChild) => grandChild.ExSpouses.Contains(queriedHero))))
                {
                    isRelated = AddList(queriedHero.IsFemale ? "str_ex_granddaughterinlaw" : "str_ex_grandsoninlaw");
                }

                // Steprelatives
                // Criteria: not an in-law and not directly related
                if (!isRelated)
                {
                    do
                    {
                        bool isStepRelated = false;
                        // to father
                        if (baseHero.Father is not null)
                        {
                            StepRelatedToParent(baseHero.Father.Spouse, queriedHero, baseHero, ref isStepRelated);
                            if (isStepRelated)
                            {
                                break;
                            }
                            foreach (Hero exSpouse in baseHero.Father.ExSpouses)
                            {
                                StepRelatedToParent(exSpouse, queriedHero, baseHero, ref isStepRelated);
                                if (isStepRelated)
                                {
                                    break;
                                }
                            }
                        }
                        // to mother
                        if (baseHero.Mother is not null)
                        {
                            StepRelatedToParent(baseHero.Mother.Spouse, queriedHero, baseHero, ref isStepRelated);
                            if (isStepRelated)
                            {
                                break;
                            }
                            foreach (Hero exSpouse in baseHero.Mother.ExSpouses)
                            {
                                StepRelatedToParent(exSpouse, queriedHero, baseHero, ref isStepRelated);
                                if (isStepRelated)
                                {
                                    break;
                                }
                            }
                        }
                        // to child
                        if (baseHero.Spouse?.Children.Contains(queriedHero) ?? false)
                        {
                            AddList(queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                        }
                        else if (baseHero.Spouse?.ExSpouses.Any(spouse => spouse.Children.Contains(queriedHero)) ?? false)
                        {
                            AddList(queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                        }
                        else if (baseHero.Spouse?.Children.Any(stepchild => stepchild.Children.Contains(queriedHero)) ?? false)
                        {
                            AddList(queriedHero.IsFemale ? "str_stepgranddaughter" : "str_stepgrandson");
                        }
                        else if (baseHero.Spouse?.Children.Any(stepchild => stepchild.Children.Any(grandChild => grandChild.Children.Contains(queriedHero))) ?? false)
                        {
                            AddList(queriedHero.IsFemale ? "str_stepgreatgranddaughter" : "str_stepgreatgrandson");
                        }
                        else if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Children.Contains(queriedHero)))
                        {
                            AddList(queriedHero.IsFemale ? "str_stepdaughter" : "str_stepson");
                        }
                    } while (false);
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
                AddList("str_relative");
            }

            // Filter out dupes
            _list = _list.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

            // Join titles together
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

        private static void RelatedToParent(Hero parent, Hero queriedHero, ref bool related)
        {
            if (parent is null)
            {
                return;
            }
            else if (parent.Siblings.Contains(queriedHero))
            {
                related = AddList(queriedHero.IsFemale ? "str_aunt" : "str_uncle");
            }
            else if (parent.Siblings.Any((Hero auntUncle) => auntUncle.Children.Contains(queriedHero)))
            {
                related = AddList("str_cousin");
            }
            else if (parent.Father == queriedHero)
            {
                related = AddList("str_grandfather");
            }
            else if (parent.Mother == queriedHero)
            {
                related = AddList("str_grandmother");
            }
            else if (parent.Father?.Father == queriedHero)
            {
                related = AddList("str_greatgrandfather");
            }
            else if (parent.Father?.Mother == queriedHero)
            {
                related = AddList("str_greatgrandmother");
            }
            else if (parent.Mother?.Father == queriedHero)
            {
                related = AddList("str_greatgrandfather");
            }
            else if (parent.Mother?.Mother == queriedHero)
            {
                related = AddList("str_greatgrandmother");
            }
        }

        // Step relatives
        private static void StepRelatedToParent(Hero stepParent, Hero queriedHero, Hero baseHero, ref bool stepRelated)
        {
            if (stepParent is null)
            {
                return;
            }
            else if (stepParent == queriedHero)
            {
                stepRelated = AddList(queriedHero.IsFemale ? "str_stepmother" : "str_stepfather");
            }
            else if (stepParent.Children.Contains(queriedHero) && !baseHero.Siblings.Contains(queriedHero))
            {
                stepRelated = AddList(queriedHero.IsFemale ? "str_stepsister" : "str_stepbrother");
            }
            else if (stepParent.Siblings.Contains(queriedHero))
            {
                stepRelated = AddList(queriedHero.IsFemale ? "str_stepaunt" : "str_stepuncle");
            }
            else if (stepParent.Children.Any((Hero stepsibling) => stepsibling.Children.Contains(queriedHero)))
            {
                stepRelated = AddList(queriedHero.IsFemale ? "str_stepniece" : "str_stepnephew");
            }
            else if (stepParent.Siblings.Any((Hero auntuncle) => auntuncle.Children.Contains(queriedHero)))
            {
                stepRelated = AddList("str_stepcousin");
            }
            else if (stepParent.Father == queriedHero)
            {
                stepRelated = AddList("str_stepgrandfather");
            }
            else if (stepParent.Father?.Father == queriedHero)
            {
                stepRelated = AddList("str_stepgreatgrandfather");
            }
            else if (stepParent.Father?.Mother == queriedHero)
            {
                stepRelated = AddList("str_stepgreatgrandmother");
            }
            else if (stepParent.Mother == queriedHero)
            {
                stepRelated = AddList("str_stepgrandmother");
            }
            else if (stepParent.Mother?.Father == queriedHero)
            {
                stepRelated = AddList("str_stepgreatgrandfather");
            }
            else if (stepParent.Mother?.Mother == queriedHero)
            {
                stepRelated = AddList("str_stepgreatgrandmother");
            }
        }

        private static bool AddList(string str)
        {
            _list.Add(FindText(str).ToString());
            return true;
        }

        private static TextObject FindText(string id)
        {
            return GameTexts.FindText(id, null);
        }
    }
}
