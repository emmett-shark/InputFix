using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection.Emit;
namespace InputFix{

    [HarmonyPatch(typeof(GameController))]
    public partial class Patches {
        private static Action<string> LogDebug = Plugin.LogDebug;

        [HarmonyPatch("Update")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            LogDebug("Starting Transpiling");
            CodeMatcher matcher = new CodeMatcher(instructions, ilGenerator);
            LogDebug($"Instruction count: {matcher.Length}");
            LogDebug($"Current Pos: {matcher.Pos}");
            LogDebug($"Finding Instructions");
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Br));
            LogDebug($"Current Pos: {matcher.Pos}");

            //Get flag index
            CodeInstruction ins = matcher.InstructionAt(1);
            int flagIndex = 0;
            if(ins.IsStloc()) flagIndex = ((LocalBuilder)ins.operand).LocalIndex;

            //Get num9 index
            ins = matcher.InstructionAt(3);
            int num9Index = 0;
            if(ins.IsStloc()) num9Index = ((LocalBuilder)ins.operand).LocalIndex;



            /* if(Input.GetKeyDown(__instance.toot_keys[k])){
                flag = true;
            } */
            matcher.MatchForward(false, 
                new CodeMatch(OpCodes.Brfalse),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Stloc_S));
            //build if
            Label old = (Label)matcher.Instruction.operand;
            Label dest = ilGenerator.DefineLabel();
            List<CodeInstruction> codes = matcher.InstructionsWithOffsets(-5, -2); //Get Previous if statement
            List<CodeInstruction> codes1 = new List<CodeInstruction>();
            foreach(CodeInstruction c in codes){
                codes1.Add(c.Clone());
            }
            codes = codes1;
            matcher.Advance(5);
            codes.First().labels.Add(old);

            //if(Input.GetKeyDown(__instance.toot_keys[k]))
            matcher.InsertAndAdvance(codes);
            matcher.InsertAndAdvance(CodeInstruction.Call(typeof(Input), nameof(Input.GetKeyDown), new Type[]{typeof(KeyCode)}));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, dest));

            //flag = true
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, flagIndex));
            matcher.Instruction.labels = new List<Label>(){dest};

            codes.Clear();


            LogDebug($"if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) num9++;");
            matcher.MatchForward(true, 
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Blt));
            LogDebug($"Current Pos: {matcher.Pos}");

            matcher.Advance(1);

            Label label1 = ilGenerator.DefineLabel();
            Label label2 = ilGenerator.DefineLabel();
            //Input.GetMouseButton(0)
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0));
            matcher.InsertAndAdvance(CodeInstruction.Call(typeof(Input), nameof(Input.GetMouseButton), new Type[] { typeof(int) }));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label1));

            //Input.GetMouseButton(1)
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
            matcher.InsertAndAdvance(CodeInstruction.Call(typeof(Input), nameof(Input.GetMouseButton), new Type[] { typeof(int) }));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label2));

            //num9++
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, num9Index));
            matcher.AddLabelsAt(matcher.Pos - 1, new List<Label> { label1 });
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Add));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, num9Index));
            matcher.AddLabels(new List<Label> { label2 });



            //flag
            matcher.SetOperandAndAdvance(flagIndex);
            matcher.RemoveInstruction();
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);



            LogDebug($"Changing GetMouseButton(0) -> GetMouseButtonDown(0)");
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Call, name: nameof(Input.GetMouseButton)),
                new CodeMatch(OpCodes.Brtrue));
            LogDebug($"Current Pos: {matcher.Pos}");
            matcher.Advance(-1);
            matcher.SetInstruction(CodeInstruction.Call(typeof(Input), nameof(Input.GetMouseButtonDown), new Type[] { typeof(int) }));

            LogDebug($"Changing GetMouseButton(1) -> GetMouseButtonDown(1)");
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Call, name: nameof(Input.GetMouseButton)),
                new CodeMatch(OpCodes.Brfalse));
            LogDebug($"Current Pos: {matcher.Pos}");
            matcher.Advance(-1);
            matcher.SetInstruction(CodeInstruction.Call(typeof(Input), nameof(Input.GetMouseButtonDown), new Type[] { typeof(int) }));


            //(flag || num9 == 0)
            matcher.MatchForward(true, 
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Brtrue),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brtrue));
            LogDebug($"Current Pos: {matcher.Pos}");


            dest = (Label)matcher.Instruction.operand;
            Label label = ilGenerator.DefineLabel();
            //matcher.RemoveInstruction();
            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Brtrue, label));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, num9Index));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Bne_Un, dest));
            matcher.AddLabels(new List<Label>(){label});


            //remove !noteplaying
            matcher.MatchForward(true, 
                new CodeMatch(OpCodes.Div),
                new CodeMatch(OpCodes.Pop),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brfalse),
                new CodeMatch(OpCodes.Ldarg_0));
            LogDebug($"Current Pos1: {matcher.Pos}");

            matcher.RemoveInstructionsWithOffsets(0, 2);


            // num9 == 0
            matcher.MatchForward(false, 
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Br),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brtrue),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld));
            LogDebug($"Current Pos: {matcher.Pos}");
            
            matcher.Advance(2);
            matcher.SetOperandAndAdvance(num9Index);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0));
            matcher.SetOpcodeAndAdvance(OpCodes.Bne_Un);

            return matcher.InstructionEnumeration();
        }

       
    }
}