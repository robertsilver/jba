using jba.Models;
using System.Collections.Generic;

namespace jba.Services
{
    public class ReadLatitude : ProcessFile
    {
        public override List<int> ReadDataLine(string line)
        {
            throw new System.NotImplementedException();
        }

        public override double[] ReadFile(string line)
        {
            return base.ReadStandardHeader(line, "[Lati=");
        }
    }
}
