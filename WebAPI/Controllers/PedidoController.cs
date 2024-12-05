using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Domain.Enums;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.Application.Exceptions;
using GestorPedidoAPI.Application.Commons;
using GestorPedidoAPI.Application.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace GestorPedidoAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidoController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidoController(AppDbContext context)
    {
        _context = context;
    }

    private PedidoEntity ObterPedidoComValidacao(int pedidoId)
    {
        var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);
        if (pedido == null)
        {
            throw new PedidoException($"Pedido com ID {pedidoId} não encontrado.");
        }
        return pedido;
    }

    private ProdutoEntity ObterProdutoComValidacao(int produtoId)
    {
        var produto = _context.Produtos.Find(produtoId);
        if (produto == null)
        {
            throw new PedidoException($"Produto com ID {produtoId} não encontrado.");
        }

        return produto;
    }

    /// <summary>
    /// Cria um novo pedido com uma lista inicial de produtos.
    /// </summary>
    /// <param name="criarPedidoDto">Objeto contendo os detalhes dos produtos a serem adicionados ao pedido.</param>
    /// <returns>O pedido criado, incluindo os produtos adicionados.</returns>
    /// <remarks>
    /// Regras de validação:
    /// - O pedido deve conter pelo menos um produto.
    /// - Todos os produtos devem existir no banco de dados.
    /// - Cada produto deve ter uma quantidade maior que zero.
    /// - Produtos duplicados (mesmo ID) não são permitidos no pedido.
    /// 
    /// Exemplo de request body:
    /// {
    ///     "produtos": [
    ///         { "produtoId": 1, "quantidade": 2 },
    ///         { "produtoId": 2, "quantidade": 1 }
    ///     ]
    /// }
    /// </remarks>
    /// <response code="201">Pedido criado com sucesso.</response>
    /// <response code="400">
    /// Retornado em caso de:
    /// - Nenhum produto fornecido.
    /// - Produto não encontrado no banco de dados.
    /// - Quantidade inválida (menor ou igual a zero).
    /// - Produto duplicado no mesmo pedido.
    /// </response>
    [HttpPost("criar")]
    [SwaggerOperation(
        Summary = "Cria um novo pedido com produtos",
        Description = @"
    Cria um novo pedido com uma lista inicial de produtos, validando:
    - Produtos ausentes ou inexistentes.
    - Quantidade inválida.
    - Produtos duplicados.
    ")]
    [SwaggerResponse(201, "Pedido criado com sucesso.", typeof(PedidoDto))]
    [SwaggerResponse(400, "Dados inválidos ou erro de validação.")]

    public IActionResult CriarPedido([FromBody] CriarPedidoDto criarPedidoDto)
    {
        // Verifica se a lista de produtos é nula ou vazia
        if (criarPedidoDto.Produtos == null || !criarPedidoDto.Produtos.Any())
        {
            return BadRequest("Um pedido deve conter pelo menos um produto.");
        }

        // Cria o pedido com status inicial "Aberto"
        var pedido = new PedidoEntity
        {
            Status = PedidoStatus.Aberto.ToString()
        };
        _context.Pedidos.Add(pedido);

        // Lista auxiliar para validar produtos duplicados no pedido
        var produtosNoPedido = new HashSet<int>();

        // Valida e adiciona cada produto ao pedido
        foreach (var produtoDto in criarPedidoDto.Produtos)
        {
            // Verifica se o produto já foi adicionado ao pedido
            if (!produtosNoPedido.Add(produtoDto.ProdutoId))
            {
                return BadRequest($"O produto com ID {produtoDto.ProdutoId} foi adicionado mais de uma vez ao pedido.");
            }

            // Verifica se o produto existe no banco de dados
            ProdutoEntity produto;
            try
            {
                produto = ObterProdutoComValidacao(produtoDto.ProdutoId);
            }
            catch (PedidoException ex)
            {
                return NotFound(ex.Message);
            }

            // Valida se a quantidade é maior que zero
            if (produtoDto.Quantidade <= 0)
            {
                return BadRequest($"Quantidade inválida para o produto com ID {produtoDto.ProdutoId}. Deve ser maior que zero.");
            }

            // Adiciona o produto ao pedido
            var pedidoProduto = new PedidoProduto
            {
                PedidoId = pedido.Id,
                ProdutoId = produto.Id,
                Quantidade = produtoDto.Quantidade
            };
            _context.PedidoProdutos.Add(pedidoProduto);
        }

        // Salva as alterações no banco de dados
        _context.SaveChanges();

        return Ok($"Pedido com ID {pedido.Id} criado com sucesso.");
    }

    /// <summary>
    /// Adiciona produtos a um pedido existente.
    /// </summary>
    /// <param name="pedidoId">ID do pedido.</param>
    /// <param name="produtosDto">Lista de produtos a serem adicionados ao pedido.</param>
    /// <returns>Mensagem de sucesso ou erro detalhada.</returns>
    /// <remarks>
    /// Este endpoint adiciona um ou mais produtos a um pedido existente, considerando as seguintes regras:
    /// - O pedido deve existir.
    /// - O pedido não pode estar com o status "Fechado".
    /// - Todos os produtos devem existir no banco de dados.
    /// - Produtos duplicados (mesmo ID) não podem ser adicionados no mesmo request.
    /// - A quantidade de cada produto deve ser maior que zero.
    /// 
    /// Exemplo de request body:
    /// {
    ///     "produtos": [
    ///         { "produtoId": 1, "quantidade": 2 },
    ///         { "produtoId": 2, "quantidade": 3 }
    ///     ]
    /// }
    /// </remarks>
    /// <response code="200">Produtos adicionados com sucesso ao pedido.</response>
    /// <response code="400">
    /// Retornado em caso de:
    /// - Pedido fechado.
    /// - Produto duplicado no request.
    /// - Quantidade inválida.
    /// </response>
    /// <response code="404">Pedido ou produto não encontrado.</response>
    [HttpPost("{pedidoId}/adicionar-produtos")]
    [SwaggerOperation(
        Summary = "Adiciona produtos ao pedido",
        Description = @"
            Adiciona um ou mais produtos a um pedido existente, considerando as seguintes regras:
            - O pedido deve existir.
            - O pedido não pode estar com o status 'Fechado'.
            - Todos os produtos devem existir no banco de dados.
            - Produtos duplicados (mesmo ID) não podem ser adicionados no mesmo request.
            - A quantidade de cada produto deve ser maior que zero.

            Exemplo de request:
            POST /api/pedido/{pedidoId}/adicionar-produtos
            Body:
            {
                ""produtos"": [
                    { ""produtoId"": 1, ""quantidade"": 2 },
                    { ""produtoId"": 2, ""quantidade"": 3 }
                ]
            }
            "
    )]
    [SwaggerResponse(200, "Produtos adicionados com sucesso ao pedido.")]
    [SwaggerResponse(400, "Dados inválidos ou erro de validação.")]
    [SwaggerResponse(404, "Pedido ou produto não encontrado.")]

    public IActionResult AdicionarProduto(int pedidoId, [FromBody] List<ProdutoPedidoDto> produtosDto)
    {
        if (produtosDto == null || !produtosDto.Any())
            return BadRequest("A lista de produtos não pode estar vazia.");

        PedidoEntity pedido;
        try
        {
            pedido = ObterPedidoComValidacao(pedidoId);
        }
        catch (PedidoException ex)
        {
            return NotFound(ex.Message);
        }

        foreach (var produtoDto in produtosDto)
        {
            ProdutoEntity produto;
            try
            {
                produto = ObterProdutoComValidacao(produtoDto.ProdutoId);
            }
            catch (PedidoException ex)
            {
                return NotFound(ex.Message);
            }

            // Verifique se o produto já está associado ao pedido
            var pedidoProdutoExistente = _context.PedidoProdutos
                .FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoDto.ProdutoId);

            if (pedidoProdutoExistente != null)
            {
                return BadRequest($"Produto com ID {produtoDto.ProdutoId} já existe no pedido e não pode ser duplicado.");
            }
            else
            {
                // Adicione uma nova associação se não existir
                var pedidoProduto = new PedidoProduto
                {
                    PedidoId = pedidoId,
                    ProdutoId = produtoDto.ProdutoId,
                    Quantidade = produtoDto.Quantidade
                };

                _context.PedidoProdutos.Add(pedidoProduto);
            }
        }

        if (pedido.Status == PedidoStatus.Fechado.ToString())
            return BadRequest($"Pedido com ID {pedidoId} está fechado e não pode ser modificado.");

        _context.SaveChanges();

        return Ok($"Produtos adicionados com sucesso ao pedido {pedidoId}.");
    }



    /// <summary>
    /// Remove completamente um produto de um pedido.
    /// </summary>
    /// <param name="pedidoId">ID do pedido.</param>
    /// <param name="produtoId">ID do produto a ser removido.</param>
    /// <returns>Mensagem de sucesso ou erro detalhada.</returns>
    /// <remarks>
    /// Este endpoint remove um produto específico de um pedido existente, considerando as seguintes regras:
    /// - O pedido deve existir.
    /// - O produto deve estar associado ao pedido.
    /// - Não é permitido remover o último produto de um pedido (o pedido não pode ficar sem produtos).
    /// - Não é permitido modificar pedidos com status "Fechado".
    /// 
    /// Exemplo de request:
    /// DELETE /api/pedido/1/produto/2
    /// 
    /// Regras de validação:
    /// - Retorna 404 se o pedido ou produto não for encontrado.
    /// - Retorna 400 se o pedido estiver fechado ou se a tentativa de remoção deixar o pedido sem produtos.
    /// </remarks>
    /// <response code="200">Produto removido com sucesso do pedido.</response>
    /// <response code="404">Pedido ou produto não encontrado.</response>
    /// <response code="400">Pedido está fechado ou tentativa de remoção deixaria o pedido sem produtos.</response>
    [HttpDelete("{pedidoId}/produto/{produtoId}")]
    [SwaggerOperation(
        Summary = "Remove um produto do pedido",
        Description = @"
            Remove completamente um produto de um pedido específico, com as seguintes regras de validação:
            - O pedido deve existir.
            - O produto deve estar associado ao pedido.
            - Não é permitido remover o último produto do pedido.
            - Não é permitido modificar pedidos com status 'Fechado'.
            "
    )]
    [SwaggerResponse(200, "Produto removido com sucesso do pedido.")]
    [SwaggerResponse(404, "Pedido ou produto não encontrado.")]
    [SwaggerResponse(400, "Pedido está fechado ou tentativa de remoção deixaria o pedido sem produtos.")]

    public IActionResult RemoverProduto(int pedidoId, int produtoId)
    {
        PedidoEntity pedido;
        try
        {
            pedido = ObterPedidoComValidacao(pedidoId);
        }
        catch (PedidoException ex)
        {
            return NotFound(ex.Message);
        }

        if (pedido.Status == PedidoStatus.Fechado.ToString())
            return BadRequest($"Pedido com ID {pedidoId} está fechado e não pode ser modificado.");

        var pedidoProduto = _context.PedidoProdutos
            .FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoId);

        if (pedidoProduto == null)
            return NotFound($"Produto com ID {produtoId} não encontrado no pedido {pedidoId}.");

        // Verificar se o pedido ficará sem produtos após a remoção
        var totalProdutosNoPedido = _context.PedidoProdutos.Count(pp => pp.PedidoId == pedidoId);
        if (totalProdutosNoPedido <= 1) // Último produto
            return BadRequest($"O pedido com ID {pedidoId} não pode ficar sem produtos.");

        _context.PedidoProdutos.Remove(pedidoProduto);
        _context.SaveChanges();

        return Ok($"Produto com ID {produtoId} removido completamente do pedido {pedidoId}.");
    }


    /// <summary>
    /// Atualiza a quantidade de um ou mais produtos existentes em um pedido.
    /// </summary>
    /// <param name="pedidoId">ID do pedido.</param>
    /// <param name="produtosDto">Lista de produtos com as novas quantidades a serem atualizadas no pedido.</param>
    /// <returns>Mensagem de sucesso ou erro detalhada.</returns>
    /// <remarks>
    /// Este endpoint permite atualizar a quantidade de produtos já associados a um pedido, considerando as seguintes regras:
    /// - O pedido deve existir.
    /// - O pedido não pode estar com o status "Fechado".
    /// - A lista de produtos no request não pode conter duplicatas (mesmo ID de produto).
    /// - A quantidade para cada produto deve ser maior que zero.
    /// - Todos os produtos no request devem existir no banco de dados.
    /// - O produto especificado deve estar associado ao pedido.
    /// 
    /// Exemplo de request body:
    /// {
    ///     "produtos": [
    ///         { "produtoId": 1, "quantidade": 5 },
    ///         { "produtoId": 2, "quantidade": 3 }
    ///     ]
    /// }
    /// </remarks>
    /// <response code="200">Produtos atualizados com sucesso no pedido.</response>
    /// <response code="400">
    /// Retornado em caso de:
    /// - Pedido fechado.
    /// - Produto duplicado no request.
    /// - Quantidade inválida.
    /// </response>
    /// <response code="404">
    /// Retornado em caso de:
    /// - Pedido não encontrado.
    /// - Produto não encontrado.
    /// - Produto não associado ao pedido.
    /// </response>
    [HttpPut("{pedidoId}/atualizar-produto")]
    [SwaggerOperation(
        Summary = "Atualiza a quantidade de um ou mais produtos no pedido",
        Description = @"
            Atualiza a quantidade de produtos existentes em um pedido, considerando as seguintes regras:
            - O pedido deve existir.
            - O pedido não pode estar com o status 'Fechado'.
            - A lista de produtos no request não pode conter duplicatas (mesmo ID de produto).
            - A quantidade para cada produto deve ser maior que zero.
            - Todos os produtos no request devem existir no banco de dados.
            - O produto especificado deve estar associado ao pedido.

            Exemplo de request:
            PUT /api/pedido/{pedidoId}/atualizar-produto
            Body:
            {
                ""produtos"": [
                    { ""produtoId"": 1, ""quantidade"": 5 },
                    { ""produtoId"": 2, ""quantidade"": 3 }
                ]
            }
            "
    )]
    [SwaggerResponse(200, "Produtos atualizados com sucesso no pedido.")]
    [SwaggerResponse(400, "Dados inválidos ou erro de validação.")]
    [SwaggerResponse(404, "Pedido ou produto não encontrado.")]
    public IActionResult AtualizarProduto(int pedidoId, [FromBody] List<ProdutoPedidoDto> produtosDto)
    {
        if (produtosDto == null || !produtosDto.Any())
            return BadRequest("A lista de produtos para atualização não pode estar vazia.");

        // Verifica se há duplicatas no request
        var duplicados = produtosDto
            .GroupBy(p => p.ProdutoId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicados.Any())
            return BadRequest($"Produtos duplicados no request: {string.Join(", ", duplicados)}.");

        PedidoEntity pedido;
        try
        {
            pedido = ObterPedidoComValidacao(pedidoId);
        }
        catch (PedidoException ex)
        {
            return NotFound(ex.Message);
        }

        // Verifica se o pedido está fechado
        if (pedido.Status == PedidoStatus.Fechado.ToString())
            return BadRequest($"Pedido com ID {pedidoId} está fechado e não pode ser modificado.");

        foreach (var produtoDto in produtosDto)
        {
            if (produtoDto.Quantidade <= 0)
                return BadRequest($"A quantidade para o produto com ID {produtoDto.ProdutoId} deve ser maior que zero.");

            ProdutoEntity produto;
            try
            {
                produto = ObterProdutoComValidacao(produtoDto.ProdutoId);
            }
            catch (PedidoException ex)
            {
                return NotFound(ex.Message);
            }

            var pedidoProduto = _context.PedidoProdutos
                .FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoDto.ProdutoId);

            if (pedidoProduto == null)
                return NotFound($"Produto com ID {produtoDto.ProdutoId} não encontrado no pedido com ID {pedidoId}.");

            // Atualiza a quantidade
            pedidoProduto.Quantidade = produtoDto.Quantidade;
        }

        _context.SaveChanges();
        return Ok($"Produtos atualizados com sucesso no pedido {pedidoId}.");
    }

    /// <summary>
    /// Fecha um pedido existente.
    /// </summary>
    /// <param name="pedidoId">ID do pedido a ser fechado.</param>
    /// <returns>Mensagem de sucesso ou erro detalhada.</returns>
    /// <remarks>
    /// Este endpoint permite fechar um pedido existente após validar que ele contém produtos, com as seguintes regras:
    /// - O pedido deve existir.
    /// - O pedido deve conter pelo menos um produto associado.
    /// - O pedido deve estar aberto para ser fechado (não pode estar com o status "Fechado").
    ///
    /// Exemplo de requisição:
    /// POST /api/pedido/{pedidoId}/fechar
    ///
    /// Possíveis respostas:
    /// - 200: Pedido fechado com sucesso.
    /// - 400: O pedido não contém produtos ou já está fechado.
    /// - 404: Pedido não encontrado.
    /// </remarks>
    /// <response code="200">Pedido fechado com sucesso.</response>
    /// <response code="400">
    /// Retornado em caso de:
    /// - Pedido sem produtos.
    /// - Pedido já fechado.
    /// </response>
    /// <response code="404">Pedido não encontrado.</response>
    [HttpPost("{pedidoId}/fechar")]
    [SwaggerOperation(
        Summary = "Fecha um pedido",
        Description = @"
            Fecha um pedido existente após validar as seguintes condições:
            - O pedido deve existir.
            - O pedido deve conter pelo menos um produto associado.
            - O pedido deve estar aberto (não pode estar com o status 'Fechado').

            Exemplo de requisição:
            POST /api/pedido/{pedidoId}/fechar
            "
    )]
    [SwaggerResponse(200, "Pedido fechado com sucesso.")]
    [SwaggerResponse(400, "Pedido sem produtos ou já fechado.")]
    [SwaggerResponse(404, "Pedido não encontrado.")]
    public IActionResult FecharPedido(int pedidoId)
    {
        PedidoEntity pedido;
        try
        {
            pedido = ObterPedidoComValidacao(pedidoId);
        }
        catch (PedidoException ex)
        {
            return NotFound(ex.Message);
        }

        if (!pedido.PedidoProdutos.Any())
            throw new PedidoException($"Pedido com ID {pedidoId} não pode ser fechado sem produtos.");

        pedido.Status = PedidoStatus.Fechado.ToString();
        _context.SaveChanges();

        return Ok($"Pedido com ID {pedidoId} fechado com sucesso.");
    }

    /// <summary>
    /// Reabre um pedido fechado.
    /// </summary>
    /// <param name="pedidoId">ID do pedido a ser reaberto.</param>
    /// <response code="200">Pedido reaberto com sucesso.</response>
    /// <response code="400">O pedido já está aberto e não pode ser reaberto.</response>
    /// <response code="404">Pedido não encontrado para o ID fornecido.</response>
    [HttpPatch("{pedidoId}/reabrir")]
    [SwaggerOperation(
        Summary = "Reabre um pedido fechado",
        Description = @"
            Permite reabrir um pedido que foi fechado, alterando seu status para 'Aberto'.

            Regras de validação:
            - O pedido deve existir na base de dados.
            - O pedido deve estar com o status 'Fechado' para ser reaberto.

            Exemplo de requisição:
            PATCH /api/pedido/{pedidoId}/reabrir

            Possíveis respostas:
            - 200: Pedido reaberto com sucesso.
            - 400: Pedido já está aberto.
            - 404: Pedido não encontrado."
    )]
    [SwaggerResponse(200, "Pedido reaberto com sucesso.")]
    [SwaggerResponse(400, "O pedido já está aberto e não pode ser reaberto.")]
    [SwaggerResponse(404, "Pedido não encontrado.")]
    public IActionResult ReabrirPedido(int pedidoId)
    {
        var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);

        if (pedido == null)
        {
            return NotFound($"Pedido com ID {pedidoId} não encontrado.");
        }

        if (pedido.Status == PedidoStatus.Aberto.ToString())
        {
            return BadRequest($"Pedido com ID {pedidoId} já está aberto e não pode ser reaberto.");
        }

        if (pedido.Status != PedidoStatus.Fechado.ToString())
        {
            return BadRequest($"Somente pedidos com status 'Fechado' podem ser reabertos. Status atual: {pedido.Status}.");
        }

        pedido.Status = PedidoStatus.Aberto.ToString();
        _context.SaveChanges();

        return Ok($"Pedido com ID {pedidoId} reaberto com sucesso.");
    }


    /// <summary>
    /// Lista pedidos com paginação.
    /// </summary>
    /// <param name="page">Número da página. Deve ser maior ou igual a 1. Valor padrão: 1.</param>
    /// <param name="pageSize">Quantidade de itens por página. Deve ser maior ou igual a 1. Valor padrão: 10.</param>
    /// <returns>Uma lista paginada de pedidos com os produtos associados.</returns>
    /// <remarks>
    /// Este endpoint retorna uma lista de pedidos com suporte a paginação. 
    /// 
    /// Regras de validação:
    /// - O número da página (`page`) deve ser maior ou igual a 1.
    /// - O tamanho da página (`pageSize`) deve ser maior ou igual a 1.
    ///
    /// Exemplo de requisição:
    /// GET /api/pedido/listar?page=1&pageSize=10
    ///
    /// Exemplo de resposta:
    /// {
    ///   "totalItems": 35,
    ///   "totalPages": 4,
    ///   "currentPage": 1,
    ///   "items": [
    ///     {
    ///       "id": 1,
    ///       "dataCriacao": "2024-11-28T10:45:00Z",
    ///       "status": "Aberto",
    ///       "produtos": [
    ///         {
    ///           "produtoId": 1,
    ///           "nome": "Produto A",
    ///           "preco": 10.0,
    ///           "quantidade": 2
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    ///
    /// Possíveis respostas:
    /// - 200: Retorna a lista de pedidos paginados.
    /// - 400: Valores de `page` ou `pageSize` inválidos.
    /// </remarks>
    /// <response code="200">Lista de pedidos paginados retornada com sucesso.</response>
    /// <response code="400">Parâmetros de paginação inválidos (valores menores que 1).</response>
    [HttpGet("listar")]
    [SwaggerOperation(
        Summary = "Lista pedidos com paginação",
        Description = @"
            Retorna uma lista de pedidos com suporte a paginação.

            Regras de validação:
            - O número da página (`page`) deve ser maior ou igual a 1.
            - O tamanho da página (`pageSize`) deve ser maior ou igual a 1.

            Exemplo de requisição:
            GET /api/pedido/listar?page=1&pageSize=10"
    )]
    [SwaggerResponse(200, "Lista de pedidos paginados retornada com sucesso.", typeof(PaginacaoResponse<PedidoDto>))]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos.")]
    public IActionResult ListarPedidosPaginados([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Os valores de página e tamanho devem ser maiores que zero.");
        }

        var query = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto)
            .OrderBy(p => p.Id);

        var totalItems = query.Count();

        var pedidos = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .Select(p => new
            {
                p.Id,
                p.DataCriacao,
                Status = Enum.TryParse<PedidoStatus>(p.Status, out var status) ? status : PedidoStatus.Desconhecido,
                Produtos = p.PedidoProdutos.Select(pp => new
                {
                    pp.ProdutoId,
                    Nome = pp.Produto?.Nome ?? "Produto não especificado",
                    Preco = pp.Produto?.Preco ?? 0m,
                    pp.Quantidade
                })
            });

        var response = new PaginacaoResponse<object>
        {
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            CurrentPage = page,
            Items = pedidos
        };

        return Ok(response);
    }

    /// <summary>
    /// Lista pedidos paginados com base no status especificado.
    /// </summary>
    /// <param name="page">Número da página. Deve ser maior ou igual a 1. Valor padrão: 1.</param>
    /// <param name="pageSize">Quantidade de itens por página. Deve ser maior ou igual a 1. Valor padrão: 10.</param>
    /// <param name="status">Status do pedido. Valores aceitos: "Aberto" ou "Fechado". Caso não seja fornecido, retorna todos os pedidos.</param>
    /// <returns>Uma lista paginada de pedidos filtrados pelo status especificado, com os produtos associados.</returns>
    /// <remarks>
    /// Este endpoint retorna uma lista de pedidos filtrados pelo status especificado, com suporte a paginação.
    /// 
    /// Regras de validação:
    /// - O número da página (`page`) deve ser maior ou igual a 1.
    /// - O tamanho da página (`pageSize`) deve ser maior ou igual a 1.
    /// - O parâmetro `status` deve ser "Aberto" ou "Fechado". Se omitido, retorna todos os pedidos.
    ///
    /// Exemplo de requisição:
    /// GET /api/pedido/filtrar-status?page=1&pageSize=10&status=Aberto
    ///
    /// Exemplo de resposta:
    /// {
    ///   "totalItems": 20,
    ///   "totalPages": 2,
    ///   "currentPage": 1,
    ///   "items": [
    ///     {
    ///       "id": 1,
    ///       "dataCriacao": "2024-11-28T10:45:00Z",
    ///       "status": "Aberto",
    ///       "produtos": [
    ///         {
    ///           "produtoId": 1,
    ///           "nome": "Produto A",
    ///           "preco": 10.0,
    ///           "quantidade": 2
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    ///
    /// Possíveis respostas:
    /// - 200: Retorna a lista de pedidos paginados com sucesso.
    /// - 400: Parâmetros de entrada inválidos ou status fornecido inválido.
    /// </remarks>
    /// <response code="200">Lista de pedidos paginados retornada com sucesso.</response>
    /// <response code="400">Parâmetros de entrada inválidos ou status fornecido inválido.</response>
    [HttpGet("filtrar-status")]
    [SwaggerOperation(
        Summary = "Lista pedidos por status com paginação",
        Description = @"
            Retorna uma lista de pedidos com suporte a paginação, filtrando pelo status ('Aberto' ou 'Fechado').

            Regras de validação:
            - O número da página (`page`) deve ser maior ou igual a 1.
            - O tamanho da página (`pageSize`) deve ser maior ou igual a 1.
            - O parâmetro `status` deve existir no banco de dados. Se omitido, retorna todos os pedidos.

            Exemplo de requisição:
            GET /api/pedido/filtrar-status?page=1&pageSize=10&status=Aberto"
    )]
    [SwaggerResponse(200, "Lista de pedidos paginados retornada com sucesso.", typeof(PaginacaoResponse<object>))]
    [SwaggerResponse(400, "Parâmetros de entrada inválidos ou status fornecido inválido.")]
    public IActionResult ListarPedidosPaginadosEPorStatus([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Página e tamanho devem ser maiores que zero.");
        }

        // Cast explícito para IQueryable<Pedido> para evitar conflitos de tipo
        var query = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto)
            .AsQueryable(); // Adicionado .AsQueryable() para evitar conflitos de tipo

        // Filtrar por status se fornecido
        if (status != null)
        {
            if (Enum.TryParse<PedidoStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(p => p.Status.Equals(parsedStatus.ToString(), StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return BadRequest("Status deve ser um dos valores definidos no enumerador PedidoStatus.");
            }
        }

        var totalItems = query.Count();
        var pedidos = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .Select(p => new
            {
                p.Id,
                p.DataCriacao,
                Status = Enum.TryParse<PedidoStatus>(p.Status, out var pedidoStatus) ? pedidoStatus : PedidoStatus.Desconhecido,
                Produtos = p.PedidoProdutos.Select(pp => new
                {
                    pp.ProdutoId,
                    Nome = pp.Produto?.Nome ?? "Produto não especificado",
                    Preco = pp.Produto?.Preco ?? 0m,
                    pp.Quantidade
                })
            });

        var response = new PaginacaoResponse<object>
        {
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            CurrentPage = page,
            Items = pedidos
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtém os detalhes de um pedido pelo ID.
    /// </summary>
    /// <param name="pedidoId">ID do pedido a ser detalhado. Deve ser um número válido.</param>
    /// <returns>Os detalhes do pedido solicitado, incluindo informações dos produtos associados.</returns>
    /// <remarks>
    /// Este endpoint retorna os detalhes de um pedido específico, incluindo sua data de criação, status e lista de produtos associados.
    /// 
    /// Regras de validação:
    /// - O `pedidoId` deve corresponder a um pedido existente no sistema.
    /// - Caso o pedido não seja encontrado, retorna um código HTTP 404.
    /// 
    /// Exemplo de requisição:
    /// GET /api/pedido/1/detalhar
    ///
    /// Exemplo de resposta:
    /// {
    ///   "id": 1,
    ///   "dataCriacao": "2024-11-28T10:45:00Z",
    ///   "status": "Aberto",
    ///   "produtos": [
    ///     {
    ///       "produtoId": 1,
    ///       "nome": "Produto A",
    ///       "preco": 10.0,
    ///       "quantidade": 2
    ///     },
    ///     {
    ///       "produtoId": 2,
    ///       "nome": "Produto B",
    ///       "preco": 20.0,
    ///       "quantidade": 1
    ///     }
    ///   ]
    /// }
    ///
    /// Possíveis respostas:
    /// - 200: Pedido detalhado com sucesso.
    /// - 404: Pedido não encontrado para o ID fornecido.
    /// </remarks>
    /// <response code="200">Pedido detalhado com sucesso.</response>
    /// <response code="404">Pedido não encontrado para o ID fornecido.</response>
    [HttpGet("{pedidoId}/detalhar")]
    [SwaggerOperation(
        Summary = "Detalha um pedido",
        Description = @"
        Retorna os dados de um pedido específico, incluindo seus produtos.

        Regras de validação:
        - O `pedidoId` deve corresponder a um pedido existente no sistema.
        - Caso o pedido não seja encontrado, retorna um código HTTP 404.

        Exemplo de requisição:
        GET /api/pedido/{pedidoId}/detalhar"
    )]
    [SwaggerResponse(200, "Pedido detalhado com sucesso.", typeof(PedidoDto))]
    [SwaggerResponse(404, "Pedido não encontrado.")]
    public IActionResult DetalharPedido(int pedidoId)
    {
        var pedido = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto)
            .FirstOrDefault(p => p.Id == pedidoId);

        if (pedido == null)
        {
            return NotFound($"Pedido com ID {pedidoId} não encontrado.");
        }

        var pedidoDto = new PedidoDto
        {
            Id = pedido.Id,
            DataCriacao = pedido.DataCriacao,
            Status = Enum.TryParse<PedidoStatus>(pedido.Status, out var status) ? status : PedidoStatus.Desconhecido,
            Produtos = pedido.PedidoProdutos.Select(pp => new PedidoProdutoDto
            {
                ProdutoId = pp.ProdutoId,
                Nome = pp.Produto.Nome,
                Preco = pp.Produto.Preco,
                Quantidade = pp.Quantidade
            }).ToList()
        };

        return Ok(pedidoDto);
    }

}