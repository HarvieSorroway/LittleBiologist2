using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LittleBiologist.LBioHUD;

namespace LittleBiologist
{
    /// <summary>
    /// 信息标签的页面类，具体示例可以参考StaticInfoPage和RelationShipLabelPage
    /// </summary>
    public class BaseLabelPage : LBioHUDGraphics
    {
        /// <summary>
        /// 竖向间隙，用于控制两行文本的间距
        /// </summary>
        public float VerticalGap => 5f;
        public float VerticalPageGap => 20f;//焯，忘记做什么用的了

        public readonly int thisPageIndex = 0;
        public int currentSubPageIndex = 0;

        public InfoLabel infoLabel;

        /// <summary>
        /// 子页面的HUDGraphics，与子页面的序号对应
        /// </summary>
        public Dictionary<int, List<LBioHUDGraphics>> subPageGraphics = new Dictionary<int, List<LBioHUDGraphics>>();
       
        /// <summary>
        /// 子页面的FNodes，与子页面的序号对应
        /// </summary>
        public Dictionary<int, List<FNode>> subPageFNodes = new Dictionary<int, List<FNode>>();

        public CreatureInfoGetter CreatureInfoGetter
        {
            get => infoLabel.creatureInfoGetter;
        }

        /// <summary>
        /// 子页面的数量，如果你有大于一页的需求，重写该属性
        /// </summary>
        public virtual int SubPageLength => 1;

        public BaseLabelPage(LBioHUD hud, InfoLabel infoLabel, int pageIndex) : base(hud, infoLabel, true)
        {
            thisPageIndex = pageIndex;
            this.infoLabel = infoLabel;
            LoadPagGraphics();
        }

        /// <summary>
        /// 加载子页面的所有HUDGraohics。
        /// 重写方法请先调用基方法，保证字典正确的初始化
        /// </summary>
        public virtual void LoadPagGraphics()
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

        /// <summary>
        /// 轮换子页面的方法
        /// </summary>
        public void AlternateSubPage()
        {
            currentSubPageIndex++;
            if (currentSubPageIndex >= SubPageLength) currentSubPageIndex = 0;
        }

        /// <summary>
        /// 切换主页面的方法
        /// </summary>
        /// <param name="currentPageIndex"></param>
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
