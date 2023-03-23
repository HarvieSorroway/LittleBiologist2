using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LittleBiologist
{
    public class StaticInfoPage : BaseLabelPage
    {
        public override int SubPageLength => 2;

        public FLabel testLabel0;
        public FLabel testLabel1;

        public StaticInfoPage(LBioHUD part, InfoLabel infoLabel, int pageIndex) : base(part, infoLabel, pageIndex)
        {
            localPos = new Vector2(0f, -VerticalPageGap);
        }

        public override void LoadPageModules()
        {
            base.LoadPageModules();
            
            var pairs = CreatureInfoGetter.PersonalityInfo.Keys.ToArray();
            for (int i = 0; i < pairs.Length; i++)
            {
                FloatBar bar = new FloatBar(hud, this, new Vector2(0, 1f), Color.cyan * 0.5f, 100, pairs[i]);
                bar.localPos = new Vector2(0f, -25f * i);
                bar.FocusValue = CreatureInfoGetter.PersonalityInfo[pairs[i]];

                subPageGraphics[0].Add(bar);
            }
            if (subPageGraphics[0].Count == 0)//说明没有获取静态信息
            {
                var warningLabel = new FLabel(Custom.GetFont(), "I dont use personality");
                subPageFNodes[0].Add(warningLabel);
                fnodes.Add(warningLabel);
            }

            pairs = CreatureInfoGetter.ScavSkillInfo.Keys.ToArray();
            var vals = CreatureInfoGetter.ScavSkillInfo.Values.ToArray();

            if (pairs.Length > 0)
            {
                ScavSkillPentagon scavSkillPentagon = new ScavSkillPentagon(hud, this, pairs, vals, Color.green * 0.5f);
                scavSkillPentagon.localPos = new Vector2(40, -80f);
                subPageGraphics[1].Add(scavSkillPentagon);
            }
            else
            {
                var warningLabel = new FLabel(Custom.GetFont(), "I'm not scav >:");
                subPageFNodes[1].Add(warningLabel);
                fnodes.Add(warningLabel);
            }
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;

            if (subPageFNodes[0].Count != 0)
            {
                subPageFNodes[0][0].SetPosition(AnchorPos.x, AnchorPos.y);
                subPageFNodes[0][0].alpha = Alpha;
            }
            if (subPageFNodes[1].Count != 0)
            {
                subPageFNodes[1][0].SetPosition(AnchorPos.x, AnchorPos.y);
                subPageFNodes[1][0].alpha = Alpha;
            }
        }
    }
}
