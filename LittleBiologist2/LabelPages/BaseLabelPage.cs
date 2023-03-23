using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LittleBiologist.LBioHUD;

namespace LittleBiologist
{
    public class BaseLabelPage : LBioHUDGraphics
    {
        public float VerticalGap => 5f;
        public float VerticalPageGap => 20f;

        public readonly int thisPageIndex = 0;
        public int currentSubPageIndex = 0;

        public InfoLabel infoLabel;

        public Dictionary<int, List<LBioHUDGraphics>> subPageGraphics = new Dictionary<int, List<LBioHUDGraphics>>();
        public Dictionary<int, List<FNode>> subPageFNodes = new Dictionary<int, List<FNode>>();

        public CreatureInfoGetter CreatureInfoGetter
        {
            get => infoLabel.creatureInfoGetter;
        }

        public virtual int SubPageLength => 1;

        public BaseLabelPage(LBioHUD hud, InfoLabel infoLabel, int pageIndex) : base(hud, infoLabel)
        {
            thisPageIndex = pageIndex;
            this.infoLabel = infoLabel;
            LoadPageModules();
        }

        public virtual void LoadPageModules()
        {
            for (int i = 0; i < SubPageLength; i++)
            {
                subPageGraphics.Add(i, new List<LBioHUDGraphics>());
                subPageFNodes.Add(i, new List<FNode>());
            }

            for(int i = 0; i < SubPageLength; i++)
            {
                Plugin.Log(i.ToString() + subPageGraphics[i].ToString());
            }

            Plugin.Log(ToString() + " load page modules");
        }

        public override void InitSprites()
        {
            if (initiated) return;
            base.InitSprites();
        }

        public void AlternateSubPage()
        {
            currentSubPageIndex++;
            if (currentSubPageIndex >= SubPageLength) currentSubPageIndex = 0;
        }

        public void AlternateMainPage(int currentPageIndex)
        {
            if (currentPageIndex == thisPageIndex)
            {
                isVisible = true;
                alpha = 1f;
                currentSubPageIndex = 0;
            }
            else
            {
                alpha = 0f;
                isVisible = false;
            }

            UpdateVisibility();
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);

            if (!ShouldDrawOrUpdate) return;
        }

        public override void UpdateVisibility()
        {
            for (int i = 0; i < SubPageLength; i++)
            {
                //Plugin.Log(ToString() + i.ToString());
                //Plugin.Log((i == (currentSubPageIndex) & IsVisible).ToString());
                if (subPageFNodes[i].Count > 0)
                {
                    //Plugin.Log((subPageFNodes[i][0] as FLabel).text + " " + subPageFNodes[i][0].isVisible.ToString() + "->" + ((i == currentSubPageIndex) & IsVisible).ToString());
                    if (subPageFNodes[i][0].isVisible != ((i == currentSubPageIndex) & IsVisible))
                    {
                        foreach (var node in subPageFNodes[i])
                        {
                             node.isVisible = ((i == currentSubPageIndex) & IsVisible);
                        }
                    }
                }
                if (subPageGraphics[i].Count > 0)
                {
                    if (subPageGraphics[i][0].IsVisible != ((i == currentSubPageIndex) & IsVisible))
                    {
                        foreach (var graphics in subPageGraphics[i])
                        {
                            graphics.isVisible = ((i == currentSubPageIndex) & IsVisible);
                        }
                    }
                }
            }
        }
    }
}
