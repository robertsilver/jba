using jba.Models;
using System;
using System.Collections.Generic;

namespace jba.Services
{
    public class ReadGridXY : ProcessFile
    {
        public override List<int> ReadDataLine(string line)
        {
            throw new NotImplementedException();
        }

        public override double[] ReadFile(string line)
        {
            return base.ReadStandardHeader(line, "[GridX,Y=");
        }        
    }
}
