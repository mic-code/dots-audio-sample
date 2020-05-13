using static Simulize.Utility.AudioKernels.PlayAudioSampleReaderNode;

namespace Simulize.Utility
{
    public struct AudioSampleNodeCompleted
    {
        public readonly Categories category;

        public AudioSampleNodeCompleted(Categories category)
        {
            this.category = category;
        }
    }
}