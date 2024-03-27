

static class EmitCPP{

    static string EmitArgs(IExpression[] args){
        var cpp = "(";
        for(var i=0;i<args.Length;i++){
            cpp+=EmitExpression(args[i]);
            if(i<args.Length-1){
                cpp+=", ";
            }
        }
        cpp+=")";
        return cpp;
    }

    static string EmitExpression(IExpression expression){
        if(expression is Call call){
            return call.name.value+EmitArgs(call.args);
        }
        else if(expression is Literal literal){
            return literal.value.value;
        }
        else{
            throw new NotImplementedException();
        }
    }

    static string EmitParameters(Variable[] parameters){
        var cpp = "(";
        for(var i=0;i<parameters.Length;i++){
            cpp+=parameters[i].type.value+" "+parameters[i].name.value;
            if(i<parameters.Length-1){
                cpp+=", ";
            }
        }
        cpp+=")";
        return cpp;
    }

    public static string Emit(Body body){
        string cppFunctions = "#include <stdio.h>\n\n";
        string cppMain = "";

        cppMain += "int main(){\n";
        foreach(var s in body.statements){
            if(s is ImportFunction importFunction){
                cppFunctions += importFunction.returnType.value+" "+importFunction.name.value+EmitParameters(importFunction.parameters)+"{\n";
                cppFunctions += importFunction.cppCode.value;
                cppFunctions += "}\n";
            }
            else if(s is Expression expression){
                cppMain += EmitExpression(expression.expression)+";\n";
            }
            else{
                throw new Exception(s.GetType().Name);
            }
        }
        cppMain += "return 0;\n";
        cppMain += "}\n";

        return cppFunctions + cppMain;
    }
}