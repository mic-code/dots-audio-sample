using Unity.Audio;
using Unity.Entities;

namespace Simulize.Utility
{
    public struct AudioClipPlayerSystemState : ISystemStateComponentData
    {
        public DSPNode node;
        public DSPConnection connection;

        public AudioClipPlayerSystemState(DSPNode node, DSPConnection connection)
        {
            this.node = node;
            this.connection = connection;
        }
    }
}