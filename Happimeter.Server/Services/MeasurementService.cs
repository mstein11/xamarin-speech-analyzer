using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Server.Data;
using Happimeter.Shared;

namespace Happimeter.Server.Services
{
    public class MeasurementService
    {
        private ApplicationDbContext DbContext { get; set; }
        public MeasurementService()
        {
            DbContext = ApplicationDbContext.Create();
        }
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


            DbContext.MeasurementPoints.Add(objToSave);
            await DbContext.SaveChangesAsync();
        }

        public async Task SaveMeasurementPoints(List<MeasurementMessage> models)
        {
            var listToSave = models.Select(x => new MeasurementPoint
            {
                IsSpeech = true,
                IsTurnTaking = x.IsTurnTaking,
                TurnTakingGroupName = x.TurnTakingGroupName,
                MeasurementTakenAtUtc = x.MeasurementTakenAtUtc,
                ReportedSpeechEnergy = x.ReportedSpeechEnergy,
                CustomIdentifier = x.CustomIdentifier,
            });
            try
            {
                DbContext.Configuration.AutoDetectChangesEnabled = false;
                DbContext.Configuration.ValidateOnSaveEnabled = false;
                DbContext.MeasurementPoints.AddRange(listToSave);
                await DbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {

            }
        }

        public async Task<MeasurementStatisticsModel> GetMeasurements()
        {
            var groups = await DbContext.MeasurementPoints.AsNoTracking().Where(x => x.IsTurnTaking).GroupBy(x => x.TurnTakingGroupName).ToListAsync();

            var groupLifeSpan = groups.Select(x =>
            {
                var tmp = x.Select(measure => measure.MeasurementTakenAtUtc).OrderBy(date => date);
                return new {groupName = x.Key, minDate = tmp.FirstOrDefault(), maxDate = tmp.LastOrDefault(), secondsInTimeSpan = new List<DateTime>()};
            });


            var loudestInGroups = new Dictionary<string, List<MeasurementPoint>>();
            var loudestShare = new Dictionary<string, Dictionary<string,int>>();
            var measurementPoints = 0;
            foreach (var group in groupLifeSpan)
            {
                loudestShare.Add(group.groupName,new Dictionary<string, int>());
                var namesInGroup =
                    groups.Where(x => x.Key == group.groupName)
                        .SelectMany(x => x.Select(y => y.CustomIdentifier))
                        .Distinct();

                foreach (var name in namesInGroup)
                {
                    loudestShare[group.groupName][name] = 0;
                }

                if (!loudestInGroups.ContainsKey(group.groupName))
                {
                    loudestInGroups.Add(group.groupName,new List<MeasurementPoint>());
                }
                var lastDate = group.minDate;
                while (group.minDate < group.maxDate && lastDate <= group.maxDate)
                {
                    group.secondsInTimeSpan.Add(lastDate);


                        var loudest =
                            groups.First(x => x.Key == group.groupName)
                                .Where(
                                    x =>
                                        x.MeasurementTakenAtUtc < lastDate.AddSeconds(1) &&
                                        x.MeasurementTakenAtUtc > lastDate.AddSeconds(-1))
                                .OrderByDescending(x => x.ReportedSpeechEnergy)
                                .FirstOrDefault();
                    if (loudest == null)
                    {
                        lastDate = lastDate.AddSeconds(1);
                        continue;
                        
                    }

                    if (!loudestShare[group.groupName].ContainsKey(loudest.CustomIdentifier))
                    {
                        loudestShare[group.groupName][loudest.CustomIdentifier] = 1;
                    }
                    else
                    {
                        loudestShare[group.groupName][loudest.CustomIdentifier]++;
                    }
                    measurementPoints++;
                    loudestInGroups[group.groupName].Add(loudest);
                    lastDate = lastDate.AddSeconds(1);
                }
            }

            var model = new MeasurementStatisticsModel
            {
                LoudestTimeline = loudestInGroups,
                LoudestShare =
                    loudestShare.Select(
                            x =>
                                new KeyValuePair<string, Dictionary<string, double>>(x.Key,
                                    x.Value.Select(
                                            share =>
                                                new KeyValuePair<string, double>(share.Key,
                                                    (double) share.Value / (double) measurementPoints))
                                        .ToDictionary(pair => pair.Key, pair => pair.Value)))
                        .ToDictionary(pair => pair.Key, pair => pair.Value)
            };

            return model;
        }
    }

    public class MeasurementStatisticsModel
    {
        public Dictionary<string, List<MeasurementPoint>> LoudestTimeline { get; set; }
        public Dictionary<string, Dictionary<string, double>> LoudestShare { get; set; }
    }
}