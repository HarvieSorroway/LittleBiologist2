using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleBiologist
{
    public class NoCustomPageException : Exception
    {
        string message = "";
        public NoCustomPageException(LBioExpand.LBioCustomPage customPage)
        {
            message = customPage.ToString() + " : LoadCustomPage returns null";
        }
        public override string Message => message;
    }
}
