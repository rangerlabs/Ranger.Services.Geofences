using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Geofences
{
    public class Breadcrumb
    {
        public Breadcrumb(string deviceId, string externalUserId, LngLat position, DateTime recordedAt, IEnumerable<KeyValuePair<string, string>> metadata, double accuracy = 0)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException($"{nameof(deviceId)} was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                throw new ArgumentException($"{nameof(externalUserId)} was null or whitespace");
            }

            this.DeviceId = deviceId;
            this.ExternalUserId = externalUserId;
            this.Position = position ?? throw new ArgumentNullException(nameof(position));
            this.RecordedAt = recordedAt;
            this.Accuracy = accuracy;
            this.Metadata = metadata;
        }

        public string DeviceId { get; }
        public string ExternalUserId { get; }
        public LngLat Position { get; }
        public double Accuracy { get; }
        public DateTime RecordedAt { get; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; }
    }
}