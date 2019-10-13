﻿using MonoMod.Cil;
using System;
using UnityEngine;
using Mono.Cecil.Cil;

namespace RoR2Cheats
{
    public class Hooks
    {
        
        public static void InitializeHooks()
        {
            ConCommandHooks();

            SeedHooks();

            CameraFOVHooks();

            SetupNoEnemyIL();

            SetupFOVIL();

            IL.RoR2.Networking.GameNetworkManager.FixedUpdateServer += GameNetworkManager_FixedUpdateServer;
            //IL.RoR2.Networking.GameNetworkManager.cctor += GameNetworkManager_cctor;
        }

        private static void GameNetworkManager_cctor(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdstr("sv_time_transmit_interval"),
                x => x.MatchLdcI4(out _),
                x => x.MatchLdcR4(out _)
                );
            c.Next.Next.Next.Operand = Cheats.TickIntervalMulti;

        }

        private static void GameNetworkManager_FixedUpdateServer(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld("RoR2.Networking.GameNetworkManager", "timeTransmitTimer"),
                x => x.MatchLdsfld("RoR2.Networking.GameNetworkManager", "svTimeTransmitInterval")
                );
            c.Index += 4;
            c.Emit(OpCodes.Ldc_R4, Cheats.TickIntervalMulti);
            c.Emit(OpCodes.Mul);

        }

        private static void ConCommandHooks()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                R2API.Utils.CommandHelper.RegisterCommands(self);
                orig(self);
            };
        }

        private static void SeedHooks()
        {
            On.RoR2.PreGameController.Awake += (orig,self)=> 
            {
                orig(self);
                if (Cheats.seed != 0)
                {
                    self.runSeed = Cheats.seed;
                }
            };
        }



        private static void CameraFOVHooks()
        {
            On.RoR2.CameraRigController.Start += (orig, self) =>
            {
                self.baseFov = Cheats.FieldOfVision;
                orig(self);
            };
        }

        private static void SetupNoEnemyIL()
        {
            IL.RoR2.CombatDirector.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(x => x.MatchStfld("RoR2.CombatDirector", "monsterCredit")))
                {
                    c.EmitDelegate<Func<float, float>>((f) =>
                    {
                        return Cheats.noEnemies ? 0f : f;
                    });
                }
                else
                {
                    Debug.LogWarning("RoR2Cheats - Could not create IL hook IL.RoR2.CombatDirector.FixedUpdate");
                }
            };

            IL.RoR2.TeleporterInteraction.OnStateChanged += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(x => x.MatchStfld("RoR2.CombatDirector", "monsterCredit")))
                {
                    c.EmitDelegate<Func<float, float>>((f) =>
                    {
                        return Cheats.noEnemies ? 0f : f;
                    });
                }
                else
                {
                    Debug.LogWarning("RoR2Cheats - Could not create IL hook IL.RoR2.TeleporterInteraction.OnStateChanged");
                }
            };
            IL.RoR2.SceneDirector.Start += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(x => x.MatchStfld("RoR2.SceneDirector", "monsterCredit")))
                {
                    c.EmitDelegate<Func<int, int>>((i) =>
                    {
                        return Cheats.noEnemies ? 0 : i;
                    });
                }
                else
                {
                    Debug.LogWarning("RoR2Cheats - Could not create IL hook IL.RoR2.SceneDirector.Start");
                }
            };
        }

        private static void SetupFOVIL()
        {

            IL.RoR2.CameraRigController.Update += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(
                    x => x.MatchLdcR4(1.3f)
                ))
                {
                    c.Index++;
                    c.EmitDelegate<Func<float, float>>((f) => { return 1; });
                }
                else
                {
                    Debug.LogWarning("RoR2Cheats - Could not create IL hook IL.RoR2.CameraRigController.Update");
                }
            };

            IL.EntityStates.Huntress.BackflipState.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(x => x.MatchLdcR4(60f)))
                {
                    c.Index++;
                    c.EmitDelegate<Func<float, float>>(f => { return Cheats.FieldOfVision - 10f; });
                }
                else
                {
                    Debug.LogWarning("RoR2Cheats - Could not create IL hook IL.EntityStates.Huntress.BackflipState.FixedUpdate");
                }
            };

            IL.EntityStates.Commando.DodgeState.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(x => x.MatchLdcR4(60f)))
                {
                    c.Index++;
                    c.EmitDelegate<Func<float, float>>(f => { return Cheats.FieldOfVision - 10f; });
                }
                else
                {
                    Debug.LogWarning("RoR2Cheats - Could not create IL hook IL.EntityStates.Commando.DodgeState.FixedUpdate");
                }
            };
        }

    }
}
