# API para Gestão de Pedidos

Este projeto é uma **API para Gestão de Pedidos**, construída em **.NET** com suporte a endpoints para manipulação de pedidos e produtos, além de integração com Swagger para documentação.

---

## Tecnologias Utilizadas

- **.NET**: Versão **9.0**
- **Entity Framework Core**: Para mapeamento de entidades e persistência.
- **xUnit**: Para testes automatizados.

---

## Como Configurar e Executar

1. **Clone o repositório para sua máquina local:**
   ```bash
   git clone https://github.com/t3chm4n/GestorPedidoAPI
   cd GestorPedidoAPI

2. **Verifique a versão do seu dotnet**
   ```bash
   dotnet --version

  - Certifique-se de ter o **.NET SDK 9.0** ou superior instalado.
  - Se não estiver instalado, baixe a versão necessária no site oficial: [Download .NET]([http://localhost:5196](https://dotnet.microsoft.com/download))

3. **Restaure as dependências**
   ```bash
   dotnet restore

4. **Compile o projeto**
   ```bash
   dotnet build

2. **Execute os testes (Opcional)**
   ```bash
   dotnet test

3. **Inicie o servidor**
   ```bash
   dotnet run --project WebAPI

## Endereços Importantes

- **Raiz da API**: [http://localhost:5196](http://localhost:5196)
- **Swagger (Documentação)**: [http://localhost:5196/swagger](http://localhost:5196/swagger)

---

## Funcionalidades Atuais

### Gestão de Pedidos

- **Criação**: Adicionar novos pedidos.
- **Edição**: Atualizar produtos dentro de um pedido (adição, atualização, exclusão).
- **Exclusão**: Exclui pedidos inteiros
- **Abertura/Fechamento**: Fechar ou reabrir pedidos.
- **Listagem**: Visualizar pedidos, com paginação.
- **Listagem por Status**: Visualizar pedidos por status, com paginação.
- **Detalhamento**: Obter informações detalhadas de um pedido específico.

### Gestão de Produtos

- **Criação**: Adicionar novos produtos ao sistema.
- **Exclusão**: Remover produtos (não associados a pedidos).

---

## To-Do Próximos Passos

1. **Endpoints para Pedidos**
   - Separar os endpoints em arquivos, para melhor organização (arquivo atual está com 900 linhas).

2. **Endpoints para Produtos**
   - Implementar edição e exclusão de produtos.
   - Melhorar regras de validação nos endpoints existentes.

3. **Documentação Externa**
   - Publicar documentação externa: já deixei os endpoints com comentários para geração de XML e HTML docs

---
