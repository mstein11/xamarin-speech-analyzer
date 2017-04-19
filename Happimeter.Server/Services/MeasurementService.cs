using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Server.Data;
using Happimeter.Server.Models;

namespace Happimeter.Server.Services
{
    public class MeasurementService
    {
        public async Task SaveMeasurementPoint(TurnTakingMessage model)
        {
            var objToSave = new MeasurementPoint
            {
                IsSpeech = true,
                IsTurnTaking = model.GroupName != null,
                TurnTakingGroupName = model.GroupName,
                MeasurementTakenAtUtc = model.AudioTimeStamp.ToUniversalTime(),
                ReportedSpeechEnergy = model.Volumne,
                CustomIdentifier = "",
            };

            var context = ApplicationDbContext.Create();
            context.MeasurementPoints.Add(objToSave);
            await context.SaveChangesAsync();
        }

        public async Task SaveMeasurementPoints(List<TurnTakingMessage> models)
        {
            var listToSave = models.Select(x => new MeasurementPoint
            {
                IsSpeech = true,
                IsTurnTaking = x.GroupName != null,
                TurnTakingGroupName = x.GroupName,
                MeasurementTakenAtUtc = x.AudioTimeStamp.ToUniversalTime(),
                ReportedSpeechEnergy = x.Volumne,
                CustomIdentifier = "",
            });

            var context = ApplicationDbContext.Create();
            context.MeasurementPoints.AddRange(listToSave);
            await context.SaveChangesAsync();
        }
    }
}