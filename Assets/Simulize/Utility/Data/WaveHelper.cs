using System;
using System.IO;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Simulize.Utility
{
    public static class WaveHelper
    {
        public static bool TryReadWav(string filename, out AudioSampleData clip)
        {
            //Debug.Log($"Reading wav file {filename}");
            try
            {
                using (var fs = File.Open(filename, FileMode.Open))
                using (var reader = new BinaryReader(fs))
                {
                    var result = TryReadWav(reader, out clip);

                    if (result)
                    {
                        //Debug.Log($"Finished reading wav file {filename}");
                    }
                    else
                    {
                        Debug.LogError("Failed to load wav: " + filename);
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load wav: " + filename);
                Debug.LogException(e);
                clip = default;
                return false;
            }
        }

        public static bool TryReadWav(byte[] data, out AudioSampleData clip)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                return TryReadWav(reader, out clip);
            }
        }

        public static void WriteWav(AudioSampleData data, string path)
        {
            using (var stream = File.Open(path, FileMode.CreateNew))
            using (var writer = new BinaryWriter(stream))
            {
                WriteWav(writer, data);
            }
        }

        public static void WriteWav(BinaryWriter writer, AudioSampleData clip)
        {
            var bytes = clip.ToByteArray();
            const int sampleSize = sizeof(float);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + bytes.Length);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)3);
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * sampleSize);
            writer.Write((short)(clip.channels * sampleSize));
            writer.Write((short)(sampleSize * 8));

            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static bool TryReadWav(BinaryReader reader, out AudioSampleData clip)
        {
            // chunk 0
            if (reader.ReadByte() != 'R' || 
                reader.ReadByte() != 'I' ||
                reader.ReadByte() != 'F' ||
                reader.ReadByte() != 'F')
            {
                Debug.Log("chunkId != RIFF");
                clip = default;
                return false;
            }

            var fileSize = reader.ReadInt32();

            if (reader.ReadByte() != 'W' || 
                reader.ReadByte() != 'A' ||
                reader.ReadByte() != 'V' ||
                reader.ReadByte() != 'E')
            {
                Debug.Log("riffType != WAVE");
                clip = default;
                return false;
            }

            while (reader.ReadByte() != 'f' ||
                   reader.ReadByte() != 'm' ||
                   reader.ReadByte() != 't' ||
                   reader.ReadByte() != ' ')
            {
                // search for 'fmt '
            }

            var fmtSize = reader.ReadInt32(); // bytes for this chunk

            var fmtCode = reader.ReadInt16();
            switch (fmtCode)
            {
                case 0:
                    Debug.LogWarning($"fmtCode = {fmtCode} (UNKNOWN)");
                    break;

                case 1:
                    break;

                default:
                    Debug.LogError($"fmtCode = {fmtCode}");
                    clip = default;
                    return false;
            }

            int channels = reader.ReadInt16();
            var sampleRate = reader.ReadInt32();
            var byteRate = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();

            switch (fmtSize)
            {
                case 16:
                    break;

                case 18:
                    // Read any extra values
                    var extra18 = reader.ReadBytes(reader.ReadInt16());
                    break;

                case 28:
                    var extra28 = reader.ReadBytes(reader.ReadInt16());
                    break;

                default:
                    Debug.LogError($"fmtSize = {fmtSize}");
                    clip = default;
                    return false;
            }

            // chunk 2
            while (reader.ReadByte() != 'd' ||
                   reader.ReadByte() != 'a' ||
                   reader.ReadByte() != 't' ||
                   reader.ReadByte() != 'a')
            {
                // search for 'data'
            }

            var bytes = reader.ReadInt32();

            // DATA!
            var byteArray = reader.ReadBytes(bytes);

            float[] asFloat;
            switch (bitDepth)
            {
                case 64:
                    var asDouble = new double[bytes / 8];
                    Buffer.BlockCopy(
                        byteArray,
                        0,
                        asDouble,
                        0,
                        bytes
                    );
                    asFloat = Array.ConvertAll(asDouble, e => (float) e);
                    break;

                case 32:
                    asFloat = new float[bytes / 4];
                    Buffer.BlockCopy(
                        byteArray,
                        0,
                        asFloat,
                        0,
                        bytes
                    );
                    break;

                case 16:
                    var asInt16 = new short[bytes / 2];
                    Buffer.BlockCopy(
                        byteArray,
                        0,
                        asInt16,
                        0,
                        bytes
                    );
                    asFloat = Array.ConvertAll(asInt16, e => e / (float) short.MaxValue);
                    break;

                default:
                    Debug.LogError($"Unsupported bit depth: {bitDepth}");
                    clip = default;
                    return false;
            }

            var data = new NativeArray<float>(asFloat, Allocator.Temp);
            clip = new AudioSampleData(
                channels,
                bitDepth,
                sampleRate,
                data
            );
            return true;
        }
    }
}