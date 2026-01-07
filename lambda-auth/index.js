// Inicializa o tracer do Datadog. Deve ser a primeira linha do código.
const tracer = require('dd-trace').init();

const jwt = require('jsonwebtoken');
const { Pool } = require('pg');

const pool = new Pool({
  user: process.env.DB_USER,
  host: process.env.DB_HOST,
  database: process.env.DB_NAME,
  password: process.env.DB_PASSWORD,
  port: 5432,
});

// Função de log estruturado em JSON
const log = (level, message, context = {}) => {
  const logEntry = {
    level,
    message,
    ...context,
    timestamp: new Date().toISOString(),
  };
  console.log(JSON.stringify(logEntry));
};

exports.handler = async (event) => {
  const correlationId = event.requestContext?.requestId;
  const logContext = { correlationId };

  const body = JSON.parse(event.body);
  const { cpf } = body;

  if (!cpf) {
    log('warn', 'CPF ausente na requisição.', { ...logContext, statusCode: 400 });
    return {
      statusCode: 400,
      body: JSON.stringify({ message: 'CPF é obrigatório.' }),
    };
  }

  logContext.cpf = cpf; // Adiciona o CPF ao contexto para logs futuros

  try {
    const queryResult = await pool.query('SELECT id, nome, status FROM clientes WHERE cpf = $1', [cpf]);

    if (queryResult.rows.length === 0) {
      log('warn', 'Cliente não encontrado.', { ...logContext, statusCode: 404 });
      return {
        statusCode: 404,
        body: JSON.stringify({ message: 'Cliente não encontrado.' }),
      };
    }

    const cliente = queryResult.rows[0];
    logContext.clienteId = cliente.id;

    if (cliente.status !== 'Ativo') {
       log('warn', 'Tentativa de login de cliente inativo.', { ...logContext, statusCode: 401 });
       return {
        statusCode: 401,
        body: JSON.stringify({ message: 'Cliente inativo.' }),
      };
    }

    const token = jwt.sign(
      { id: cliente.id, nome: cliente.nome },
      process.env.JWT_SECRET,
      { expiresIn: '1h' }
    );

    log('info', 'Autenticação bem-sucedida.', { ...logContext, statusCode: 200 });
    return {
      statusCode: 200,
      body: JSON.stringify({ token }),
    };

  } catch (error) {
    log('error', 'Erro na autenticação.', { ...logContext, error: error.message, stack: error.stack, statusCode: 500 });
    return {
      statusCode: 500,
      body: JSON.stringify({ message: 'Erro interno do servidor.' }),
    };
  }
};
