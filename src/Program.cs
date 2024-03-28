
using System.Diagnostics;

class Program{
    static void Main(){
        var code = File.ReadAllText("main.j");
        var tree = Parser.ParseRoot(code);
        var cpp = EmitCPP.EmitRoot(tree);
        File.WriteAllText("cpp\\main.cpp", cpp);
    }
}