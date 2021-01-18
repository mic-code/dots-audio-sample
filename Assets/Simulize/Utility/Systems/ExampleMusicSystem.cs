using System.IO;
using Unity.Entities;
using UnityEngine;

namespace Simulize.Utility
{
    [UpdateAfter(typeof(MusicManager))]
    public class ExampleMusicSystem : ComponentSystem
    {
        private AudioSampleSet _music;

        protected override void OnCreate()
        {
            if (!WaveHelper.TryReadWav(
                Path.Combine(Application.dataPath, "Music", "short.wav"),
                //Path.Combine(Application.dataPath, "Music", "bensound-summer.wav"),
                out var music
            ))
            {
                return;
            }

            // try read uses temporary allocation so we resample with more persistant allocation
            this._music = new AudioSampleSet(music.Initialize(AudioSettings.outputSampleRate));
            music.Dispose();

            this.EntityManager.SetSharedComponentData(
                this.EntityManager.CreateEntity(typeof(ChangesMusicTrack)),
                new ChangesMusicTrack(this._music)
            );
        }

        protected override void OnDestroy()
        {
            Debug.Log($"{nameof(ExampleMusicSystem)}.{nameof(this.OnDestroy)}");
            this._music?.Dispose();
            this._music = null;
        }


        protected override void OnUpdate()
        {
        }
    }
}