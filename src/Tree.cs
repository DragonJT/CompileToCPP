

class Variable(Token type, Token name)
{
    public Token type = type;
    public Token name = name;
}

interface INode{}

interface IExpression:INode{}

enum LiteralType{Float, Int, String, Char, Varname, True, False}
class Literal(LiteralType type, Token value) : IExpression{
    public LiteralType type = type;
    public Token value = value;

    public override string ToString(){
        return value.value;
    }
}

class BinaryOp(IExpression left, IExpression right, Token op) : IExpression{
    public IExpression left = left;
    public IExpression right = right;
    public Token op = op;

    public override string ToString() {
        return $"{left} {op.value} {right}";
    }
}

class UnaryOp(IExpression expression, Token op) : IExpression{
    public IExpression expression = expression;
    public Token op = op;
}

class Call(Token name, IExpression[] args) : IExpression{
    public Token name = name;
    public IExpression[] args = args;
}

class New(Token name, IExpression[] args) : IExpression{
    public Token name = name;
    public IExpression[] args = args;
}

interface IStatement:INode{}

class Expression(IExpression expression) : IStatement{
    public IExpression expression = expression;
}

class Indexor(Token varname, IExpression indexExpression) : IExpression{
    public Token varname = varname;
    public IExpression indexExpression = indexExpression;
}

class While(IExpression condition, Body body) : IStatement{
    public IExpression condition = condition;
    public Body body = body;
}

class For(Token varname, IExpression start, IExpression end, Body body) : IStatement{
    public Token varname = varname;
    public IExpression start = start;
    public IExpression end = end;
    public Body body = body;
}

class If(IExpression condition, Body body) : IStatement{
    public IExpression condition = condition;
    public Body body = body;
}

class Break:IStatement{
}

class Return(IExpression? expression) : IStatement{
    public IExpression? expression = expression;
}

class ImportFunction(Token returnType, Token name, Variable[] parameters, Token cppCode):IStatement{
    public Token returnType = returnType;
    public Token name = name;
    public Variable[] parameters = parameters;
    public Token cppCode = cppCode;
}

class Function(Token returnType, Token name, Variable[] parameters, Body body):IStatement
{
    public Token returnType = returnType;
    public Token name = name;
    public Variable[] parameters = parameters;
    public Body body = body;
}

class Body(List<IStatement> statements) : INode{
    public List<IStatement> statements = statements;
}
