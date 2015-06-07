using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using HashInterface;
namespace OOPlaba4
{
    public class Adapter : IExt
    {
        IHash extObj;
        public Adapter(IHash obj)
        {
            
            extObj = obj;
        }
        public void saveChkSum(Object obj, string fileName)
        {
            byte[] str = Encoding.Default.GetBytes(obj.ToString());
            extObj.CreateHash(str, fileName);
        }
    }
}
