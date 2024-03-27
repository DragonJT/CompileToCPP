

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
        else if(expression is BinaryOp binaryOp){
            return EmitExpression(binaryOp.left)+binaryOp.op.value+EmitExpression(binaryOp.right);
        }
        else if(expression is UnaryOp unaryOp){
            return unaryOp.op.value+EmitExpression(unaryOp.expression);
        }
        else if(expression is Indexor indexor){
            return indexor.varname.value+"["+EmitExpression(indexor.indexExpression)+"]";
        }
        else if(expression is Var @var){
            return @var.type.value+" "+@var.name.value;
        }
        else{
            throw new Exception(expression.GetType().Name);
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

    static void EmitBody(Body body, ref string bodyCpp, ref string baseCpp){
        foreach(var s in body.statements){
            if(s is ImportFunction importFunction){
                baseCpp += importFunction.returnType.value+" "+importFunction.name.value+EmitParameters(importFunction.parameters)+"{\n";
                baseCpp += importFunction.cppCode.value;
                baseCpp += "}\n";
            }
            else if(s is Function function){
                baseCpp += function.returnType.value+" "+function.name.value+EmitParameters(function.parameters)+"{\n";
                var funcBody = "";
                EmitBody(function.body, ref funcBody, ref baseCpp);
                baseCpp+=funcBody;
                baseCpp += "}\n";
            }
            else if(s is Expression expression){
                bodyCpp += EmitExpression(expression.expression)+";\n";
            }
            else if(s is If @if){
                bodyCpp += "if("+EmitExpression(@if.condition)+"){\n";
                EmitBody(@if.body, ref bodyCpp, ref baseCpp);
                bodyCpp += "}\n";
            }
            else if(s is While @while){
                bodyCpp += "while("+EmitExpression(@while.condition)+"){\n";
                EmitBody(@while.body, ref bodyCpp, ref baseCpp);
                bodyCpp += "}\n";
            }
            else if(s is Break @break){
                bodyCpp += "break;\n";
            }
            else if(s is For @for){
                var i = @for.varname.value;
                bodyCpp += $"for(int {i}={EmitExpression(@for.start)};{i}<{EmitExpression(@for.end)};{i}++){{\n";
                EmitBody(@for.body, ref bodyCpp, ref baseCpp);
                bodyCpp += "}\n";
            }
            else if(s is Return @return){
                if(@return.expression == null){
                    bodyCpp += "return;\n";
                }
                else{
                    bodyCpp += "return "+EmitExpression(@return.expression)+";\n";
                }
            }
            else{
                throw new Exception(s.GetType().Name);
            }
        }
    }

    public static string Emit(Body body){
        string baseCpp = "#include <stdio.h>\n\n";
        string mainCpp = "";

        mainCpp += "int main(){\n";
        EmitBody(body, ref mainCpp, ref baseCpp);
        mainCpp += "return 0;\n";
        mainCpp += "}\n";

        return baseCpp + mainCpp;
    }
}