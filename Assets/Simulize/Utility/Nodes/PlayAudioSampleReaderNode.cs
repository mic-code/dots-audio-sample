using Unity.Audio;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using static Simulize.Utility.AudioKernels.PlayAudioSampleReaderNode;

namespace Simulize.Utility.AudioKernels
{
    [BurstCompile(CompileSynchronously = true)]
    public struct PlayAudioSampleReaderNode : IAudioKernel<Parameters, Providers>
    {
        public enum Parameters { }
        public enum Providers { }

        public enum Categories { OneShot, Music }

        /// <summary>
        /// The sample reader that has been assigned to this node.
        /// </summary>
        public AudioSampleDataReader reader;

        /// <summary>
        /// The category this node has been placed in. This makes it way to the event.
        /// </summary>
        public Categories category;

        /// <summary>
        /// Whether the node is currently playing or not.
        /// </summary>
        public bool playing;

        /// <summary>
        /// Set this to stop playing on the next round.
        /// </summary>
        public bool terminate;

        /// <summary>
        /// Whether to fire the completed event on terminations or not.
        /// </summary>
        public bool notifyOnTerminate;

        public void Initialize()
        {
        }

        public void Execute(ref ExecuteContext<Parameters, Providers> context)
        {
            if (!this.playing)
            {
                return;
            }

            // end immediately when main thread requested the node to terminate
            if (this.terminate &&
                this.notifyOnTerminate)
            {
                // notify the main thread
                context.PostEvent(new AudioSampleNodeCompleted(this.category));
                this.playing = false;
                return;
            }

            var output = context.Outputs.GetSampleBuffer(0);

            var read = this.reader.Read(output.GetBuffer(0).Slice(), output.GetBuffer(1).Slice());
            if (read == output.GetBuffer(1).Length / 2)
            {
                return;
            }

            // notify the main thread
            context.PostEvent(new AudioSampleNodeCompleted(this.category));
            this.playing = false;
        }

        public void Dispose()
        {
        }
    }
}