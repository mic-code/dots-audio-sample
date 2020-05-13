using System;
using Unity.Entities;

namespace Simulize.Utility
{
    [Serializable]
    public struct ChangesMusicTrack : ISharedComponentData,
                                      IEquatable<ChangesMusicTrack>
    {
        public readonly AudioSampleSet samples;

        public ChangesMusicTrack(AudioSampleSet samples)
        {
            this.samples = samples;
        }

        public bool Equals(ChangesMusicTrack other) => Equals(this.samples, other.samples);

        public override bool Equals(object obj) => obj is ChangesMusicTrack other && this.Equals(other);

        public override int GetHashCode() => this.samples != null ? this.samples.GetHashCode() : 0;

        public static bool operator ==(ChangesMusicTrack left, ChangesMusicTrack right) => left.Equals(right);

        public static bool operator !=(ChangesMusicTrack left, ChangesMusicTrack right) => !left.Equals(right);
    }
}