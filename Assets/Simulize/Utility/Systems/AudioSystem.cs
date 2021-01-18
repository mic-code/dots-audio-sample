using Simulize.Utility.AudioKernels;
using Unity.Audio;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static Simulize.Utility.AudioKernels.PlayAudioSampleReaderNode;

namespace Simulize.Utility
{
    public class AudioSystem : ComponentSystem
    {
        private DSPGraph _graph;
        private AudioOutputHandle _output;
        private NativeQueue<DSPNode> _available;
        private NativeList<DSPNode> _all;

        public DSPGraph Graph => this._graph;

        protected override void OnCreate()
        {
            this._available = new NativeQueue<DSPNode>(Allocator.Persistent);
            this._all = new NativeList<DSPNode>(Allocator.Persistent);

            var format = ChannelEnumConverter.GetSoundFormatFromSpeakerMode(AudioSettings.speakerMode);
            var channels = ChannelEnumConverter.GetChannelCountFromSoundFormat(format);
            AudioSettings.GetDSPBufferSize(out var bufferLength, out _);

            var sampleRate = AudioSettings.outputSampleRate;
            Debug.Log($"Audio SampleRate will be {sampleRate}");

            this._graph = DSPGraph.Create(format, channels, bufferLength, sampleRate);

            var driver = new DefaultDSPGraphDriver { Graph = this._graph };
            this._output = driver.AttachToDefaultOutput();

            this._graph.AddNodeEventHandler<AudioSampleNodeCompleted>(this.ReleaseNode);
        }

        protected override void OnUpdate()
        {
            this.Graph.Update();

            this.Entities.WithNone<AudioClipPlayerSystemState>()
                .ForEach<PlaysOneShotAudioSample>(this.InitializeState);

            this.Entities.WithNone<PlaysOneShotAudioSample>()
                .ForEach<AudioClipPlayerSystemState>(this.RemoveState);

            this.Entities.WithAll<PlaysOneShotAudioSample>()
                .ForEach<AudioClipPlayerSystemState>(this.UpdateState);
        }

        protected override void OnDestroy()
        {
            Debug.Log($"{nameof(AudioSystem)}.{nameof(this.OnDestroy)}");
            using (var block = this._graph.CreateCommandBlock())
            {
                // disconnect and release all nodes
                foreach (var node in this._all)
                {
                    block.ReleaseDSPNode(node);
                }
            }

            this._output.Dispose();

            this._graph.Dispose();

            this._available.Dispose();
            this._all.Dispose();
        }

        private void InitializeState(Entity entity, PlaysOneShotAudioSample trigger)
        {
            using (var block = this._graph.CreateCommandBlock())
            {
                var state = this.CreateState(block);
                var sample = trigger.samples.RandomSample();
                if (sample.HasValue)
                {
                    block.TriggerAudioSample(state.node, sample.Value.ToReader());
                }
                this.PostUpdateCommands.AddComponent(entity, state);
            }
        }

        private AudioClipPlayerSystemState CreateState(DSPCommandBlock block)
        {
            DSPNode node;
            if (this._available.Count > 0)
            {
                node = this._available.Dequeue();
            }
            else
            {
                Debug.Log($"Creating additional OneShot PlayAudioSampleNode #{this._all.Length}");
                node = block.CreateDSPNode<Parameters, Providers, PlayAudioSampleReaderNode>();
                block.AddOutletPort(node, 2);
                this._all.Add(node);
            }

            var connection = block.Connect(
                node,
                0,
                this._graph.RootDSP,
                0
            );

            return new AudioClipPlayerSystemState(node, connection);
        }


        private void UpdateState(Entity entity, ref AudioClipPlayerSystemState state)
        {
        }

        private void RemoveState(Entity entity, ref AudioClipPlayerSystemState state)
        {
            this.PostUpdateCommands.RemoveComponent<AudioClipPlayerSystemState>(entity);
            this._graph.TerminateAudioSampleNode(state.node);
        }

        private void ReleaseNode(DSPNode node, AudioSampleNodeCompleted @event)
        {
            if (@event.category != Categories.OneShot ||
                !this._available.IsCreated)
            {
                return;
            }

            using (var block = this._graph.CreateCommandBlock())
            {
                block.Disconnect(
                    node,
                    0,
                    this._graph.RootDSP,
                    0
                );
            }

            this._available.Enqueue(node);
        }
    }
}