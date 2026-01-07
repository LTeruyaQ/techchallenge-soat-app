using Adapters.Controllers;
using Adapters.Gateways;
using Adapters.Presenters;
using API.Jobs;
using API.Notificacoes;
using Core.DTOs.Config;
using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.Entidades.Cliente;
using Core.DTOs.Entidades.Estoque;
using Core.DTOs.Entidades.OrdemServicos;
using Core.DTOs.Entidades.Servico;
using Core.DTOs.Entidades.Usuarios;
using Core.DTOs.Entidades.Veiculo;
using Core.Interfaces.Controllers;
using Core.Interfaces.Eventos;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.AlertasEstoque;
using Core.Interfaces.Handlers.Autenticacao;
using Core.Interfaces.Handlers.Clientes;
using Core.Interfaces.Handlers.Estoques;
using Core.Interfaces.Handlers.InsumosOS;
using Core.Interfaces.Handlers.Orcamentos;
using Core.Interfaces.Handlers.OrdensServico;
using Core.Interfaces.Handlers.Servicos;
using Core.Interfaces.Handlers.Usuarios;
using Core.Interfaces.Handlers.Veiculos;
using Core.Interfaces.Jobs;
using Core.Interfaces.Presenters;
using Core.Interfaces.Repositorios;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using Core.Interfaces.UseCases;
using Core.UseCases.AlertasEstoque;
using Core.UseCases.AlertasEstoque.CadastrarVariosAlertas;
using Core.UseCases.AlertasEstoque.VerificarAlertaEnviadoHoje;
using Core.UseCases.Autenticacao;
using Core.UseCases.Autenticacao.AutenticarUsuario;
using Core.UseCases.Clientes;
using Core.UseCases.Clientes.AtualizarCliente;
using Core.UseCases.Clientes.CadastrarCliente;
using Core.UseCases.Clientes.ObterCliente;
using Core.UseCases.Clientes.ObterClientePorDocumento;
using Core.UseCases.Clientes.ObterClientePorNome;
using Core.UseCases.Clientes.ObterTodosClientes;
using Core.UseCases.Clientes.RemoverCliente;
using Core.UseCases.Estoques;
using Core.UseCases.Estoques.AtualizarEstoque;
using Core.UseCases.Estoques.CadastrarEstoque;
using Core.UseCases.Estoques.DeletarEstoque;
using Core.UseCases.Estoques.ObterEstoque;
using Core.UseCases.Estoques.ObterEstoqueCritico;
using Core.UseCases.Estoques.ObterTodosEstoques;
using Core.UseCases.InsumosOS;
using Core.UseCases.InsumosOS.CadastrarInsumos;
using Core.UseCases.Orcamentos;
using Core.UseCases.Orcamentos.GerarOrcamento;
using Core.UseCases.OrdensServico;
using Core.UseCases.OrdensServico.AceitarOrcamento;
using Core.UseCases.OrdensServico.AtualizarOrdemServico;
using Core.UseCases.OrdensServico.CadastrarOrdemServico;
using Core.UseCases.OrdensServico.ListarOrdensServicoAtivas;
using Core.UseCases.OrdensServico.ObterOrdemServico;
using Core.UseCases.OrdensServico.ObterOrdemServicoPorStatus;
using Core.UseCases.OrdensServico.ObterOrcamentosExpirados;
using Core.UseCases.OrdensServico.ObterTodosOrdensServico;
using Core.UseCases.OrdensServico.RecusarOrcamento;
using Core.UseCases.Servicos;
using Core.UseCases.Servicos.CadastrarServico;
using Core.UseCases.Servicos.DeletarServico;
using Core.UseCases.Servicos.EditarServico;
using Core.UseCases.Servicos.ObterServico;
using Core.UseCases.Servicos.ObterServicoPorNome;
using Core.UseCases.Servicos.ObterServicosDisponiveis;
using Core.UseCases.Servicos.ObterTodosServicos;
using Core.UseCases.Usuarios;
using Core.UseCases.Usuarios.AtualizarUsuario;
using Core.UseCases.Usuarios.CadastrarUsuario;
using Core.UseCases.Usuarios.DeletarUsuario;
using Core.UseCases.Usuarios.ObterTodosUsuarios;
using Core.UseCases.Usuarios.ObterUsuario;
using Core.UseCases.Usuarios.ObterUsuarioPorEmail;
using Core.UseCases.Usuarios.ObterUsuariosParaAlertaEstoque;
using Core.UseCases.Veiculos;
using Core.UseCases.Veiculos.AtualizarVeiculo;
using Core.UseCases.Veiculos.CadastrarVeiculo;
using Core.UseCases.Veiculos.DeletarVeiculo;
using Core.UseCases.Veiculos.ObterTodosVeiculos;
using Core.UseCases.Veiculos.ObterVeiculo;
using Core.UseCases.Veiculos.ObterVeiculoPorCliente;
using Core.UseCases.Veiculos.ObterVeiculoPorPlaca;
using Infraestrutura.Autenticacao;
using Infraestrutura.Dados;
using Infraestrutura.Dados.UdT;
using Infraestrutura.Logs;
using Infraestrutura.Repositorios;
using Infraestrutura.Servicos;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace API
{
    public class CompositionRoot : ICompositionRoot
    {
        private readonly MecanicaContexto _dbContext;

        private readonly IRepositorio<ClienteEntityDto> _repositorioCliente;
        private readonly IRepositorio<EnderecoEntityDto> _repositorioEndereco;
        private readonly IRepositorio<ContatoEntityDto> _repositorioContato;
        private readonly IRepositorio<OrdemServicoEntityDto> _repositorioOrdemServico;
        private readonly IRepositorio<InsumoOSEntityDto> _repositorioInsumoOS;
        private readonly IRepositorio<EstoqueEntityDto> _repositorioEstoque;
        private readonly IRepositorio<ServicoEntityDto> _repositorioServico;
        private readonly IRepositorio<UsuarioEntityDto> _repositorioUsuario;
        private readonly IRepositorio<VeiculoEntityDto> _repositorioVeiculo;
        private readonly IRepositorio<AlertaEstoqueEntityDto> _repositorioAlertaEstoque;

        private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;
        private readonly IUsuarioLogadoServico _usuarioLogadoServico;
        private readonly IIdCorrelacionalService _idCorrelacionalService;
        private readonly IVerificarEstoqueJob _verificarEstoqueJob;
        private readonly IServicoEmail _servicoEmail;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        private readonly ILogger<VerificarEstoqueJob> _loggerVerificarEstoqueJob;

        public CompositionRoot(MecanicaContexto contexto,
            IMediator mediator,
            IIdCorrelacionalService idCorrelacionalService,
            IHttpContextAccessor httpContext,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _dbContext = contexto;
            _mediator = mediator;

            _unidadeDeTrabalho = new UnidadeDeTrabalho(_dbContext);

            _repositorioCliente = new Repositorio<ClienteEntityDto>(_dbContext);
            _repositorioEndereco = new Repositorio<EnderecoEntityDto>(_dbContext);
            _repositorioContato = new Repositorio<ContatoEntityDto>(_dbContext);
            _repositorioOrdemServico = new Repositorio<OrdemServicoEntityDto>(_dbContext);
            _repositorioInsumoOS = new Repositorio<InsumoOSEntityDto>(_dbContext);
            _repositorioEstoque = new Repositorio<EstoqueEntityDto>(_dbContext);
            _repositorioServico = new Repositorio<ServicoEntityDto>(_dbContext);
            _repositorioUsuario = new Repositorio<UsuarioEntityDto>(_dbContext);
            _repositorioVeiculo = new Repositorio<VeiculoEntityDto>(_dbContext);
            _repositorioAlertaEstoque = new Repositorio<AlertaEstoqueEntityDto>(_dbContext);

            _loggerVerificarEstoqueJob = NullLogger<VerificarEstoqueJob>.Instance;


            _idCorrelacionalService = idCorrelacionalService;

            _usuarioLogadoServico = new UsuarioLogadoServico(httpContext, _repositorioUsuario);

            var alertaEstoqueGateway = new AlertaEstoqueGateway(_repositorioAlertaEstoque);
            var estoqueGateway = new EstoqueGateway(_repositorioEstoque);
            var usuarioGateway = new UsuarioGateway(_repositorioUsuario);

            var logServicoVerificarEstoqueJob = new LogServico<VerificarEstoqueJob>(
                _idCorrelacionalService,
                _loggerVerificarEstoqueJob,
                _usuarioLogadoServico);

            _servicoEmail = new ServicoEmail(configuration);

            _verificarEstoqueJob = new VerificarEstoqueJob(this);
        }

        public IRepositorio<T> CriarRepositorio<T>() where T : EntityDto
        {
            return new Repositorio<T>(_dbContext);
        }

        public ILogServico<T> CriarLogService<T>() where T : class
        {
            return new LogServico<T>(_idCorrelacionalService, NullLogger<T>.Instance, _usuarioLogadoServico);
        }

        public IServicoEmail CriarServicoEmail()
        {
            return _servicoEmail;
        }

        public IUnidadeDeTrabalho CriarUnidadeDeTrabalho()
        {
            return _unidadeDeTrabalho;
        }

        #region Criação de Gateways

        public IClienteGateway CriarClienteGateway()
        {
            return new ClienteGateway(_repositorioCliente);
        }

        public IEnderecoGateway CriarEnderecoGateway()
        {
            return new EnderecoGateway(_repositorioEndereco);
        }

        public IContatoGateway CriarContatoGateway()
        {
            return new ContatoGateway(_repositorioContato);
        }

        public IOrdemServicoGateway CriarOrdemServicoGateway()
        {
            return new OrdemServicoGateway(_repositorioOrdemServico);
        }

        public IInsumosGateway CriarInsumosGateway()
        {
            return new InsumosGateway(_repositorioInsumoOS);
        }

        public IEstoqueGateway CriarEstoqueGateway()
        {
            return new EstoqueGateway(_repositorioEstoque);
        }

        public IAlertaEstoqueGateway CriarAlertaEstoqueGateway()
        {
            return new AlertaEstoqueGateway(_repositorioAlertaEstoque);
        }

        public IVerificarEstoqueJobGateway CriarVerificarEstoqueJobGateway()
        {
            return new VerificarEstoqueJobGateway(_verificarEstoqueJob);
        }

        public IServicoGateway CriarServicoGateway()
        {
            return new ServicoGateway(_repositorioServico);
        }

        public IUsuarioGateway CriarUsuarioGateway()
        {
            return new UsuarioGateway(_repositorioUsuario);
        }

        public IVeiculoGateway CriarVeiculoGateway()
        {
            return new VeiculoGateway(_repositorioVeiculo);
        }

        public IEventoPublisher CriarOSFinalizadaEventosPublisher()
        {
            return new OrdemServicoFinalizadaEventoPublisher(_mediator);
        }
        
        public IEventoPublisher CriarOSCanceladaEventosPublisher()
        {
            return new OrdemServicoCanceladaEventoPublisher(_mediator);
        }
        
        public IEventoPublisher CriarOSEmOrcamentoEventosPublisher()
        {
            return new OrdemServicoEmOrcamentoEventoPublisher(_mediator);
        }

        public IEventoGateway CriarEventosGateway()
        {
            var osCanceladaEventoPublisher = CriarOSCanceladaEventosPublisher();
            var osEmOrcamentoEventoPublisher = CriarOSEmOrcamentoEventosPublisher();
            var osFinalizadaEventoPublisher = CriarOSFinalizadaEventosPublisher();
            return new EventosGateway([osCanceladaEventoPublisher, osEmOrcamentoEventoPublisher, osFinalizadaEventoPublisher]);
        }

        public ILogGateway<T> CriarLogServicoGateway<T>() where T : class
        {
            var logServico = new LogServico<T>(_idCorrelacionalService, NullLogger<T>.Instance, _usuarioLogadoServico);
            return new LogGateway<T>(logServico);
        }

        public IUnidadeDeTrabalhoGateway CriarUnidadeDeTrabalhoGateway()
        {
            return new UnidadeDeTrabalhoGateway(_unidadeDeTrabalho);
        }

        public IUsuarioLogadoServicoGateway CriarUsuarioLogadoServicoGateway()
        {
            return new UsuarioLogadoServicoGateway(_usuarioLogadoServico);
        }
        #endregion

        #region Criação de Handlers Individuais - Cliente

        public ICadastrarClienteHandler CriarCadastrarClienteHandler()
        {
            var clienteGateway = CriarClienteGateway();
            var enderecoGateway = CriarEnderecoGateway();
            var contatoGateway = CriarContatoGateway();
            var logServicoGateway = CriarLogServicoGateway<CadastrarClienteHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new CadastrarClienteHandler(
                clienteGateway,
                enderecoGateway,
                contatoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IAtualizarClienteHandler CriarAtualizarClienteHandler()
        {
            var clienteGateway = CriarClienteGateway();
            var enderecoGateway = CriarEnderecoGateway();
            var contatoGateway = CriarContatoGateway();
            var logServicoGateway = CriarLogServicoGateway<AtualizarClienteHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new AtualizarClienteHandler(
                clienteGateway,
                enderecoGateway,
                contatoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterClienteHandler CriarObterClienteHandler()
        {
            var clienteGateway = CriarClienteGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterClienteHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterClienteHandler(
                clienteGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterTodosClientesHandler CriarObterTodosClientesHandler()
        {
            var clienteGateway = CriarClienteGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterTodosClientesHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterTodosClientesHandler(
                clienteGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IRemoverClienteHandler CriarRemoverClienteHandler()
        {
            var clienteGateway = CriarClienteGateway();
            var logServicoGateway = CriarLogServicoGateway<RemoverClienteHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new RemoverClienteHandler(
                clienteGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterClientePorDocumentoHandler CriarObterClientePorDocumentoHandler()
        {
            var clienteGateway = CriarClienteGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterClientePorDocumentoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterClientePorDocumentoHandler(
                clienteGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterClientePorNomeHandler CriarObterClientePorNomeHandler()
        {
            var clienteGateway = CriarClienteGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterClientePorNomeHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterClientePorNomeHandler(
                clienteGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Handlers Individuais - Veículo

        public ICadastrarVeiculoHandler CriarCadastrarVeiculoHandler()
        {
            var veiculoGateway = CriarVeiculoGateway();
            var logServicoGateway = CriarLogServicoGateway<CadastrarVeiculoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new CadastrarVeiculoHandler(
                veiculoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IAtualizarVeiculoHandler CriarAtualizarVeiculoHandler()
        {
            var veiculoGateway = CriarVeiculoGateway();
            var logServicoGateway = CriarLogServicoGateway<AtualizarVeiculoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new AtualizarVeiculoHandler(
                veiculoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterVeiculoHandler CriarObterVeiculoHandler()
        {
            var veiculoGateway = CriarVeiculoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterVeiculoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterVeiculoHandler(
                veiculoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterTodosVeiculosHandler CriarObterTodosVeiculosHandler()
        {
            var veiculoGateway = CriarVeiculoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterTodosVeiculosHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterTodosVeiculosHandler(
                veiculoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterVeiculoPorClienteHandler CriarObterVeiculoPorClienteHandler()
        {
            var veiculoGateway = CriarVeiculoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterVeiculoPorClienteHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterVeiculoPorClienteHandler(
                veiculoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterVeiculoPorPlacaHandler CriarObterVeiculoPorPlacaHandler()
        {
            var veiculoGateway = CriarVeiculoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterVeiculoPorPlacaHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterVeiculoPorPlacaHandler(
                veiculoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IDeletarVeiculoHandler CriarDeletarVeiculoHandler()
        {
            var veiculoGateway = CriarVeiculoGateway();
            var logServicoGateway = CriarLogServicoGateway<DeletarVeiculoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new DeletarVeiculoHandler(
                veiculoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Handlers Individuais - Usuário

        public ICadastrarUsuarioHandler CriarCadastrarUsuarioHandler()
        {
            var usuarioGateway = CriarUsuarioGateway();
            var logGateway = CriarLogServicoGateway<CadastrarUsuarioHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();
            var servicoSenha = new ServicoSenha();

            return new CadastrarUsuarioHandler(
                usuarioGateway,
                logGateway,
                udtGateway,
                usuarioLogadoServicoGateway,
                servicoSenha);
        }
        public IAtualizarUsuarioHandler CriarAtualizarUsuarioHandler()
        {
            var usuarioGateway = CriarUsuarioGateway();
            var servicoSenha = new ServicoSenha();
            var logServicoGateway = CriarLogServicoGateway<AtualizarUsuarioHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new AtualizarUsuarioHandler(
                usuarioGateway,
                servicoSenha,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterUsuarioHandler CriarObterUsuarioHandler()
        {
            var usuarioGateway = CriarUsuarioGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterUsuarioHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterUsuarioHandler(
                usuarioGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterTodosUsuariosHandler CriarObterTodosUsuariosHandler()
        {
            var usuarioGateway = CriarUsuarioGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterTodosUsuariosHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterTodosUsuariosHandler(
                usuarioGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IDeletarUsuarioHandler CriarDeletarUsuarioHandler()
        {
            var usuarioGateway = CriarUsuarioGateway();
            var logServicoGateway = CriarLogServicoGateway<DeletarUsuarioHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new DeletarUsuarioHandler(
                usuarioGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterUsuarioPorEmailHandler CriarObterUsuarioPorEmailHandler()
        {
            var usuarioGateway = CriarUsuarioGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterUsuarioPorEmailHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterUsuarioPorEmailHandler(
                usuarioGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterUsuariosParaAlertaEstoqueHandler CriarObterUsuariosParaAlertaEstoqueHandler()
        {
            var usuarioGateway = CriarUsuarioGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterUsuariosParaAlertaEstoqueHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterUsuariosParaAlertaEstoqueHandler(
                usuarioGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Handlers Individuais - Serviço

        public ICadastrarServicoHandler CriarCadastrarServicoHandler()
        {
            var servicoGateway = CriarServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<CadastrarServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new CadastrarServicoHandler(
                servicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IEditarServicoHandler CriarEditarServicoHandler()
        {
            var servicoGateway = CriarServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<EditarServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new EditarServicoHandler(
                servicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IDeletarServicoHandler CriarDeletarServicoHandler()
        {
            var servicoGateway = CriarServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<DeletarServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new DeletarServicoHandler(
                servicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterServicoHandler CriarObterServicoHandler()
        {
            var servicoGateway = CriarServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterServicoHandler(
                servicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterTodosServicosHandler CriarObterTodosServicosHandler()
        {
            var servicoGateway = CriarServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterTodosServicosHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterTodosServicosHandler(
                servicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterServicoPorNomeHandler CriarObterServicoPorNomeHandler()
        {
            var servicoGateway = CriarServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterServicoPorNomeHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterServicoPorNomeHandler(
                servicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterServicosDisponiveisHandler CriarObterServicosDisponiveisHandler()
        {
            var servicoGateway = CriarServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterServicosDisponiveisHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterServicosDisponiveisHandler(
                servicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Handlers Individuais - Orçamento

        public IGerarOrcamentoHandler CriarGerarOrcamentoHandler()
        {
            var logServicoGateway = CriarLogServicoGateway<GerarOrcamentoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new GerarOrcamentoHandler(
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Handlers Individuais - Estoque

        public ICadastrarEstoqueHandler CriarCadastrarEstoqueHandler()
        {
            var estoqueGateway = CriarEstoqueGateway();
            var logServicoGateway = CriarLogServicoGateway<CadastrarEstoqueHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new CadastrarEstoqueHandler(
                estoqueGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IAtualizarEstoqueHandler CriarAtualizarEstoqueHandler()
        {
            var estoqueGateway = CriarEstoqueGateway();
            var logServicoGateway = CriarLogServicoGateway<AtualizarEstoqueHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new AtualizarEstoqueHandler(
                estoqueGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IDeletarEstoqueHandler CriarDeletarEstoqueHandler()
        {
            var estoqueGateway = CriarEstoqueGateway();
            var logServicoGateway = CriarLogServicoGateway<DeletarEstoqueHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new DeletarEstoqueHandler(
                estoqueGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterEstoqueHandler CriarObterEstoqueHandler()
        {
            var estoqueGateway = CriarEstoqueGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterEstoqueHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterEstoqueHandler(
                estoqueGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterTodosEstoquesHandler CriarObterTodosEstoquesHandler()
        {
            var estoqueGateway = CriarEstoqueGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterTodosEstoquesHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterTodosEstoquesHandler(
                estoqueGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterEstoqueCriticoHandler CriarObterEstoqueCriticoHandler()
        {
            var estoqueGateway = CriarEstoqueGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterEstoqueCriticoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterEstoqueCriticoHandler(
                estoqueGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Handlers Individuais - Autenticação

        public ISegurancaGateway CriarSegurancaGateway()
        {
            var servicoSenha = new ServicoSenha();
            var servicoJwt = CriarServicoJwt();
            return new SegurancaGateway(servicoSenha, servicoJwt);
        }

        public IAutenticarUsuarioHandler CriarAutenticarUsuarioHandler()
        {
            var clienteUseCases = CriarClienteUseCases();
            var segurancaGateway = CriarSegurancaGateway();
            var logServicoGateway = CriarLogServicoGateway<AutenticarUsuarioHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new AutenticarUsuarioHandler(
                segurancaGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Handlers Individuais - InsumoOS
        public ICadastrarInsumosHandler CriarCadastrarInsumosHandler()
        {
            var verificarEstoqueJobGateway = CriarVerificarEstoqueJobGateway();
            var logServicoGateway = CriarLogServicoGateway<CadastrarInsumosHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();
            var insumosGateway = CriarInsumosGateway();

            return new CadastrarInsumosHandler(
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway,
                verificarEstoqueJobGateway,
                insumosGateway);
        }
        #endregion

        #region Criação de Handlers Individuais - OrdemServico

        public ICadastrarOrdemServicoHandler CriarCadastrarOrdemServicoHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<CadastrarOrdemServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new CadastrarOrdemServicoHandler(
                ordemServicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IAtualizarOrdemServicoHandler CriarAtualizarOrdemServicoHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var eventosGateway = CriarEventosGateway();
            var logServicoGateway = CriarLogServicoGateway<AtualizarOrdemServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new AtualizarOrdemServicoHandler(
                ordemServicoGateway,
                eventosGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterOrdemServicoHandler CriarObterOrdemServicoHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterOrdemServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterOrdemServicoHandler(
                ordemServicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterTodosOrdensServicoHandler CriarObterTodosOrdensServicoHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterTodosOrdensServicoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterTodosOrdensServicoHandler(
                ordemServicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterOrdemServicoPorStatusHandler CriarObterOrdemServicoPorStatusHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterOrdemServicoPorStatusHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterOrdemServicoPorStatusHandler(
                ordemServicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IAceitarOrcamentoHandler CriarAceitarOrcamentoHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<AceitarOrcamentoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new AceitarOrcamentoHandler(
                ordemServicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IRecusarOrcamentoHandler CriarRecusarOrcamentoHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var eventosGateway = CriarEventosGateway();
            var logServicoGateway = CriarLogServicoGateway<RecusarOrcamentoHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new RecusarOrcamentoHandler(
                ordemServicoGateway,
                eventosGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IListarOrdensServicoAtivasHandler CriarListarOrdensServicoAtivasHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ListarOrdensServicoAtivasHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ListarOrdensServicoAtivasHandler(
                ordemServicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IObterOrcamentosExpiradosHandler CriarObterOrcamentosExpiradosHandler()
        {
            var ordemServicoGateway = CriarOrdemServicoGateway();
            var logServicoGateway = CriarLogServicoGateway<ObterOrcamentosExpiradosHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new ObterOrcamentosExpiradosHandler(
                ordemServicoGateway,
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        #endregion

        #region Criação de Use Cases

        public IClienteUseCases CriarClienteUseCases()
        {
            var cadastrarClienteHandler = CriarCadastrarClienteHandler();
            var atualizarClienteHandler = CriarAtualizarClienteHandler();
            var obterClienteHandler = CriarObterClienteHandler();
            var obterTodosClientesHandler = CriarObterTodosClientesHandler();
            var removerClienteHandler = CriarRemoverClienteHandler();
            var obterClientePorDocumentoHandler = CriarObterClientePorDocumentoHandler();
            var obterClientePorNomeHandler = CriarObterClientePorNomeHandler();

            return new ClienteUseCasesFacade(
                cadastrarClienteHandler,
                atualizarClienteHandler,
                obterClienteHandler,
                obterTodosClientesHandler,
                removerClienteHandler,
                obterClientePorDocumentoHandler,
                obterClientePorNomeHandler);
        }

        public IServicoUseCases CriarServicoUseCases()
        {
            var cadastrarServicoHandler = CriarCadastrarServicoHandler();
            var editarServicoHandler = CriarEditarServicoHandler();
            var deletarServicoHandler = CriarDeletarServicoHandler();
            var obterServicoHandler = CriarObterServicoHandler();
            var obterTodosServicosHandler = CriarObterTodosServicosHandler();
            var obterServicoPorNomeHandler = CriarObterServicoPorNomeHandler();
            var obterServicosDisponiveisHandler = CriarObterServicosDisponiveisHandler();

            return new ServicoUseCasesFacade(
                cadastrarServicoHandler,
                editarServicoHandler,
                deletarServicoHandler,
                obterServicoHandler,
                obterTodosServicosHandler,
                obterServicoPorNomeHandler,
                obterServicosDisponiveisHandler);
        }

        public IEstoqueUseCases CriarEstoqueUseCases()
        {
            var cadastrarEstoqueHandler = CriarCadastrarEstoqueHandler();
            var atualizarEstoqueHandler = CriarAtualizarEstoqueHandler();
            var deletarEstoqueHandler = CriarDeletarEstoqueHandler();
            var obterEstoqueHandler = CriarObterEstoqueHandler();
            var obterTodosEstoquesHandler = CriarObterTodosEstoquesHandler();
            var obterEstoqueCriticoHandler = CriarObterEstoqueCriticoHandler();

            return new EstoqueUseCasesFacade(
                cadastrarEstoqueHandler,
                atualizarEstoqueHandler,
                deletarEstoqueHandler,
                obterEstoqueHandler,
                obterTodosEstoquesHandler,
                obterEstoqueCriticoHandler);
        }

        public IOrdemServicoUseCases CriarOrdemServicoUseCases()
        {
            var cadastrarOrdemServicoHandler = CriarCadastrarOrdemServicoHandler();
            var atualizarOrdemServicoHandler = CriarAtualizarOrdemServicoHandler();
            var obterOrdemServicoHandler = CriarObterOrdemServicoHandler();
            var obterTodosOrdensServicoHandler = CriarObterTodosOrdensServicoHandler();
            var obterOrdemServicoPorStatusHandler = CriarObterOrdemServicoPorStatusHandler();
            var aceitarOrcamentoHandler = CriarAceitarOrcamentoHandler();
            var recusarOrcamentoHandler = CriarRecusarOrcamentoHandler();
            var listarOrdensServicoAtivasHandler = CriarListarOrdensServicoAtivasHandler();
            var obterOrcamentosExpiradosHandler = CriarObterOrcamentosExpiradosHandler();

            return new OrdemServicoUseCasesFacade(
                cadastrarOrdemServicoHandler,
                atualizarOrdemServicoHandler,
                obterOrdemServicoHandler,
                obterTodosOrdensServicoHandler,
                obterOrdemServicoPorStatusHandler,
                aceitarOrcamentoHandler,
                recusarOrcamentoHandler,
                listarOrdensServicoAtivasHandler,
                obterOrcamentosExpiradosHandler);
        }

        public IInsumoOSUseCases CriarInsumoOSUseCases()
        {
            var cadastrarInsumosHandler = CriarCadastrarInsumosHandler();

            return new InsumoOSUseCasesFacade(
                cadastrarInsumosHandler);
        }

        public IUsuarioUseCases CriarUsuarioUseCases()
        {
            var cadastrarUsuarioHandler = CriarCadastrarUsuarioHandler();
            var atualizarUsuarioHandler = CriarAtualizarUsuarioHandler();
            var obterUsuarioHandler = CriarObterUsuarioHandler();
            var obterTodosUsuariosHandler = CriarObterTodosUsuariosHandler();
            var deletarUsuarioHandler = CriarDeletarUsuarioHandler();
            var obterUsuarioPorEmailHandler = CriarObterUsuarioPorEmailHandler();
            var obterUsuariosParaAlertaEstoqueHandler = CriarObterUsuariosParaAlertaEstoqueHandler();

            return new UsuarioUseCasesFacade(
                cadastrarUsuarioHandler,
                atualizarUsuarioHandler,
                obterUsuarioHandler,
                obterTodosUsuariosHandler,
                deletarUsuarioHandler,
                obterUsuarioPorEmailHandler,
                obterUsuariosParaAlertaEstoqueHandler);
        }

        public IVeiculoUseCases CriarVeiculoUseCases()
        {
            var cadastrarVeiculoHandler = CriarCadastrarVeiculoHandler();
            var atualizarVeiculoHandler = CriarAtualizarVeiculoHandler();
            var obterVeiculoHandler = CriarObterVeiculoHandler();
            var obterTodosVeiculosHandler = CriarObterTodosVeiculosHandler();
            var obterVeiculoPorClienteHandler = CriarObterVeiculoPorClienteHandler();
            var obterVeiculoPorPlacaHandler = CriarObterVeiculoPorPlacaHandler();
            var deletarVeiculoHandler = CriarDeletarVeiculoHandler();

            return new VeiculoUseCasesFacade(
                cadastrarVeiculoHandler,
                atualizarVeiculoHandler,
                obterVeiculoHandler,
                obterTodosVeiculosHandler,
                obterVeiculoPorClienteHandler,
                obterVeiculoPorPlacaHandler,
                deletarVeiculoHandler);
        }

        public IOrcamentoUseCases CriarOrcamentoUseCases()
        {
            var gerarOrcamentoHandler = CriarGerarOrcamentoHandler();
            return new OrcamentoUseCasesFacade(gerarOrcamentoHandler);
        }

        public ConfiguracaoJwt CriarConfiguracaoJwt()
        {
            var configuracao = new ConfiguracaoJwt
            {
                SecretKey = _configuration["Jwt:SecretKey"],
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                ExpiryInMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"])
            };

            return configuracao;
        }

        public IServicoJwt CriarServicoJwt()
        {
            var configuracaoJwt = CriarConfiguracaoJwt();
            return new ServicoJwt(configuracaoJwt);
        }

        public IAutenticacaoUseCases CriarAutenticacaoUseCases()
        {
            var autenticarUsuarioHandler = CriarAutenticarUsuarioHandler();
            return new AutenticacaoUseCasesFacade(autenticarUsuarioHandler);
        }

        #endregion

        #region Criação de Presenters

        public IClientePresenter CriarClientePresenter()
        {
            return new ClientePresenter();
        }

        public IOrdemServicoPresenter CriarOrdemServicoPresenter()
        {
            return new OrdemServicoPresenter();
        }

        public IServicoPresenter CriarServicoPresenter()
        {
            return new ServicoPresenter();
        }

        public IEstoquePresenter CriarEstoquePresenter()
        {
            return new EstoquePresenter();
        }

        public IUsuarioPresenter CriarUsuarioPresenter()
        {
            return new UsuarioPresenter();
        }

        public IVeiculoPresenter CriarVeiculoPresenter()
        {
            return new VeiculoPresenter();
        }

        public IAutenticacaoPresenter CriarAutenticacaoPresenter()
        {
            return new AutenticacaoPresenter();
        }

        #endregion

        #region Criação de Serviços

        public IUsuarioLogadoServico CriarUsuarioLogadoServico()
        {
            return _usuarioLogadoServico;
        }

        #endregion

        #region Criação de Controllers

        public IClienteController CriarClienteController()
        {
            return new ClienteController(this);
        }

        public IOrdemServicoController CriarOrdemServicoController()
        {
            return new OrdemServicoController(this);
        }

        public IInsumoOSController CriarInsumoOSController()
        {
            return new InsumoOSController(this);
        }

        public IServicoController CriarServicoController()
        {
            return new ServicoController(this);
        }

        public IEstoqueController CriarEstoqueController()
        {
            return new EstoqueController(this);
        }

        public IUsuarioController CriarUsuarioController()
        {
            return new UsuarioController(this);
        }

        public IVeiculoController CriarVeiculoController()
        {
            return new VeiculoController(this);
        }

        public IAutenticacaoController CriarAutenticacaoController()
        {
            return new AutenticacaoController(this);
        }

        public IAlertaEstoqueController CriarAlertaEstoqueController()
        {
            return new AlertaEstoqueController(this);
        }

        public IAlertaEstoqueUseCases CriarAlertaEstoqueUseCases()
        {
            return new AlertaEstoqueUseCasesFacade(
                CriarCadastrarVariosAlertasHandler(),
                CriarVerificarAlertaEnviadoHojeHandler());
        }

        public ICadastrarVariosAlertasHandler CriarCadastrarVariosAlertasHandler()
        {
            var logServicoGateway = CriarLogServicoGateway<CadastrarVariosAlertasHandler>();
            var udtGateway = CriarUnidadeDeTrabalhoGateway();
            var usuarioLogadoServicoGateway = CriarUsuarioLogadoServicoGateway();

            return new CadastrarVariosAlertasHandler(
                CriarAlertaEstoqueGateway(),
                logServicoGateway,
                udtGateway,
                usuarioLogadoServicoGateway);
        }

        public IVerificarAlertaEnviadoHojeHandler CriarVerificarAlertaEnviadoHojeHandler()
        {
            return new VerificarAlertaEnviadoHojeHandler(CriarAlertaEstoqueGateway());
        }

        #endregion
    }
}
