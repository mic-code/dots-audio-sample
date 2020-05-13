using Unity.Audio;
using Unity.Burst;
using static Simulize.Utility.AudioKernels.PlayAudioSampleReaderNode;

namespace Simulize.Utility.AudioKernels
{
    [BurstCompile(CompileSynchronously = true)]
    public struct StartPlayingAudioSample : IAudioKernelUpdate<Parameters, Providers, PlayAudioSampleReaderNode>
    {
        private readonly AudioSampleDataReader _reader;
        private readonly Categories _category;

        public StartPlayingAudioSample(AudioSampleDataReader reader, Categories category)
        {
            this._reader = reader;
            this._category = category;
        }

        // This update job is used to kick off playback of the node.
        public void Update(ref PlayAudioSampleReaderNode audioKernel)
        {
            audioKernel.playing = true;
            audioKernel.category = this._category;
            audioKernel.reader = this._reader;
        }
    }

    public static class StartPlayingAudioSampleExtensions
    {
        public static void TriggerAudioSample(
            this DSPCommandBlock block,
            DSPNode node,
            AudioSampleDataReader reader,
            Categories category = Categories.OneShot)
        {
            // Kick off playback. This will be done in a better way in the future.
            block.UpdateAudioKernel<StartPlayingAudioSample, Parameters, Providers, PlayAudioSampleReaderNode>(
                new StartPlayingAudioSample(reader, category),
                node
            );
        }
    }
}