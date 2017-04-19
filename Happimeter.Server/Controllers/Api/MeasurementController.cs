using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Happimeter.Server.Services;
using Happimeter.Shared;
using Happimeter.Shared.DataStructures;

namespace Happimeter.Server.Controllers
{
    public class MeasurementController : ApiController
    {
        /// <summary>
        ///     GroupNames => Users => Data
        /// </summary>
        private static Dictionary<string, Dictionary<string,SlidingBuffer<MeasurementMessage>>> Groups { get; set; }

        private MeasurementService _measurementService;

        public MeasurementController()
        {
            _measurementService = new MeasurementService();
            if (Groups == null)
            {
                Groups = new Dictionary<string, Dictionary<string, SlidingBuffer<MeasurementMessage>>>();
            }
            
        }

        public async Task<object> ReportAndGetForGroup(List<MeasurementMessage> message)
        {
            await _measurementService.SaveMeasurementPoints(message);


            var groupName = message.FirstOrDefault()?.TurnTakingGroupName;
            var userName = message.FirstOrDefault()?.CustomIdentifier;

            if (string.IsNullOrEmpty(groupName))
            {
                groupName = "-";
            }

            if (string.IsNullOrEmpty(userName))
            {
                userName = "-";
            }

            foreach (var turnTakingMessage in message)
            {
                if (!Groups.ContainsKey(groupName))
                {
                    Groups.Add(groupName, new Dictionary<string, SlidingBuffer<MeasurementMessage>>());
                }

                if (!Groups[groupName].ContainsKey(userName))
                {
                    Groups[groupName].Add(userName, new SlidingBuffer<MeasurementMessage>(60));
                }

                Groups[groupName][userName].Add(turnTakingMessage);
            }


            var referenceTime  = DateTime.UtcNow;
            var allLatestMessages = Groups[groupName].Select(
                x =>
                    x.Value.Where(turnTakingMessage => turnTakingMessage.MeasurementTakenAtUtc.AddSeconds(3) > referenceTime)
                        .OrderByDescending(y => y.MeasurementTakenAtUtc)
                        .FirstOrDefault()
            ).Where(x => x != null);

            return allLatestMessages;
        }
    }
}
