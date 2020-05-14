using System;
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using UnityEngine;

namespace Simulize.Utility
{
    [UpdateAfter(typeof(AudioSystem))]
    public class ExampleOneShotSystem : ComponentSystem
    {
        private Entity _prefab;
        private AudioSampleSet _samples;

        protected override void OnCreate()
        {
            this._samples = new AudioSampleSet(GetAudioSamples());

            this._prefab = this.EntityManager.CreateEntity(typeof(PlaysOneShotAudioSample), typeof(Prefab));
            this.EntityManager.SetName(this._prefab, "One shot audio");
            this.EntityManager.SetSharedComponentData(this._prefab, new PlaysOneShotAudioSample(this._samples));
        }

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.PostUpdateCommands.Instantiate(this._prefab);
            }
        }

        protected override void OnDestroy()
        {
            Debug.Log($"{nameof(ExampleOneShotSystem)}.{nameof(this.OnDestroy)}");
            this._samples?.Dispose();
            this._samples = null;
        }

        private static IEnumerable<AudioSampleData> GetAudioSamples()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Application.dataPath, "Samples")))
            {
                var extension = Path.GetExtension(file);
                if (!extension.Equals(".wav", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (!WaveHelper.TryReadWav(file, out var sample))
                {
                    continue;
                }

                Debug.Log($"Found and initialized '{file}'");
                var prepared = sample.Initialize(AudioSettings.outputSampleRate);
                sample.Dispose();
                yield return prepared;
            }
        }
    }
}