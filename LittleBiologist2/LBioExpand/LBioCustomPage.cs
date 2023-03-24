using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LittleBiologist.CreatureInfoGetter;

namespace LittleBiologist.LBioExpand
{
    /// <summary>
    /// 用于添加自定义标签页的类
    /// </summary>
    public class LBioCustomPage
    {
        public bool registed = false;
        public List<DynamicInfoGetter> dynamicInfoGetters = new List<DynamicInfoGetter>();
        public List<DynamicFloatInfo> dynamicFloatInfos = new List<DynamicFloatInfo>();
        
        /// <summary>
        /// 返回自定义的标签页，用于在InfoLabel加载时添加
        /// </summary>
        /// <param name="hud">hud本体</param>
        /// <param name="infoLabel">infoLabel本体</param>
        /// <param name="pageIndex">当前页面的序号，自动计算</param>
        /// <returns></returns>
        public virtual BaseLabelPage LoadCustomPage(LBioHUD hud,InfoLabel infoLabel,int pageIndex)
        {
            return null;
        }

        /// <summary>
        /// 获取静态信息时调用该方法，例如 Personality 相关信息
        /// </summary>
        /// <param name="target"></param>
        public virtual void GetStaticInfo(Creature target)
        {
        }

        /// <summary>
        /// 将自定义的 DynamicFloatInfo 添加到 CreatureInfoGetters 中。
        /// dynamicFloatInfos 会在调用前清空，因此请在重写中添加自己的后再调用原方法
        /// 注：每次 CreatureInfoGetters 调用 SetTarget 时会调用该方法
        /// </summary>
        /// <param name="creatureInfoGetter"></param>
        public virtual void InitDynamicInfos(CreatureInfoGetter creatureInfoGetter)
        {
            if(dynamicFloatInfos.Count > 0)
            {
                foreach(var info in dynamicFloatInfos)
                {
                    creatureInfoGetter.customFloatInfos.Add(info);
                }
            }
        }

        /// <summary>
        /// 将自定义的 DynamicInfoGetter 添加到 CreatureInfoGetters 中。
        /// dynamicInfoGetters 调用前会清空，因此请在重写中添加自己的后再调用原方法
        /// 注：CreatureInfoGetter 类初始化时调用该方法
        /// </summary>
        /// <param name="creatureInfoGetter"></param>
        public virtual void AddDynamicGetters(CreatureInfoGetter creatureInfoGetter)
        {
            if (dynamicInfoGetters.Count > 0)
            {
                foreach (var getter in dynamicInfoGetters)
                {
                    creatureInfoGetter.customInfoGetter.Add(getter);
                }
            }
        }
    }
}
