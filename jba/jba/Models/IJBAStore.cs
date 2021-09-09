using System.Collections.Generic;

namespace jba.Models
{
    public interface IJBAStore
    {
        void StoreData(List<PrecipitationData> data);
    }
}