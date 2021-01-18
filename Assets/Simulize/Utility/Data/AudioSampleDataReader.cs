using Unity.Collections;

namespace Simulize.Utility
{
    public struct AudioSampleDataReader
    {
        public readonly int channels;
        public readonly int frequency;
        public readonly NativeSlice<float> samples;
        public bool loop;
        public bool stopped;
        public int offset;
        public bool valid;

        public AudioSampleDataReader(
            int channels,
            int frequency,
            NativeSlice<float> samples,
            bool loop = false,
            bool stopped = false)
        {
            this.channels = channels;
            this.frequency = frequency;
            this.samples = samples;
            this.loop = loop;
            this.stopped = stopped;
            this.offset = 0;
            this.valid = true;
        }

        public int Read(NativeSlice<float> buffer, NativeSlice<float> buffer2)
        {

            if (stopped)
            {
                return 0;
            }

            var read = 0;

            while (true)
            {
                // we need to remember how much was available this round
                var available = samples.Length / channels - offset;
                var bufferSize = buffer.Length;

                // handle the easier scenario of the sample being longer than the buffer
                if (bufferSize < available)
                {
                    //if (buffer.Length != buffer2.Length)
                    //    throw new System.Exception($"{buffer.Length}!={buffer2.Length}");

                    // copy over the next block of data
                    buffer.CopyFrom(samples.Slice(offset, bufferSize));
                    buffer2.CopyFrom(samples.Slice(offset + samples.Length / channels, buffer2.Length));

                    // advance the sample forward
                    offset += bufferSize;

                    // the buffer is full
                    return (read + bufferSize) / channels;
                }

                // copy over the remaining sample data
                buffer.Slice(0, available)
                      .CopyFrom(samples.Slice(offset, available));
                buffer2.Slice(0, available)
                      .CopyFrom(samples.Slice(offset + samples.Length / channels, available));
                read += available;

                // reset the sample
                offset = 0;

                // stop if we are not set to loop
                if (!loop)
                {
                    stopped = true;
                    return read / channels;
                }

                if (buffer.Length == available)
                {
                    return read / channels;
                }

                // advance the buffer forward
                buffer = buffer.Slice(available);
            }
        }

        private void Reset()
        {
            this.offset = 0;
        }
    }
}