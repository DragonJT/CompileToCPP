
using System.Diagnostics;

class Program{
    static void Main(){
        var code = File.ReadAllText("main.j");
        var tree = Parser.ParseBody(code);
        var cpp = EmitCPP.Emit(tree);
        File.WriteAllText("cpp\\main.cpp", cpp);
    }
}