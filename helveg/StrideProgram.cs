using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.GameDefaults.ProceduralModels;
using Stride.GameDefaults.Extensions;
using Stride.Engine.Processors;
using Stride.Physics;
using Stride.Rendering.Compositing;
using Stride.Rendering.Images;

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
                game.AddGraphicsCompositor();
                var rendererCollection = (SceneRendererCollection)game.SceneSystem.GraphicsCompositor.Game;
                var cameraRenderer = (SceneCameraRenderer)rendererCollection.First();
                var forwardRenderer = (ForwardRenderer)cameraRenderer.Child;
                var postEffects = (PostProcessingEffects)forwardRenderer.PostEffects;
                var antialiasing = (FXAAEffect)postEffects.Antialiasing;
                antialiasing.Enabled = true;
                antialiasing.Dither = FXAAEffect.DitherType.Medium;
                antialiasing.Quality = 0;
                game.Window.AllowUserResizing = true;
                var cameraEntity = new Entity("Camera")
                {
                    new CameraComponent
                    {
                        Projection = CameraProjectionMode.Orthographic,
                        Slot = game.SceneSystem.GraphicsCompositor.Cameras[0].ToSlotId()
                    }
                };
                cameraEntity.Transform.Position = new Vector3(0, 0, 10);
                cameraEntity.Transform.Rotation = Quaternion.RotationYawPitchRoll(0, 0, 0);
                game.SceneSystem.SceneInstance.RootScene.Entities.Add(cameraEntity);

                var entity = game.CreatePrimitive(PrimitiveModelType.Teapot, includeCollider: true);

                entity.Transform.Position = new Vector3(0, 0, 0);
                var rigidbody = new RigidbodyComponent();
                rigidbody.ColliderShape = new SphereColliderShape(false, 2);
                entity.Add(rigidbody);
                entity.Scene = rootScene;
                rigidbody.ApplyTorque(Vector3.UnitY * 100);
                rigidbody.Gravity = Vector3.Zero;

            }
        }
    }
}
