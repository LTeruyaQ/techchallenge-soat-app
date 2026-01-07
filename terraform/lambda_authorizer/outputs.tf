# lambda_authorizer/outputs.tf

output "lambda_function_arn" {
  description = "ARN da função Lambda do Authorizer"
  value       = aws_lambda_function.authorizer.arn
}

output "lambda_function_invoke_arn" {
  description = "ARN de invocação da função Lambda do Authorizer"
  value       = aws_lambda_function.authorizer.invoke_arn
}

output "lambda_function_name" {
  description = "Nome da função Lambda do Authorizer"
  value       = aws_lambda_function.authorizer.function_name
}
