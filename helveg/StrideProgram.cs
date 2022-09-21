using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.GameDefaults.ProceduralModels;
using Stride.GameDefaults.Extensions;

namespace Helveg
{
    public class StrideProgram
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello from StrideProgram!");

            using var game = new Game();

            game.Run(start: Start);

            void Start(Scene rootScene)
            {
                game.SetupBase3DScene();

                var entity = game.CreatePrimitive(PrimitiveModelType.Capsule);

                entity.Transform.Position = new Vector3(0, 8, 0);

                entity.Scene = rootScene;
            }
        }
    }
}
