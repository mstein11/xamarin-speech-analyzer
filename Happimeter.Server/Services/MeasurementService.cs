using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Server.Data;
using Happimeter.Server.Models;
using Happimeter.Shared;

namespace Happimeter.Server.Services
{
    public class MeasurementService
    {
        public async Task SaveMeasurementPoint(MeasurementMessage model)
        {
            var objToSave = new MeasurementPoint
            {
                IsSpeech = true,
                IsTurnTaking = model.IsTurnTaking,
                TurnTakingGroupName = model.TurnTakingGroupName,
                MeasurementTakenAtUtc = model.MeasurementTakenAtUtc,
                ReportedSpeechEnergy = model.ReportedSpeechEnergy,
                CustomIdentifier = model.CustomIdentifier,
            };

            var context = ApplicationDbContext.Create();
            context.MeasurementPoints.Add(objToSave);
            await context.SaveChangesAsync();
        }

        public async Task SaveMeasurementPoints(List<MeasurementMessage> models)
        {
            var listToSave = models.Select(x => new MeasurementPoint
            {
                IsSpeech = true,
                IsTurnTaking = x.TurnTakingGroupName != null,
                TurnTakingGroupName = x.TurnTakingGroupName,
                MeasurementTakenAtUtc = x.MeasurementTakenAtUtc,
                ReportedSpeechEnergy = x.ReportedSpeechEnergy,
                CustomIdentifier = "",
            });
            try
            {

                var context = ApplicationDbContext.Create();
                context.MeasurementPoints.AddRange(listToSave);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {

            }
        }
    }
}