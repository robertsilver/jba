using jba.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace jba.Services
{
    public class ReadData : ProcessFile
    {
        public override List<int> ReadDataLine(string line)
        {
            Regex regex = new Regex(@"^\d$");
            List<int> precipitation = null;

            if (string.IsNullOrEmpty(line))
                return precipitation;

            if (!regex.IsMatch(line.TrimStart().Substring(0, 1)))
                return precipitation;

            precipitation = new List<int>();

            foreach (var number in line.Split(' '))
            {
                if (string.IsNullOrEmpty(number))
                    continue;

                precipitation.Add(Convert.ToInt32(number));
            }

            return precipitation;
        }

        public override double[] ReadFile(string line)
        {
            throw new System.NotImplementedException();
        }
    }
}
