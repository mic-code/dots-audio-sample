using Simulize.Utility.AudioKernels;
using Unity.Audio;
using Unity.Entities;
using UnityEngine;
using static Simulize.Utility.AudioKernels.PlayAudioSampleReaderNode;

namespace Simulize.Utility
{
    [UpdateAfter(typeof(AudioSystem))]
    public class MusicManager : ComponentSystem
    {
        private DSPGraph _graph;
        private AudioClipPlayerSystemState _current;
        private AudioClipPlayerSystemState _previous;

        protected override void OnCreate()
        {
            this._graph = this.World.GetOrCreateSystem<AudioSystem>()
                              .Graph;

            using (var block = this._graph.CreateCommandBlock())
            {
                this._current = this.CreateNode(block);
                this._previous = this.CreateNode(block);
            }
        }

        protected override void OnUpdate()
        {
            this.Entities.ForEach(
                (Entity entity, ChangesMusicTrack track) =>
                {
                    this.PostUpdateCommands.RemoveComponent<ChangesMusicTrack>(entity);
                    this.ChangeMusicTrack(track.samples);
                }
            );
        }

        protected override void OnDestroy()
        {
            Debug.Log($"{nameof(MusicManager)}.{nameof(this.OnDestroy)}");
            using (var block = this._graph.CreateCommandBlock())
            {
                block.ReleaseDSPNode(this._current.node);
                block.ReleaseDSPNode(this._previous.node);
            }
        }

        public void ChangeMusicTrack(AudioSampleSet sample)
        {
            var random = sample.RandomSample();
            if (!random.HasValue)
            {
                return;
            }

            // swap the current and previous
            var current = this._previous;
            this._previous = this._current;
            this._current = current;

            using (var block = this._graph.CreateCommandBlock())
            {
                block.TriggerAudioSample(
                    this._current.node,
                    random.Value.ToReader(true),
                    Categories.Music
                );

                block.SetAttenuation(
                    this._current.connection,
                    1f,
                    AudioSettings.outputSampleRate * 15
                );

                block.SetAttenuation(
                    this._previous.connection,
                    0f,
                    AudioSettings.outputSampleRate * 4
                );
            }
        }

        private AudioClipPlayerSystemState CreateNode(DSPCommandBlock block)
        {
            var node = block.CreateDSPNode<Parameters, Providers, PlayAudioSampleReaderNode>();
            block.AddOutletPort(node, 2);
            var connection = block.Connect(
                node,
                0,
                this._graph.RootDSP,
                0
            );
            return new AudioClipPlayerSystemState(node, connection);
        }
    }
}