using System.Collections.Generic;

namespace INMS.Application.Models
{
    public class CorrelationResult
    {
        public int RootCauseDevice { get; set; }
        public List<int> Path { get; set; }
    }
}