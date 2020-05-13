using System.Collections.Generic;
using System.Linq;

namespace Simulize.Utility
{
    public class AudioSampleSet
    {
        public AudioSampleSet(params AudioSampleData[] data)
        {
            this.samples = data;
        }

        public AudioSampleSet(IEnumerable<AudioSampleData> samples)
            : this(samples.ToArray())
        {
        }

        public readonly AudioSampleData[] samples;
    }

    public static class AudioSampleSetExtensions
    {
        public static AudioSampleData? RandomSample(this AudioSampleSet set) =>
            set.samples.Length != 0
                ? set.samples[UnityEngine.Random.Range(0, set.samples.Length)]
                : (AudioSampleData?) null;
    }
}