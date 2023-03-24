using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleBiologist.LBioExpand
{
    public static class LBioExpandCore
    {
        public static List<LBioCustomPage> customPages = new List<LBioCustomPage>();

        public static void RegistPage(LBioCustomPage customPage)
        {
            if (customPage.registed) return;
            customPages.Add(customPage);
            customPage.registed = true;
        }
    }
}
