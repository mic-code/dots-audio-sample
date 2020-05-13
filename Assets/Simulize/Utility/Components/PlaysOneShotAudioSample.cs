using System;
using Unity.Entities;

namespace Simulize.Utility
{
    [Serializable]
    public struct PlaysOneShotAudioSample : ISharedComponentData,
                                            IEquatable<PlaysOneShotAudioSample>
    {
        public readonly AudioSampleSet samples;

        public PlaysOneShotAudioSample(AudioSampleSet samples)
        {
            this.samples = samples;
        }

        #region Equality

        public bool Equals(PlaysOneShotAudioSample other) => Equals(this.samples, other.samples);

        public override bool Equals(object obj) => obj is PlaysOneShotAudioSample other && this.Equals(other);

        public override int GetHashCode() => this.samples.GetHashCode();

        public static bool operator ==(PlaysOneShotAudioSample left, PlaysOneShotAudioSample right) => left.Equals(right);

        public static bool operator !=(PlaysOneShotAudioSample left, PlaysOneShotAudioSample right) => !left.Equals(right);

        #endregion
    }
}