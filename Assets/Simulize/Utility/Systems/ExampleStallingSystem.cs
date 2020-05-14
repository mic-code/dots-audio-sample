using System.Threading;
using Unity.Entities;
using UnityEngine;

namespace Simulize.Utility
{
    [AlwaysUpdateSystem]
    public class ExampleStallingSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            if (Input.GetKey(KeyCode.BackQuote))
            {
                Thread.Sleep(1);
            }

            if (Input.GetKey(KeyCode.Alpha1))
            {
                Thread.Sleep(10);
            }

            if (Input.GetKey(KeyCode.Alpha2))
            {
                Thread.Sleep(100);
            }

            if (Input.GetKey(KeyCode.Alpha3))
            {
                Thread.Sleep(200);
            }

            if (Input.GetKey(KeyCode.Alpha4))
            {
                Thread.Sleep(400);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                GC.Collect();
            }
        }
    }
}