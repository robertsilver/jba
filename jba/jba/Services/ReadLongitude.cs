using jba.Models;
using System.Collections.Generic;

namespace jba.Services
{
    public class ReadLongitude : ProcessFile
    {
        public override List<int> ReadDataLine(string line)
        {
            throw new System.NotImplementedException();
        }

        public override double[] ReadFile(string line)
        {
            return base.ReadStandardHeader(line, "[Long=");
        }
    }
}
