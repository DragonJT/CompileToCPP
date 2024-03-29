

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
            if(literal.type == LiteralType.String){
                return '"'+literal.value.value+'"';
            }
            else if(literal.type == LiteralType.Char){
                return '\''+literal.value.value+'\'';
            }
            else{
                return literal.value.value;
            }
        }
        else if(expression is BinaryOp binaryOp){
            return EmitExpression(binaryOp.left)+binaryOp.op.value+EmitExpression(binaryOp.right);
        }
        else if(expression is PrefixUnaryOp prefixUnaryOp){
            return prefixUnaryOp.op.value+EmitExpression(prefixUnaryOp.expression);
        }
        else if(expression is PostfixUnaryOp postfixUnaryOp){
            return EmitExpression(postfixUnaryOp.expression)+postfixUnaryOp.op.value;
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

    static string EmitBody(Body body){
        var cpp = "";
        foreach(var s in body.statements){
            if(s is Expression expression){
                cpp += EmitExpression(expression.expression)+";\n";
            }
            else if(s is If @if){
                cpp += "if("+EmitExpression(@if.condition)+"){\n";
                cpp += EmitBody(@if.body);
                cpp += "}\n";
            }
            else if(s is While @while){
                cpp += "while("+EmitExpression(@while.condition)+"){\n";
                cpp += EmitBody(@while.body);
                cpp += "}\n";
            }
            else if(s is Break @break){
                cpp += "break;\n";
            }
            else if(s is For @for){
                var i = @for.varname.value;
                cpp += $"for(int {i}={EmitExpression(@for.start)};{i}<{EmitExpression(@for.end)};{i}++){{\n";
                cpp += EmitBody(@for.body);
                cpp += "}\n";
            }
            else if(s is Return @return){
                if(@return.expression == null){
                    cpp += "return;\n";
                }
                else{
                    cpp += "return "+EmitExpression(@return.expression)+";\n";
                }
            }
            else{
                throw new Exception(s.GetType().Name);
            }
        }
        return cpp;
    }

    public static string EmitRoot(Root root){
        string cpp = "#include <stdio.h>\n\n";
        foreach(var d in root.declarations){
            if(d is Function function){
                cpp += function.returnType.value+" "+function.name.value+EmitParameters(function.parameters)+"{\n";
                cpp += EmitBody(function.body);
                cpp += "}\n";
            }
            else if(d is Global global){
                cpp += EmitExpression(global.left)+"="+EmitExpression(global.right);
            }
        }
        return cpp;
    }
}