/*
    This file is part of SharpMagic.

    SharpMagic is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    SharpMagic is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with SharpMagic. If not, see <http://www.gnu.org/licenses/>.
*/
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;

public class SharpMagic
{

    /// <summary>
    /// Dumps a list of all methods in an assembly to the system console.
    /// </summary>
    /// <param name="assembly">Assembly to inspect</param>
    /// <param name="detailed">Whether to print all the details (true) or just a list (false)</param>
    public static void DumpMethods(AssemblyDefinition assembly, bool detailed) {
        foreach (TypeDefinition typeDefinition in assembly.MainModule.Types) {
            foreach (MethodDefinition method in typeDefinition.Methods) {
                if (!detailed) {
                    Console.WriteLine(method.FullName);
                } else {
                    DumpMethod(method);
                }
            }
        }
    }

    /// <summary>
    /// Finds a method inside of an assembly by its full name.
    /// </summary>
    /// <param name="assembly">Assembly to inspect</param>
    /// <param name="fullName">Full name of the method</param>
    /// <returns>Method definition of the method or null if none is found</returns>
    public static MethodDefinition FindMethod(AssemblyDefinition assembly, string fullName) {
        foreach (TypeDefinition typeDefinition in assembly.MainModule.Types) {
            foreach (MethodDefinition method in typeDefinition.Methods) {
                if (method.FullName == fullName)
                    return method;
            }
        }
        return null;
    }

    /// <summary>
    /// Dumps a method definition to the system console.
    /// </summary>
    /// <param name="method">Method to dump</param>
    public static void DumpMethod(MethodDefinition method) {
        Console.WriteLine("[" + method.FullName + "]");
        Console.WriteLine("Variables:");
        foreach (VariableDefinition def in method.Body.Variables) {
            Console.WriteLine("  "+def.VariableType + " " + def.Name + " at " + def.Index + " (pinned="+def.IsPinned+")");
        }
        Console.WriteLine("Exception handlers:");
        foreach (var def in method.Body.ExceptionHandlers) {
            Console.WriteLine("  "+def.CatchType+" from "+def.HandlerStart+" to "+def.HandlerEnd);
            Console.WriteLine("     try from " + def.TryStart + " to " + def.TryEnd);
        }
        Console.WriteLine("Code:");
        foreach (Instruction inst in method.Body.Instructions) {
            Console.WriteLine("  "+inst.ToString());
        }
        Console.WriteLine("Code size       : " + method.Body.CodeSize);
        Console.WriteLine("Max stack size  : " + method.Body.MaxStackSize);
        Console.WriteLine("Local var token : " + method.Body.LocalVarToken);
        Console.WriteLine("Attributes      : " + method.Attributes);
    }

    /// <summary>
    /// Dumps an exception handler to the system console.
    /// </summary>
    /// <param name="handler">Handler to dump</param>
    public static void DumpExceptionHandler(ExceptionHandler handler) {
        Console.WriteLine("["+handler.HandlerType+" "+handler.CatchType.ToString()+"]");
        Console.WriteLine("  try from " + handler.TryStart + " to " + handler.TryEnd);
        Console.WriteLine("  catch from " + handler.HandlerStart + " to " + handler.HandlerEnd);
    }

    /// <summary>
    /// Gets the variable index for an operation that pops a value from the evaluation stack
    /// and stores it in a local variable.
    /// </summary>
    /// <param name="inst">Instruction to inspect</param>
    /// <returns>Variable index or -1 of not applicable</returns>
    public static int GetStlocIndex(Instruction inst) {
        switch (inst.OpCode.Code) {
            case Code.Stloc:
                return 0xFFFF & (short)inst.Operand;
            case Code.Stloc_S:
                return 0xFF & (byte)inst.Operand;
            case Code.Stloc_0:
                return 0;
            case Code.Stloc_1:
                return 1;
            case Code.Stloc_2:
                return 2;
            case Code.Stloc_3:
                return 3;
            default:
                return -1;
        }
    }

    /// <summary>
    /// Gets the variable index for an operation that pushes a local variable to the
    /// evaluation stack.
    /// </summary>
    /// <param name="inst">Instruction to inspect</param>
    /// <returns>Variable index or -1 not applicable</returns>
    public static int GetLdlocIndex(Instruction inst) {
        switch (inst.OpCode.Code) {
            case Code.Ldloc:
                return 0xFFFF & (short)inst.Operand;
            case Code.Ldloc_S:
                return 0xFF & (byte)inst.Operand;
            case Code.Ldloc_0:
                return 0;
            case Code.Ldloc_1:
                return 1;
            case Code.Ldloc_2:
                return 2;
            case Code.Ldloc_3:
                return 3;
            default:
                return -1;
        }
    }

    /// <summary>
    /// Sets the variable index for an operation that pops a value from the evaluation stack
    /// and stores it in a local variable.
    /// </summary>
    /// <param name="inst">Instruction to modify</param>
    /// <param name="index">Index to set</param>
    public static void SetStlocIndex(Instruction inst, int index) {
        switch (index) {
            case 0:
                inst.OpCode = OpCodes.Stloc_0;
                inst.Operand = null;
                break;
            case 1:
                inst.OpCode = OpCodes.Stloc_1;
                inst.Operand = null;
                break;
            case 2:
                inst.OpCode = OpCodes.Stloc_2;
                inst.Operand = null;
                break;
            case 3:
                inst.OpCode = OpCodes.Stloc_3;
                inst.Operand = null;
                break;
            default:
                if (index <= 0xFF) {
                    inst.OpCode = OpCodes.Stloc_S;
                    inst.Operand = (byte)index;
                } else {
                    inst.OpCode = OpCodes.Stloc;
                    inst.Operand = (short)index;
                }
                break;
        }
    }

    /// <summary>
    /// Sets the variable index for an operation that pushes a local variable to the
    /// evaluation stack. 
    /// </summary>
    /// <param name="inst">Instruction to modify</param>
    /// <param name="index">Variable index to set</param>
    public static void SetLdlocIndex(Instruction inst, int index) {
        switch (index) {
            case 0:
                inst.OpCode = OpCodes.Ldloc_0;
                inst.Operand = null;
                break;
            case 1:
                inst.OpCode = OpCodes.Ldloc_1;
                inst.Operand = null;
                break;
            case 2:
                inst.OpCode = OpCodes.Ldloc_2;
                inst.Operand = null;
                break;
            case 3:
                inst.OpCode = OpCodes.Ldloc_3;
                inst.Operand = null;
                break;
            default:
                if (index <= 0xFF) {
                    inst.OpCode = OpCodes.Ldloc_S;
                    inst.Operand = (byte)index;
                } else {
                    inst.OpCode = OpCodes.Ldloc;
                    inst.Operand = (short)index;
                }
                break;
        }
    }

    /// <summary>
    /// Gets the number of stack pops that an instruction performs.
    /// </summary>
    /// <param name="inst">Instruction</param>
    /// <returns>Number of stack pops or -1 if variable. int.MaxValue if everything is popped.</returns>
    public static int GetPops(Instruction inst) {
        // http://edc.tversu.ru/elib/inf/0028/ch08lev1sec3.html
        switch (inst.OpCode.StackBehaviourPop) {
            case StackBehaviour.Pop1:
                return 1;
            case StackBehaviour.Pop1_pop1:
                return 2;
            case StackBehaviour.Popi:
                return 1;
            case StackBehaviour.Popi_pop1:
                return 2;
            case StackBehaviour.Popi_popi:
                return 2;
            case StackBehaviour.Popi_popi8:
                return 2;
            case StackBehaviour.Popi_popi_popi:
                return 3;
            case StackBehaviour.Popi_popr4:
                return 2;
            case StackBehaviour.Popi_popr8:
                return 2;
            case StackBehaviour.Popref:
                return 1;
            case StackBehaviour.Popref_pop1:
                return 2;
            case StackBehaviour.Popref_popi:
                return 2;
            case StackBehaviour.Popref_popi_popi:
                return 3;
            case StackBehaviour.Popref_popi_popi8:
                return 3;
            case StackBehaviour.Popref_popi_popr4:
                return 3;
            case StackBehaviour.Popref_popi_popr8:
                return 3;
            case StackBehaviour.Popref_popi_popref:
                return 3;
            case StackBehaviour.PopAll:
                return int.MaxValue;
            case StackBehaviour.Varpop:
                return -1;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Gets the number of stack pushes that an instruction performs.
    /// </summary>
    /// <param name="inst">Instruction</param>
    /// <returns>Number of stack pushes or -1 if variable</returns>
    public static int GetPushes(Instruction inst) {
        // http://edc.tversu.ru/elib/inf/0028/ch08lev1sec3.html
        switch (inst.OpCode.StackBehaviourPush) {
            case StackBehaviour.Push1:
                return 1;
            case StackBehaviour.Push1_push1:
                return 2;
            case StackBehaviour.Pushi:
                return 1;
            case StackBehaviour.Pushi8:
                return 1;
            case StackBehaviour.Pushr4:
                return 1;
            case StackBehaviour.Pushr8:
                return 1;
            case StackBehaviour.Pushref:
                return 1;
            case StackBehaviour.Varpush:
                return -1;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Transfuses an instruction into a target method.
    /// </summary>
    /// <param name="inst">Instruction to transfuse</param>
    /// <param name="offset">Start offset</param>
    /// <param name="targetAssembly">Target assembly</param>
    /// <param name="targetMethod">Target method</param>
    /// <param name="importToAssembly">Assembly to import to</param>
    /// <param name="verbose">Whether to print verbose messages to the console</param>
    /// <returns></returns>
    public static Instruction TransfuseInstruction(Instruction inst, int offset, AssemblyDefinition targetAssembly, MethodDefinition targetMethod, AssemblyDefinition importToAssembly, bool verbose) {
        Instruction newInst = null;
        ILProcessor proc = targetMethod.Body.GetILProcessor();
        if (inst.Operand == null) {
            newInst = proc.Create(inst.OpCode);
        } else if (inst.Operand is byte) {
            newInst = proc.Create(inst.OpCode, (byte)inst.Operand);
        } else if (inst.Operand is sbyte) {
            newInst = proc.Create(inst.OpCode, (sbyte)inst.Operand);
        } else if (inst.Operand is short) {
            newInst = proc.Create(inst.OpCode, (short)inst.Operand);
        } else if (inst.Operand is int) {
            newInst = proc.Create(inst.OpCode, (int)inst.Operand);
        } else if (inst.Operand is long) {
            newInst = proc.Create(inst.OpCode, (long)inst.Operand);
        } else if (inst.Operand is float) {
            newInst = proc.Create(inst.OpCode, (float)inst.Operand);
        } else if (inst.Operand is double) {
            newInst = proc.Create(inst.OpCode, (double)inst.Operand);
        } else if (inst.Operand is string) {
            newInst = proc.Create(inst.OpCode, (string)inst.Operand);
        } else if (inst.Operand is Instruction) {
            newInst = proc.Create(inst.OpCode, TransfuseInstruction(inst.Operand as Instruction, offset, targetAssembly, targetMethod, importToAssembly, verbose));
        } else if (inst.Operand is Instruction[]) {
            Instruction[] insts = inst.Operand as Instruction[];
            for (var i = 0; i < insts.Length; i++) {
                insts[i] = TransfuseInstruction(insts[i], offset, targetAssembly, targetMethod, importToAssembly, verbose);
            }
            newInst = proc.Create(inst.OpCode, insts);
        } else if (inst.Operand is VariableReference) {
            VariableDefinition def = (inst.Operand as VariableReference).Resolve();
            newInst = proc.Create(inst.OpCode, new VariableDefinition(def.Name, importToAssembly.MainModule.Import(def.VariableType)));
        } else if (inst.Operand is TypeReference) {
            TypeDefinition def = (inst.Operand as TypeReference).Resolve();
            if (def != null) {
                if (verbose)
                    Console.WriteLine("< import " + def);
                newInst = proc.Create(inst.OpCode, importToAssembly.MainModule.Import(def));
            } else {
                throw new Exception("Unresolved TypeReference: " + inst.Operand);
            }
        } else if (inst.Operand is FieldReference) {
            FieldDefinition def = (inst.Operand as FieldReference).Resolve();
            if (def != null) {
                if (verbose)
                    Console.WriteLine("< import " + def);
                newInst = proc.Create(inst.OpCode, importToAssembly.MainModule.Import(def));
            } else {
                throw new Exception("Unresolved FieldReference: " + inst.Operand);
            }
        } else if (inst.Operand is MethodReference) {
            MethodDefinition def = (inst.Operand as MethodReference).Resolve();
            if (def != null) {
                if (verbose)
                    Console.WriteLine("< import " + def);
                newInst = proc.Create(inst.OpCode, importToAssembly.MainModule.Import(def));
            } else {
                throw new Exception("Unresolved MethodReference: " + inst.Operand);
            }
        } else {
            throw new Exception("Unknown Operand type: " + inst.Operand.GetType());
        }
        newInst.Offset = offset + inst.Offset;
        return newInst;
    }

    /// <summary>
    /// Patches a list of exception handlers with a substituted instruction.
    /// </summary>
    /// <param name="handlers">List of handlers to patch</param>
    /// <param name="inst">Original instruction</param>
    /// <param name="newInst">New instruction</param>
    /// <param name="verbose">Whether to print verbose messages to the console or not</param>
    /// <returns>Number of patched instructions</returns>
    public static int PatchExceptionHandlers(List<ExceptionHandler> handlers, Instruction inst, Instruction newInst, bool verbose) {
        int n = 0;
        foreach (ExceptionHandler handler in handlers) {
            if (handler.TryStart == inst) {
                handler.TryStart = newInst;
                ++n;
            }
            if (handler.TryEnd == inst) {
                handler.TryEnd = newInst;
                ++n;
            }
            if (handler.HandlerStart == inst) {
                handler.HandlerStart = newInst;
                ++n;
            }
            if (handler.HandlerEnd == inst) {
                handler.HandlerEnd = newInst;
                ++n;
            }
        }
        if (verbose && n > 0)
            Console.WriteLine(" patched " + n + " exception handlers");
        return n;
    }

    /// <summary>
    /// Tests if an instruction is referenced from at least one other one.
    /// </summary>
    /// <param name="instructions">List of instructions to search</param>
    /// <param name="handlers">Exception handlers to search</param>
    /// <param name="inst">Instruction to look for</param>
    /// <returns>true if it is referenced, otherwise false</returns>
    public static bool IsReferenced(List<Instruction> instructions, List<ExceptionHandler> handlers, Instruction inst) {
        foreach (Instruction iinst in instructions) {
            if (iinst.Operand == inst)
                return true;
        }
        foreach (ExceptionHandler handler in handlers) {
            if (handler.TryStart == inst)
                return true;
            if (handler.TryEnd == inst)
                return true;
            if (handler.HandlerStart == inst)
                return true;
            if (handler.HandlerEnd == inst)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Compresses a list of instructions.
    /// </summary>
    /// <param name="instructions">List of instructions to compress</param>
    /// <param name="handlers">Exception handlers for reference checking</param>
    /// <returns>Compressed size</returns>
    public static int CompressInstructions(List<Instruction> instructions, List<ExceptionHandler> handlers) {
        int offset = 0;
        Instruction prev = null;
        for (int i=0; i<instructions.Count; ) {
            Instruction inst = instructions[i];
            if (inst.OpCode == OpCodes.Nop && !IsReferenced(instructions, handlers, inst)) {
                // Remove unnecessary nops
                instructions.RemoveAt(i);
                continue;
            }
            inst.Offset = offset;
            offset += inst.GetSize();
            if (prev != null)
                prev.Next = inst;
            else
                inst.Next = null;
            prev = inst;
            ++i;
        }
        return offset;
    }

    /// <summary>
    /// Merges the body of the second method with that of the first by inserting the second method
    /// at the top of the first. If you need to access any method parameters, make sure to use the
    /// correct signature as this is not checked.
    /// </summary>
    /// <param name="topAssembly">Top method assembly</param>
    /// <param name="topMethodName">Top method full name</param>
    /// <param name="bottomAssembly">Bottom method assembly</param>
    /// <param name="bottomMethodName">Bottom method full name</param>
    /// <param name="finalAssembly">Final assembly to import to</param>
    /// <param name="finalMethod">Final method to modify</param>
    /// <param name="resolver">Assembly resolved to use</param>
    /// <param name="verbose">Whether to print verbose messages to the system console</param>
    /// <returns>Patched target method defintion</returns>
    public static MethodDefinition MergeMethods(AssemblyDefinition topAssembly, string topMethodName, AssemblyDefinition bottomAssembly, string bottomMethodName, AssemblyDefinition finalAssembly, MethodDefinition finalMethod, IAssemblyResolver resolver, bool verbose) {
        MethodDefinition topMethod = FindMethod(topAssembly, topMethodName);
        if (topMethod == null)
            throw new Exception("No such method: " + topMethodName + " in " + topAssembly);
        MethodDefinition bottomMethod = FindMethod(bottomAssembly, bottomMethodName);
        if (bottomMethod == null)
            throw new Exception("No such method: " + bottomMethodName + " in " + bottomAssembly);

        if (verbose) {
            Console.WriteLine("Merging ...");
            Console.WriteLine(" top    : " + topMethod.FullName    + "\n          " + topAssembly.FullName);
            Console.WriteLine(" bottom : " + bottomMethod.FullName + "\n          " + bottomAssembly.Name);
            Console.WriteLine(" final  : " + finalMethod.FullName  + "\n          " + finalAssembly.Name);
            Console.WriteLine("");
        }

        // Collect all exception handlers on our way
        List<ExceptionHandler> exceptionHandlers = new List<ExceptionHandler>();

        if (verbose)
            Console.WriteLine("Reading top method...");
        int varOffset = 0; // Remember the number of top vars
        List<Instruction> topInstructions = new List<Instruction>();
        foreach (Instruction inst in topMethod.Body.Instructions) {
            topInstructions.Add(inst);
            if (verbose)
                Console.WriteLine("+ " + inst.ToString());
            int index = GetStlocIndex(inst); // Assignment to var at index
            if (index >= varOffset) {
                varOffset = index + 1;
                if (verbose)
                    Console.WriteLine("# var " + varOffset);
            }
        }
        Instruction lastTopInst = topInstructions[topInstructions.Count - 1];
        // If the top methods return type is different than that of the bottom, trim the RET
        if (topMethod.ReturnType.ToString() != bottomMethod.ReturnType.ToString()) {
            if (verbose)
                Console.WriteLine("Trimming ...");
            if (lastTopInst.OpCode == OpCodes.Ret) {
                if (verbose)
                    Console.WriteLine("- " + lastTopInst.ToString());
                // We cannot just delete it as a jump could reference it
                lastTopInst.OpCode = OpCodes.Nop;
                lastTopInst.Operand = null;
                if (verbose)
                    Console.WriteLine("+ " + lastTopInst.ToString());
                if (topMethod.ReturnType.ToString() != "System.Void") {
                    // If it is not returning void, also trim the previous LD
                    lastTopInst = topInstructions[topInstructions.Count - 2];
                    if (GetPushes(lastTopInst) == 1) {
                        if (verbose)
                            Console.WriteLine("- " + lastTopInst.ToString());
                        lastTopInst.OpCode = OpCodes.Nop;
                        lastTopInst.Operand = null;
                        if (verbose)
                            Console.WriteLine("+ " + lastTopInst.ToString());
                    }
                    lastTopInst = topInstructions[topInstructions.Count - 1];
                }
            }
        } else {
            if (verbose)
                Console.WriteLine("Not trimming: " + topMethod.ReturnType);
        }
        foreach (ExceptionHandler handler in topMethod.Body.ExceptionHandlers) {
            Console.WriteLine("< import " + handler.CatchType);
            handler.CatchType = finalAssembly.MainModule.Import(handler.CatchType.Resolve());
            exceptionHandlers.Add(handler);
            if (verbose)
                DumpExceptionHandler(handler);
        }
        if (verbose)
            Console.WriteLine("");

        List<Instruction> bottomInstructions = new List<Instruction>();
        // If the top instructions already returned something, skip bottom
        if (lastTopInst.OpCode != OpCodes.Ret) {
            if (verbose)
                Console.WriteLine("Reading bottom method ...");
            foreach (Instruction inst in bottomMethod.Body.Instructions) {
                bottomInstructions.Add(inst);
                if (verbose)
                    if (verbose)
                        Console.WriteLine("+ " + inst.ToString());
                // Move bottom vars up by the var offset we calculated in top
                int index = GetStlocIndex(inst);
                if (index >= 0) {
                    SetStlocIndex(inst, index + varOffset);
                    if (verbose)
                        Console.WriteLine("# var " + index + " -> " + (index + varOffset));
                } else {
                    index = GetLdlocIndex(inst);
                    if (index >= 0) {
                        SetLdlocIndex(inst, index + varOffset);
                        if (verbose)
                            Console.WriteLine("# var " + index + " -> " + (index + varOffset));
                    }
                }
            }
            // Trim bottom RET if final return type is void and bottom returns something else
            if (finalMethod.ReturnType.ToString() == "System.Void" && bottomMethod.ReturnType.ToString() != finalMethod.ReturnType.ToString()) {
                Instruction lastBottomInst = bottomInstructions[bottomInstructions.Count - 1];
                if (verbose)
                    if (lastBottomInst.OpCode == OpCodes.Ret) {
                        // Keep the last RET as it is required one way or another
                        lastBottomInst = bottomInstructions[bottomInstructions.Count - 2];
                        // But trim the previous LD as final returns void
                        if (GetPushes(lastBottomInst) == 1) {
                            if (verbose)
                                Console.WriteLine("- " + lastBottomInst.ToString());
                            lastBottomInst.OpCode = OpCodes.Nop;
                            lastBottomInst.Operand = null;
                            if (verbose)
                                 Console.WriteLine("+ " + lastBottomInst.ToString());
                            lastBottomInst = bottomInstructions[bottomInstructions.Count - 1];
                        }
                    }
            } else {
                if (verbose)
                    Console.WriteLine("Not trimming: " + finalMethod.ReturnType);
            }
            foreach (ExceptionHandler handler in bottomMethod.Body.ExceptionHandlers) {
                Console.WriteLine("< import " + handler.CatchType);
                handler.CatchType = finalAssembly.MainModule.Import(handler.CatchType.Resolve());
                exceptionHandlers.Add(handler);
                if (verbose)
                    DumpExceptionHandler(handler);
            }
            if (verbose)
                Console.WriteLine("");
        } else {
            if (verbose)
                Console.WriteLine("Skipping bottom method (already returned)");
        }

        if (verbose)
            Console.WriteLine("Baking local variables ...");
        List<VariableDefinition> finalVariables = new List<VariableDefinition>();
        int variableIndex = 0;
        if (verbose)
            Console.WriteLine("top:");
        foreach (VariableDefinition topVar in topMethod.Body.Variables) {
            VariableDefinition targetVar = new VariableDefinition(topVar.Name, finalAssembly.MainModule.Import(topVar.VariableType.Resolve()));
            finalVariables.Add(targetVar);
            if (verbose)
                Console.WriteLine("+ " + targetVar.VariableType + " " + targetVar.Name);
            variableIndex++;
        }
        if (lastTopInst.OpCode != OpCodes.Ret) {
            if (verbose)
                Console.WriteLine("bottom:");
            foreach (VariableDefinition bottomVar in bottomMethod.Body.Variables) {
                VariableDefinition newTargetVar = new VariableDefinition(bottomVar.Name, finalAssembly.MainModule.Import(bottomVar.VariableType.Resolve()));
                finalVariables.Add(newTargetVar);
                if (verbose)
                    Console.WriteLine("+ " + newTargetVar.VariableType + " " + newTargetVar.Name);

            }
        }
        if (verbose)
            Console.WriteLine("");

        if (verbose)
            Console.WriteLine("Merging method bodies ...");
        List<Instruction> finalInstructions = new List<Instruction>();
        int offset = 0;
        foreach (Instruction inst in topInstructions) {
            Instruction newInst = TransfuseInstruction(inst, 0, bottomAssembly, bottomMethod, finalAssembly, verbose);
            finalInstructions.Add(newInst);
            offset = newInst.Offset;
            if (verbose)
                Console.WriteLine("T "+newInst.ToString());
            PatchExceptionHandlers(exceptionHandlers, inst, newInst, verbose);
        }
        foreach (Instruction inst in bottomInstructions) {
            Instruction newInst = TransfuseInstruction(inst, offset, topAssembly, topMethod, finalAssembly, verbose);
            finalInstructions.Add(newInst);
            if (verbose)
                Console.WriteLine("B "+newInst.ToString());
            PatchExceptionHandlers(exceptionHandlers, inst, newInst, verbose);
        }
        if (verbose)
            if (verbose) {
                foreach (ExceptionHandler handler in exceptionHandlers) {
                    DumpExceptionHandler(handler);
                }
                Console.WriteLine("");
            }

        if (verbose)
            Console.WriteLine("Baking instructions ...");
        foreach (Instruction inst in finalInstructions) {
            if (inst.Operand is Instruction) {
                Instruction operand = inst.Operand as Instruction;
                if (verbose)
                    Console.WriteLine("<>" + inst);
                foreach (Instruction iinst in finalInstructions) {
                    if (iinst.Offset == operand.Offset) {
                        inst.Operand = iinst;
                        PatchExceptionHandlers(exceptionHandlers, inst.Operand as Instruction, iinst, verbose);
                    }
                }
            } else {
                if (verbose)
                    Console.WriteLine("= " + inst.ToString());
            }
        }
        if (verbose) {
            foreach (ExceptionHandler handler in exceptionHandlers) {
                DumpExceptionHandler(handler);
            }
            Console.WriteLine("");
        }

        if (verbose)
            Console.WriteLine("Compressing final code ...");
        CompressInstructions(finalInstructions, exceptionHandlers);
        if (verbose) {
            foreach (Instruction inst in finalInstructions) {
                Console.WriteLine(": " + inst.ToString());
            }
            Console.WriteLine("");
        }

        if (verbose)
            Console.WriteLine("Injecting to target ...");
        // Instructions
        finalMethod.Body.Instructions.Clear();
        foreach (Instruction inst in finalInstructions) {
            finalMethod.Body.Instructions.Add(inst);
        }
        // Variables
        finalMethod.Body.Variables.Clear();
        foreach (VariableDefinition finalVariable in finalVariables) {
            finalMethod.Body.Variables.Add(finalVariable);
        }
        finalMethod.Body.InitLocals = finalMethod.Body.Variables.Count > 0;
        // Exception handlers
        finalMethod.Body.ExceptionHandlers.Clear();
        foreach (ExceptionHandler handler in exceptionHandlers) {
            finalMethod.Body.ExceptionHandlers.Add(handler);
        }
        if (verbose)
            Console.WriteLine("Done.");
        return finalMethod;
    }

    /// <summary>
    /// Injects the source method's body to the start of the target method.
    /// </summary>
    /// <param name="targetAssembly">Target assembly</param>
    /// <param name="targetMethodName">Full target method name</param>
    /// <param name="sourceAssembly">Source assembly</param>
    /// <param name="sourceMethodName">Full source method name</param>
    /// <param name="resolver">Assembly resolved to use</param>
    /// <param name="verbose">Whether to print verbose messages to the system console</param>
    /// <returns>Patched target method defintion</returns>
    public static MethodDefinition InjectBefore(AssemblyDefinition targetAssembly, string targetMethodName, AssemblyDefinition sourceAssembly, string sourceMethodName, IAssemblyResolver resolver, bool verbose) {
        MethodDefinition finalMethod = FindMethod(targetAssembly, targetMethodName);
        if (finalMethod == null)
            throw new Exception("No such method: " + targetMethodName + " in " + targetAssembly);
        return MergeMethods(sourceAssembly, sourceMethodName, targetAssembly, targetMethodName, targetAssembly, finalMethod, resolver, verbose);
    }

    /// <summary>
    /// Injects the source method's body to the end of the target method.
    /// </summary>
    /// <param name="targetAssembly">Target assembly</param>
    /// <param name="targetMethodName">Full target method name</param>
    /// <param name="sourceAssembly">Source assembly</param>
    /// <param name="sourceMethodName">Full source method name</param>
    /// <param name="resolver">Assembly resolved to use</param>
    /// <param name="verbose">Whether to print verbose messages to the system console</param>
    /// <returns>Patched target method defintion</returns>
    public static MethodDefinition InjectAfter(AssemblyDefinition targetAssembly, string targetMethodName, AssemblyDefinition sourceAssembly, string sourceMethodName, IAssemblyResolver resolver, bool verbose) {
        MethodDefinition finalMethod = FindMethod(targetAssembly, targetMethodName);
        if (finalMethod == null)
            throw new Exception("No such method: " + targetMethodName + " in " + targetAssembly);
        return MergeMethods(targetAssembly, targetMethodName, sourceAssembly, sourceMethodName, targetAssembly, finalMethod, resolver, verbose);
    }
}
