using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;

namespace ExtendedFamily.Encyclopedia
{
    public class EncyclopediaFamilyTreeNodeVM : ViewModel
    {
        private MBBindingList<EncyclopediaFamilyTreeNodeVM> _familyBranch;

        private HeroVM _familyMember;

        public EncyclopediaFamilyTreeNodeVM(Hero ancestor, Hero hero)
        {
            FamilyBranch = new MBBindingList<EncyclopediaFamilyTreeNodeVM>();
            FamilyMember = new HeroVM(ancestor);
            if (ancestor.Children is not null)
            {
                foreach (Hero child in ancestor.Children)
                {
                    FamilyBranch.Add(new EncyclopediaFamilyTreeNodeVM(child, hero));
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            FamilyBranch.ApplyActionOnAllItems(delegate (EncyclopediaFamilyTreeNodeVM x)
            {
                x.RefreshValues();
            });
            FamilyMember.RefreshValues();
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFamilyTreeNodeVM> FamilyBranch
        {
            get
            {
                return _familyBranch;
            }
            set
            {
                if (value != _familyBranch)
                {
                    _familyBranch = value;
                    OnPropertyChanged("FamilyBranch");
                }
            }
        }

        [DataSourceProperty]
        public HeroVM FamilyMember
        {
            get
            {
                return _familyMember;
            }
            set
            {
                if (value != _familyMember)
                {
                    _familyMember = value;
                    OnPropertyChanged("FamilyMember");
                }
            }
        }
    }
}
