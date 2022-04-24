using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboLibrary.Actors;
using GLFrameworkEngine;

namespace TurboLibrary
{
    public class TurboSystem
    {
        /// <summary>
        /// The active instance of the turbo system.
        /// </summary>
        public static TurboSystem Instance;

        /// <summary>
        /// Determines if the game timer is playing or not.
        /// </summary>
        public bool IsPlaying => Timer.IsPlaying;

        /// <summary>
        /// A list of actors in the scene.
        /// </summary>
        public List<ActorBase> Actors = new List<ActorBase>();

        /// <summary>
        /// Fields for handling map data and game logic.
        /// </summary>
        public MapFieldAccessor MapFieldAccessor = new MapFieldAccessor();

        /// <summary>
        /// The camera handler of the scene.
        /// </summary>
        public DemoCameraDirector DemoCamera;

      //  public EffectRenderer EffectDrawer;

        private GameTimer Timer = new GameTimer();

        public TurboSystem() {
            Timer.OnFrameUpdate += OnFrameUpdate;
            Instance = this;

            //Init some default actors
            DemoCamera = (DemoCameraDirector)ActorFactory.GetActorEntry("DemoCamera");
         //   EffectDrawer = (EffectRenderer)ActorFactory.GetActorEntry("EffectDrawer");

            AddActor(DemoCamera);
          //  AddActor(EffectDrawer);
        }

        /// <summary>
        /// Runs the current scene during the calculation loop.
        /// </summary>
        public void Run()   { Timer.Play(); }

        /// <summary>
        /// Pauses all current scene calculations.
        /// </summary>
        public void Pause() { Timer.Pause(); }

        public void AddActor(ActorBase actor) {
            if (Actors.Contains(actor))
                return;

            actor.CreateIdx = Actors.Count;
            actor.Age = 0;
            Actors.Add(actor);
        }

        public void RemoveActor(ActorBase actor) {
            if (!Actors.Contains(actor)) //Actor already removed
                return;

            Actors.Remove(actor);
            actor.CreateIdx = -1;
        }

        public void Begin()
        {
            //Reset the default settings
            for (int i = 0; i < Actors.Count; i++)
                Actors[i].BeginFrame();
        }

        private void OnFrameUpdate(object sender, EventArgs e) {
            Begin();
            CalculateScene();
        }

        /// <summary>
        /// Calculates the current scene and is calculated each frame.
        /// </summary>
        public void CalculateScene()
        {
            //Force update the viewport to update the render cache
            GLContext.ActiveContext.UpdateViewport = true;

            for (int i = 0; i < Actors.Count; i++) {
                if (Actors[i].UpdateCalc) {
                    Actors[i].Calc();

                    Actors[i].Age++;
                    Actors[i].UpdateCalc = false;
                }
            }
        }

        public void DrawEffects(GLContext context) {
           // EffectDrawer.Draw(context);
        }

        public void ResetAnimation()
        {
            Pause();

            for (int i = 0; i < Actors.Count; i++) {
                if (Actors[i] is ActorModelBase)
                    ((ActorModelBase)Actors[i]).ResetAnimation();
            }
        }

        public void Dispose() {
            this.Timer?.Dispose();

            for (int i = 0; i < Actors.Count; i++)
                Actors[i]?.Dispose();
        }
    }
}
