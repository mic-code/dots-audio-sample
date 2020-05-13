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

        public int Read(NativeSlice<float> buffer)
        {
            if (this.stopped)
            {
                return 0;
            }

            var read = 0;

            while (true)
            {
                // we need to remember how much was available this round
                var available = this.samples.Length - this.offset;

                // handle the easier scenario of the sample being longer than the buffer
                if (buffer.Length < available)
                {
                    var bufferSize = buffer.Length / this.channels * this.channels;

                    // copy over the next block of data
                    buffer.CopyFrom(this.samples.Slice(this.offset, bufferSize));

                    // advance the sample forward
                    this.offset += bufferSize;

                    // the buffer is full
                    return (read + bufferSize) / this.channels;
                }

                // copy over the remaining sample data
                buffer.Slice(0, available)
                      .CopyFrom(this.samples.Slice(this.offset));
                read += available;

                // reset the sample
                this.offset = 0;

                // stop if we are not set to loop
                if (!this.loop)
                {
                    this.stopped = true;
                    return read / this.channels;
                }

                if (buffer.Length == available)
                {
                    return read / this.channels;
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