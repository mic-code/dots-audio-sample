using Unity.Audio;
using Unity.Burst;
using static Simulize.Utility.AudioKernels.PlayAudioSampleReaderNode;

namespace Simulize.Utility.AudioKernels
{
    [BurstCompile(CompileSynchronously = true)]
    public struct TerminateAudioSampleNode : IAudioKernelUpdate<Parameters, Providers, PlayAudioSampleReaderNode>
    {
        // This update job is used to kick off playback of the node.
        public void Update(ref PlayAudioSampleReaderNode audioKernel)
        {
            if (!audioKernel.playing)
            {
                return;
            }

            audioKernel.terminate = true;
            audioKernel.reader.loop = false;
        }
    }

    public static class TerminateAudioSampleNodeExtensions
    {
        public static void TerminateAudioSampleNode(this DSPGraph graph, DSPNode node)
        {
            using (var block = graph.CreateCommandBlock())
            {
                block.TerminateAudioSampleNode(node);
            }
        }

        public static void TerminateAudioSampleNode(this DSPCommandBlock block, DSPNode node)
        {
            block.UpdateAudioKernel<TerminateAudioSampleNode, Parameters, Providers, PlayAudioSampleReaderNode>(
                new TerminateAudioSampleNode(), 
                node);
        }
    }
}