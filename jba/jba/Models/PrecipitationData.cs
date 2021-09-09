using System;

namespace jba.Models
{
    public class PrecipitationData
    {
        public int Id { get; set; }

        public double Xref { get; set; }

        public double Yref { get; set; }

        public DateTime Date { get; set; }

        public double Value { get; set; }
    }
}