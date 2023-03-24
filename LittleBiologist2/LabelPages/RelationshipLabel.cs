using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LittleBiologist
{
    public class RelationshipPage : BaseLabelPage
    {
        public override int SubPageLength => 2;

        public FLabel testLabel0;
        public FLabel testLabel1;

        public RelationshipPage(LBioHUD part, InfoLabel infoLabel, int pageIndex) : base(part, infoLabel, pageIndex)
        {
            localPos = new Vector2(0f, -VerticalPageGap);
        }

        public override void LoadPagGraphics()
        {
            base.LoadPagGraphics();

            for (int i = 0; i < CreatureInfoGetter.dynamicFloatInfos.Count; i++)
            {
                var label = new FLabel(Custom.GetFont(), "");

                subPageFNodes[0].Add(label);
                fnodes.Add(label);
            }
            if (CreatureInfoGetter.dynamicFloatInfos.Count == 0)
            {
                var label = new FLabel(Custom.GetFont(), "I dont have relationships");
                subPageFNodes[0].Add(label);
                fnodes.Add(label);
            }

            subPageGraphics[1].Add(new RelationShipTrackerTimeLine(hud, this, 400f, Color.yellow) { localPos = new Vector2(0, -60f),});
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;

            float biasY = 0f;
            for (int i = 0; i < subPageFNodes[0].Count; i++)
            {
                if (CreatureInfoGetter.dynamicInfoValid && CreatureInfoGetter.dynamicFloatInfos.Count > 0)
                {
                    (subPageFNodes[0][i] as FLabel).text = CreatureInfoGetter.dynamicFloatInfos[i].name + String.Format(" : {0:f2}", CreatureInfoGetter.dynamicFloatInfos[i].floatInfo);
                }
                subPageFNodes[0][i].SetPosition(AnchorPos.x, AnchorPos.y - biasY);
                biasY += (subPageFNodes[0][i] as FLabel).textRect.height + VerticalGap;

                subPageFNodes[0][i].alpha = Alpha;
            }
        }
    }
}
