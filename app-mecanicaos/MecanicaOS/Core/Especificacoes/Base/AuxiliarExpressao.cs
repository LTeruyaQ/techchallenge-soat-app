using System.Linq.Expressions;

namespace Core.Especificacoes.Base
{
    internal static class AuxiliarExpressao
    {
        internal static string ObterCaminho(LambdaExpression expressao)
        {
            var pilha = new Stack<string>();
            var membro = expressao.Body as MemberExpression;
            var unario = expressao.Body as UnaryExpression;
            var chamadaMetodo = expressao.Body as MethodCallExpression;

            if (membro != null)
            {
                VisitarExpressaoMembro(membro, pilha);
            }
            else if (chamadaMetodo != null && chamadaMetodo.Method.Name == "Select" && chamadaMetodo.Arguments.Count == 2)
            {
                var lambda = chamadaMetodo.Arguments[1] as LambdaExpression;
                if (lambda != null)
                {
                    var expressaoMembro = chamadaMetodo.Arguments[0] as MemberExpression;
                    if (expressaoMembro != null)
                    {
                        VisitarExpressaoMembro(expressaoMembro, pilha);
                        var lambdaBodyMembro = lambda.Body as MemberExpression;
                        if (lambdaBodyMembro != null)
                        {
                            VisitarExpressaoMembro(lambdaBodyMembro, pilha);
                        }
                        else
                        {
                            throw new ArgumentException($"Expressão lambda.Body não é MemberExpression: {lambda.Body.GetType().Name}");
                        }
                    }
                }
            }
            else if (unario != null && unario.NodeType == ExpressionType.Convert)
            {
                var unarioOperandMembro = unario.Operand as MemberExpression;
                if (unarioOperandMembro != null)
                {
                    VisitarExpressaoMembro(unarioOperandMembro, pilha);
                }
                else
                {
                    throw new ArgumentException($"Expressão unario.Operand não é MemberExpression: {unario.Operand.GetType().Name}");
                }
            }
            else
            {
                throw new ArgumentException($"Expressão não suportada: {expressao.Body.GetType().Name}");
            }

            return string.Join(".", pilha);
        }

        private static void VisitarExpressaoMembro(MemberExpression membro, Stack<string> pilha)
        {
            if (membro == null) return;

            pilha.Push(membro.Member.Name);

            if (membro.Expression is MemberExpression membroInterno)
            {
                VisitarExpressaoMembro(membroInterno, pilha);
            }
        }
    }
}
