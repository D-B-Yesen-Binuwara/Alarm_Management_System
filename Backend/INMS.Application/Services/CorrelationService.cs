using System.Collections.Generic;
using INMS.Application.Models;

namespace INMS.Application.Services
{
    public class CorrelationService
    {
        private Dictionary<int, int> parentMap = new Dictionary<int, int>()
        {
            {2,1},
            {3,2},
            {4,3}
        };

        public CorrelationResult FindRootCause(int deviceId)
        {
            List<int> path = new List<int>();
            int current = deviceId;

            path.Add(current);

            while (parentMap.ContainsKey(current))
            {
                current = parentMap[current];
                path.Add(current);
            }

            return new CorrelationResult
            {
                RootCauseDevice = current,
                Path = path
            };
        }
    }
}