using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Happimeter.Server.Models;
using Happimeter.Server.Services;
using Happimeter.Shared.DataStructures;

namespace Happimeter.Server.Controllers
{
    public class TurnTakingController : ApiController
    {
        /// <summary>
        ///     GroupNames => Users => Data
        /// </summary>
        private static Dictionary<string, Dictionary<string,SlidingBuffer<TurnTakingMessage>>> Groups { get; set; }

        private MeasurementService _measurementService;

        public TurnTakingController()
        {
            _measurementService = new MeasurementService();
            if (Groups == null)
            {
                Groups = new Dictionary<string, Dictionary<string, SlidingBuffer<TurnTakingMessage>>>();
            }
            
        }

        public async Task<object> ReportAndGetForGroup(List<TurnTakingMessage> message)
        {
            await _measurementService.SaveMeasurementPoints(message);


            var groupName = message.FirstOrDefault()?.GroupName;
            var userName = message.FirstOrDefault()?.UserId;

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
                    Groups.Add(groupName, new Dictionary<string, SlidingBuffer<TurnTakingMessage>>());
                }

                if (!Groups[groupName].ContainsKey(userName))
                {
                    Groups[groupName].Add(userName, new SlidingBuffer<TurnTakingMessage>(60));
                }

                Groups[groupName][userName].Add(turnTakingMessage);
            }


            var referenceTime  = DateTime.UtcNow;
            var allLatestMessages = Groups[groupName].Select(
                x =>
                    x.Value.Where(turnTakingMessage => turnTakingMessage.AudioTimeStamp.AddSeconds(3) > referenceTime)
                        .OrderByDescending(y => y.AudioTimeStamp)
                        .FirstOrDefault()
            ).Where(x => x != null);

            return allLatestMessages;
        }
    }
}
