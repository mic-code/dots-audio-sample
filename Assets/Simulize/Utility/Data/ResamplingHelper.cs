using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Simulize.Utility
{
    public static class ResamplingHelper
    {
        public static NativeArray<float> ResampleMonaural(this NativeArray<float> source, int fromFrequency, int toFrequency)
        {
            var sourceSamples = source.Length;
            var rate = Convert.ToSingle(toFrequency) / fromFrequency;
            var targetSamples = (int)math.floor(sourceSamples * rate);
            var result = new NativeArray<float>(targetSamples * 2, Allocator.Persistent);
            for (var sample = 0; sample < targetSamples; sample++)
            {
                var position = sample / rate;
                var positionFloor = math.floor(position);
                var positionFraction = position - positionFloor;
                var previousSampleIndex = (int)positionFloor;
                var nextSampleIndex = previousSampleIndex + 1;

                if (nextSampleIndex >= sourceSamples)
                {
                    result[sample] = 0f;
                    result[targetSamples + sample] = 0f;
                    continue;
                }

                var prevSample = source[previousSampleIndex + 0];
                var nextSample = source[nextSampleIndex + 0];

                result[sample] = prevSample + (nextSample - prevSample) * positionFraction;
                result[targetSamples + sample] = result[sample];
            }

            return result;
        }

        public static NativeArray<float> ResampleStereo(this NativeArray<float> source, int fromFrequency, int toFrequency)
        {
            var sourceSamples = source.Length / 2;
            var rate = Convert.ToSingle(toFrequency) / fromFrequency;
            var targetSamples = (int)math.floor(sourceSamples * rate);
            var result = new NativeArray<float>(targetSamples * 2, Allocator.Persistent);
            for (var sample = 0; sample < targetSamples; sample++)
            {
                var position = sample / rate;
                var positionFloor = math.floor(position);
                var positionFraction = position - positionFloor;
                var previousSampleIndex = (int)positionFloor;
                var nextSampleIndex = previousSampleIndex + 1;

                if (nextSampleIndex >= sourceSamples)
                {
                    result[sample] = 0f;
                    result[targetSamples + sample] = 0f;
                    continue;
                }

                var prevSampleL = source[previousSampleIndex * 2 + 0];
                var prevSampleR = source[previousSampleIndex * 2 + 1];
                var sampleL = source[nextSampleIndex * 2 + 0];
                var sampleR = source[nextSampleIndex * 2 + 1];

                result[sample] = prevSampleL + (sampleL - prevSampleL) * positionFraction;
                result[targetSamples + sample] = prevSampleR + (sampleR - prevSampleR) * positionFraction;
            }

            return result;
        }

        public static NativeArray<float> Stereoify(this NativeArray<float> source)
        {
            var result = new NativeArray<float>(source.Length * 2, Allocator.Persistent);
            for (var sample = 0; sample < source.Length; sample++)
            {
                result[sample] = source[sample];
                result[source.Length + sample] = source[sample];
            }

            return result;
        }
    }
}