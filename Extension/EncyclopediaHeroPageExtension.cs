using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs;
using System.Xml;

namespace ExtendedFamily.Extension
{
    [PrefabExtension("EncyclopediaHeroPage", "descendant::GridWidget[@Id='FamilyGrid']")]
    internal class EncyclopediaHeroPageExtensionAppend1 : PrefabExtensionInsertAsSiblingPatch
    {
        public override string Id => "FamilyTree";
        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new XmlDocument();
        public EncyclopediaHeroPageExtensionAppend1()
        {
            XmlDocument.LoadXml(@"
            <Widget Id = ""FamilyTreeWidget"" HeightSizePolicy =""CoverChildren"" WidthSizePolicy=""CoverChildren"">
	            <Children>
		            <ScrollablePanel Id=""BottomSideScrollablePanel""  HeightSizePolicy =""StretchToParent"" WidthSizePolicy =""StretchToParent"" InnerPanel =""BottomSideRect\BottomSideList"" ClipRect =""BottomSideRect"" MouseScrollAxis =""Horizontal"" VerticalAlignment =""Center"" HorizontalAlignment =""Center"" HorizontalScrollbar =""..\BottomSideScrollbar"" AutoHideScrollBars =""false"">
			            <Children>
				            <Widget Id=""BottomSideRect"" HeightSizePolicy =""StretchToParent"" WidthSizePolicy=""StretchToParent"" ClipContents=""true"" DoNotAcceptEvents=""true"">
					            <Children>
						            <ListPanel Id = ""BottomSideList"" HeightSizePolicy = ""CoverChildren"" WidthSizePolicy = ""CoverChildren"" DoNotAcceptEvents=""true"">
							            <Children>
								            <Widget HeightSizePolicy =""CoverChildren"" WidthSizePolicy=""CoverChildren"" DoNotAcceptEvents=""true"">
									            <Children>
										            <Widget DataSource = ""{FamilyTree}"" HeightSizePolicy = ""CoverChildren"" WidthSizePolicy = ""CoverChildren"" HorizontalAlignment = ""Center"" MarginTop=""30"">
											            <Children>
												            <EncyclopediaFamilyTreeNodeItem Id = ""InnerPanel"" HorizontalAlignment = ""Center"" />
											            </Children>
										            </Widget> 
									            </Children>
								            </Widget>
							            </Children>
						            </ListPanel>
					            </Children>
				            </Widget>
			            </Children>
		            </ScrollablePanel>
                    <ScrollbarWidget HeightSizePolicy =""Fixed"" WidthSizePolicy=""StretchToParent"" Id=""BottomSideScrollbar"" SuggestedHeight=""30"" MinValue = ""0"" MaxValue = ""100"" MarginRight=""10"" MarginBottom=""15"" MarginTop=""15"" AlignmentAxis=""Horizontal"" HorizontalAlignment=""Center"" VerticalAlignment=""Bottom"" Handle = ""BottomSideScrollbarHandle"" UpdateChildrenStates=""true"" >
			            <Children>
				            <Widget Id=""BottomSideScrollbarHandle"" WidthSizePolicy = ""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""50"" HorizontalAlignment = ""Center""/>
			            </Children>
		            </ScrollbarWidget>
	            </Children>
            </Widget>
            ");
        }
        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    [PrefabExtension("EncyclopediaHeroPage", "descendant::GridWidget[@Id='FamilyGrid']")]
    internal class EncyclopediaHeroPageExtensionAppend2 : PrefabExtensionInsertAsSiblingPatch
    {
        public override string Id => "FamilyTreeGroup";
        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new XmlDocument();
        public EncyclopediaHeroPageExtensionAppend2()
        {
            XmlDocument.LoadXml(@"
            <EncyclopediaDivider MarginTop=""20"" Parameter.Title=""@FamilyTreeText"" Parameter.ItemList=""..\FamilyTreeWidget"" />
            ");
        }
        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    [PrefabExtension("EncyclopediaHeroPage", "descendant::GridWidget[@Id='FamilyGrid']")]
    internal class EncyclopediaHeroPageExtensionReplace : PrefabExtensionReplacePatch
    {
        public override string Id => "ResizeFamilyGrid";
        private XmlDocument XmlDocument { get; } = new XmlDocument();
        public EncyclopediaHeroPageExtensionReplace()
        {
            XmlDocument.LoadXml(@"
            <GridWidget Id=""FamilyGrid"" DataSource=""{Family}"" ScaledSuggestedHeight=""100"" WidthSizePolicy=""CoverChildren"" HeightSizePolicy=""CoverChildren"" SuggestedWidth=""350"" SuggestedHeight=""350"" DefaultCellWidth=""100"" DefaultCellHeight=""150"" HorizontalAlignment=""Left"" ColumnCount=""7"" MarginTop=""50"" MarginLeft=""20"" >
                <ItemTemplate>
                    <EncyclopediaSubPageElement>
                        <Children>
                            <TextWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""CoverChildren"" SuggestedWidth=""100"" HorizontalAlignment=""Center"" VerticalAlignment=""Bottom"" Text=""@Role"" PositionYOffset=""-65"" Brush=""Encyclopedia.SubPage.Element.Name.Text""/>
                        </Children>
                    </EncyclopediaSubPageElement>
                </ItemTemplate>
            </GridWidget>
            ");
        }
        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}
