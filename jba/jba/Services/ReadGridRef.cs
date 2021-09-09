using jba.Models;
using System;
using System.Collections.Generic;

namespace jba.Services
{
    public class ReadGridRef : ProcessFile
    {
        public override List<int> ReadDataLine(string line)
        {
            throw new NotImplementedException();
        }

        public override double[] ReadFile(string line)
        {
            var values = line.Substring("Grid-ref=".Length).Split(',');

            double[] gridValues = new double[2];
            gridValues[0] = Convert.ToDouble(values[0]);
            gridValues[1] = Convert.ToDouble(values[1]);

            return gridValues;
        }
    }
}
