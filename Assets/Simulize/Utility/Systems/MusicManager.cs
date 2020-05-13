using System;
using Simulize.Utility.AudioKernels;
using Unity.Audio;
using Unity.Entities;
using UnityEngine;
using static Simulize.Utility.AudioKernels.PlayAudioSampleReaderNode;

namespace Simulize.Utility
{
    public class MusicManager : IDisposable
    {
        private DSPGraph _graph;
        private AudioClipPlayerSystemState _current;
        private AudioClipPlayerSystemState _previous;

        public MusicManager(DSPGraph graph)
        {
            this._graph = graph;

            using (var block = this._graph.CreateCommandBlock())
            {
                this._current = this.CreateNode(block);
                this._previous = this.CreateNode(block);
            }
        }

        public void OnUpdate(EntityQueryBuilder entities, EntityCommandBuffer commands)
        {
            entities.ForEach(
                (Entity entity, ChangesMusicTrack track) =>
                {
                    commands.RemoveComponent<ChangesMusicTrack>(entity);
                    this.ChangeMusicTrack(track.samples);
                }
            );
        }

        public void Dispose()
        {
            using (var block = this._graph.CreateCommandBlock())
            {
                block.ReleaseDSPNode(this._current.node);
                block.ReleaseDSPNode(this._previous.node);
            }

            //this._current.node.Dispose(this._graph);
            //this._current = default;
            //this._previous.node.Dispose(this._graph);
            //this._previous = default;
        }

        private AudioClipPlayerSystemState CreateNode(DSPCommandBlock block)
        {
            var node = block.CreateDSPNode<Parameters, Providers, PlayAudioSampleReaderNode>();
            block.AddOutletPort(node, 2, SoundFormat.Stereo);
            var connection = block.Connect(
                node,
                0,
                this._graph.RootDSP,
                0
            );
            return new AudioClipPlayerSystemState(node, connection);
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
    }
}