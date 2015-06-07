using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using HashInterface;
using System.IO;
using System.Collections.ObjectModel;
using Weapons;
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
            
            byte[] str = Encoding.Default.GetBytes(obj.GetHashCode().ToString());
            extObj.CreateHash(str, fileName);
            
        }
    }
}
