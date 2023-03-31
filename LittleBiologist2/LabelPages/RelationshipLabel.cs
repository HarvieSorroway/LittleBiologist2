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
        public override int SubPageLength => 3;

        public FLabel testLabel0;
        public FLabel testLabel1;

        public override float EffectiveHeight
        {
            get
            {
                switch (currentSubPageIndex)
                {
                    case 0:
                        return subPageFNodes[0].Count > 0 ? subPageFNodes[0].Count * 30f : 30f;
                    case 1:
                    case 2:
                        return 350f;
                }
                return 50f;
            }
        }

        public override float EffectiveWidth
        {
            get
            {
                switch (currentSubPageIndex)
                {
                    case 0:
                        return 150f;
                    case 1:
                    case 2:
                        return 400f;
                }
                return 50f;
            }
        }


        public RelationshipPage(LBioHUD part, InfoLabel infoLabel, int pageIndex) : base(part, infoLabel, pageIndex)
        {
            localPos = new Vector2(0f, -VerticalPageGap);
        }

        public override void LoadPagGraphics()
        {
            base.LoadPagGraphics();

            for (int i = 0; i < CreatureInfoGetter.dynamicFloatInfos.Count; i++)
            {
                var label = new FLabel(Custom.GetFont(), "") { anchorX = 0f, anchorY = 1f, scale = 1.1f };

                subPageFNodes[0].Add(label);
                fnodes.Add(label);
            }
            if (CreatureInfoGetter.dynamicFloatInfos.Count == 0)
            {
                var label = new FLabel(Custom.GetFont(), "I dont have relationships") { anchorX = 0f,anchorY = 1f, scale = 1.1f };
                subPageFNodes[0].Add(label);
                fnodes.Add(label);
            }

            subPageGraphics[1].Add(new RelationShipTrackerTimeLine(hud, this, 400f, Color.yellow) { localPos = new Vector2(0, -120f)});
            subPageGraphics[2].Add(new ItemTrackerTimeLine(hud, this, 400f, Color.yellow) { localPos = new Vector2(0, -120f) });
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
