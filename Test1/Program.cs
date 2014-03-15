using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test1
{
    class Program
    {

        public class Continue : Exception
        {
            public Continue() : base("") {
            }
        }

        public object Hook() {
            return null;
        }

        public object HookPrototype(params object[] args) {
            try {
                return Hook();
            } catch (Continue) {
            }
            return null;
        }

        static void Main(string[] args) {
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(".");
            resolver.AddSearchDirectory("../../assemblies");
            resolver.AddSearchDirectory("../../assemblies/Mono");

            AssemblyDefinition targetAssembly = AssemblyDefinition.ReadAssembly("Test2.dll", new ReaderParameters { AssemblyResolver = resolver });
            AssemblyDefinition sourceAssembly = AssemblyDefinition.ReadAssembly("Test1.exe", new ReaderParameters { AssemblyResolver = resolver });

            SharpMagic.DumpMethods(sourceAssembly, false);

            MethodDefinition targetMethod = SharpMagic.InjectBefore(
                targetAssembly, "System.Void Test2.Program::HookMe(System.String)",
                sourceAssembly, "System.Object Test1.Program::HookPrototype(System.Object[])",
                resolver, true
            );
            Console.WriteLine("");

            Console.WriteLine("Writing patch ...");
            try {
                targetAssembly.Write("Test2-patched.dll");
                Console.WriteLine("Done.");
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex);
            }

            /* MethodDefinition compareMethod = Magic.FindMethod(sourceAssembly, "System.Int32 Test1.Program::HookCompare()");
            Console.Write("[Compare] ");
            Magic.DumpMethod(compareMethod);
            Console.WriteLine("");
            Console.Write("[Target] ");
            Magic.DumpMethod(targetMethod);
            Console.WriteLine(""); */

            Console.ReadKey();
        }
    }
}
