using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Happimeter.Server.Services;
using Happimeter.Shared;
using Happimeter.Shared.DataStructures;

namespace Happimeter.Server.Controllers.Api
{
    public class MeasurementController : ApiController
    {
        /// <summary>
        ///     GroupNames => Users => Data
        /// </summary>
        private static Dictionary<string, Dictionary<string,SlidingBuffer<MeasurementMessage>>> Groups { get; set; }

        private static Dictionary<string, string> GroupNameToGroupNameWithDate { get; set; }

        private MeasurementService _measurementService;

        public MeasurementController()
        {
            _measurementService = new MeasurementService();
            if (Groups == null)
            {
                Groups = new Dictionary<string, Dictionary<string, SlidingBuffer<MeasurementMessage>>>();
            }

            if (GroupNameToGroupNameWithDate == null)
            {
                GroupNameToGroupNameWithDate = new Dictionary<string, string>();
            }
            
        }

        public async Task<object> ReportAndGetForGroup(List<MeasurementMessage> message)
        {
            var turnTaking = message.Where(x => x.IsTurnTaking).ToList();
            var noTurnTaking = message.Where(x => !x.IsTurnTaking).ToList();

            var returnModel = new MeasurementResponse();
            if (turnTaking.Any())
            {
                returnModel.AllLatestMessages = await HandleTurnTaking(turnTaking);
                returnModel.HasTurnTaking = true;
            }

            if (noTurnTaking.Any())
            {
                await HandleSpeechOnly(noTurnTaking);
                returnModel.HasSpeechEnergy = true;
            }

            return returnModel;
        }

        private async Task<object> HandleSpeechOnly(List<MeasurementMessage> messages)
        {
            await _measurementService.SaveMeasurementPoints(messages);
            return "success";
        }

        private async Task<List<MeasurementMessage>> HandleTurnTaking(List<MeasurementMessage> messages)
        {
            var groupName = messages.FirstOrDefault()?.TurnTakingGroupName;
            var userName = messages.FirstOrDefault()?.CustomIdentifier;

            if (string.IsNullOrEmpty(groupName))
            {
                groupName = "-";
            }

            if (string.IsNullOrEmpty(userName))
            {
                userName = "-";
            }
            var newGroupName = groupName;

            foreach (var turnTakingMessage in messages)
            {
                if (!turnTakingMessage.IsTurnTaking)
                {
                    continue;
                }

                if (!Groups.ContainsKey(groupName) ||
                    Groups[groupName].SelectMany(x => x.Value)
                        .OrderByDescending(x => x.ReportedSpeechEnergy)
                        .FirstOrDefault()?.MeasurementTakenAtUtc < DateTime.UtcNow - TimeSpan.FromMinutes(5))
                {
                    Groups.Add(groupName, new Dictionary<string, SlidingBuffer<MeasurementMessage>>());
                    newGroupName = groupName + DateTime.UtcNow.ToString();
                    GroupNameToGroupNameWithDate.Remove(groupName);
                    GroupNameToGroupNameWithDate.Add(groupName, newGroupName);
                }
                else
                {
                    newGroupName = GroupNameToGroupNameWithDate[groupName];
                }

                if (!Groups[groupName].ContainsKey(userName))
                {
                    Groups[groupName].Add(userName, new SlidingBuffer<MeasurementMessage>(60));
                }

                Groups[groupName][userName].Add(turnTakingMessage);
            }


            var referenceTime = DateTime.UtcNow;
            var allLatestMessages = Groups[groupName].Select(
                x =>
                    x.Value.Where(turnTakingMessage => turnTakingMessage.MeasurementTakenAtUtc.AddSeconds(3) > referenceTime)
                        .OrderByDescending(y => y.MeasurementTakenAtUtc)
                        .FirstOrDefault()
            ).Where(x => x != null);
            messages.ForEach(x => x.TurnTakingGroupName = GroupNameToGroupNameWithDate[groupName]);
            await _measurementService.SaveMeasurementPoints(messages);

            return allLatestMessages.ToList();
        }
    }
}
