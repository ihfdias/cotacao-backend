# API de Cotação de Moedas (.NET com JWT)

API desenvolvida em .NET 6 como parte de um desafio técnico para vaga de desenvolvedor júnior. A principal função desta API é consultar os dados de cotação do Dólar em tempo real, disponibilizados pelo Banco Central do Brasil, e servi-los através de endpoints protegidos por autenticação JWT (JSON Web Token).

## ✨ Funcionalidades

-   **Autenticação JWT:** Implementa um sistema de segurança robusto e padrão de mercado.
-   **Endpoint de Login (`POST /login`):** Uma rota pública para que clientes possam se autenticar (com credenciais fixas para o desafio) e receber um token JWT com validade de 2 horas.
-   **Endpoint de Cotação (`GET /`):** Uma rota privada que fornece a cotação atual de compra e venda do Dólar, acessível apenas com um token JWT válido no header `Authorization`.
-   **CORS:** Configurado para permitir requisições do frontend da aplicação (tanto em ambiente de desenvolvimento local quanto em produção).
-   **Documentação Interativa:** Interface do Swagger configurada para suportar o fluxo de autenticação com `Bearer Token`, permitindo testes completos da API.

## 🚀 Tecnologias Utilizadas

-   **.NET 6** (usando o padrão de APIs Mínimas)
-   **C#**
-   **ASP.NET Core**
-   **Autenticação JWT (JSON Web Token)**
-   **.NET Secret Manager** para armazenamento seguro de chaves em desenvolvimento.
-   **Swashbuckle.AspNetCore** para geração da documentação OpenAPI (Swagger).
-   **Docker** para containerização e deploy.

## 🔧 Como Rodar o Projeto Localmente

### Pré-requisitos

-   [.NET 6 SDK](https://dotnet.microsoft.com/pt-br/download/dotnet/6.0) instalado.
-   Um editor de código como [VS Code](https://code.visualstudio.com/) com a extensão C# Dev Kit.

### Instalação e Execução

1.  **Clone o repositório:**
    ```bash
    git clone [https://github.com/ihfdias/cotacao-backend.git](https://github.com/ihfdias/cotacao-backend.git)
    ```
2.  **Navegue até a pasta do projeto:**
    ```bash
    cd cotacao-backend
    ```
3.  **Configure os Segredos:**
    -   Ative o Secret Manager: `dotnet user-secrets init`
    -   Gere e adicione a chave secreta do JWT (substitua `SUA_CHAVE_SECRETA` por uma chave forte e aleatória):
        ```bash
        dotnet user-secrets set "Jwt:Key" "SUA_CHAVE_SECRETA"
        ```
4.  **Execute a aplicação:**
    ```bash
    dotnet run
    ```
    A API estará rodando no endereço informado no terminal (ex: `http://localhost:5053`). A documentação do Swagger estará disponível em `/swagger`.

## 🔑 Endpoints da API

### `POST /login`

-   **Descrição:** Autentica o usuário e retorna um token JWT.
-   **Corpo da Requisição:**
    ```json
    {
      "username": "admin",
      "password": "12345"
    }
    ```
-   **Resposta de Sucesso (200 OK):**
    ```json
    {
      "token": "ey..."
    }
    ```

### `GET /`

-   **Descrição:** Retorna a cotação mais recente do Dólar.
-   **Header Obrigatório:** `Authorization: Bearer <seu-token-jwt>`
-   **Resposta de Sucesso (200 OK):**
    ```json
    {
      "value": [
        {
          "cotacaoCompra": 5.40890,
          "cotacaoVenda": 5.40950,
          "dataHoraCotacao": "2025-08-14 13:08:26.774"
        }
      ]
    }
    ```