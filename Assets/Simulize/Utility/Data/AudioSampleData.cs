using System;
using Unity.Collections;
using UnityEngine;

namespace Simulize.Utility
{
    public struct AudioSampleData
    {
        public readonly int channels;
        public readonly int bits;
        public readonly int frequency;
        public readonly bool valid;

        private NativeArray<float> _data;

        public AudioSampleData(
            int channels,
            int bits,
            int frequency,
            NativeArray<float> data)
        {
            this.channels = channels;
            this.bits = bits;
            this.frequency = frequency;
            this._data = data;
            this.valid = true;
        }

        public AudioSampleDataReader ToReader(
            bool loop = false,
            bool stopped = false,
            int startAt = 0,
            int? length = null) =>
            new AudioSampleDataReader(
                this.channels,
                this.frequency,
                this._data.Slice(startAt, length ?? this._data.Length - startAt),
                loop,
                stopped
            );

        public void Dispose()
        {
            this._data.Dispose();
        }

        public AudioSampleData Initialize(int targetFrequency)
        {
            switch (this.channels)
            {
                case 1:
                    return new AudioSampleData(
                        2,
                        this.bits,
                        targetFrequency,
                        this.frequency == targetFrequency
                            ? this._data.Stereoify()
                            : this._data.ResampleMonaural(this.frequency, targetFrequency)
                    );

                case 2:
                    if (this.frequency == targetFrequency)
                    {
                        return this;
                    }

                    Debug.Log(this.frequency + "=>" + targetFrequency);
                    return new AudioSampleData(
                        2,
                        this.bits,
                        targetFrequency,
                        this.frequency == targetFrequency
                            ? new NativeArray<float>(this._data, Allocator.AudioKernel)
                            : this._data.ResampleStereo(this.frequency, targetFrequency)
                    );

                default:
                    throw new Exception($"Resampling unsupported channel count '{this.channels}'.");

            }
        }

        public byte[] ToByteArray() =>
            this._data.Reinterpret<byte>(4)
                .ToArray();
    }
}