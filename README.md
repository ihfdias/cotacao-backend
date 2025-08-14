# API de Cotação de Moedas (.NET)

API desenvolvida em .NET 6 como parte de um desafio técnico para vaga de desenvolvedor júnior. A principal função desta API é consultar os dados de cotação do Dólar em tempo real, disponibilizados pelo Banco Central do Brasil, e servi-los através de um endpoint seguro.

## ✨ Funcionalidades

- **Endpoint de Cotação:** Fornece a cotação atual de compra e venda do Dólar.
- **Segurança:** O endpoint é protegido por um token de autorização (`baseToken`) que deve ser enviado via header na requisição.
- **CORS:** Configurado para permitir requisições do frontend da aplicação.
- **Documentação Interativa:** Interface do Swagger para visualizar e testar os endpoints da API.

## 🚀 Tecnologias Utilizadas

- **.NET 6** (usando o padrão de APIs Mínimas)
- **C#**
- **ASP.NET Core**
- **Swashbuckle.AspNetCore** para geração da documentação OpenAPI (Swagger).

## 🔧 Como Rodar o Projeto Localmente

### Pré-requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/pt-br/download/dotnet/6.0) instalado.
- Um editor de código como [VS Code](https://code.visualstudio.com/).

### Instalação e Execução

1.  **Clone o repositório:**
    ```bash
    git clone [https://github.com/SEU_USUARIO/ApiDefinitiva.git](https://github.com/SEU_USUARIO/ApiDefinitiva.git)
    ```
2.  **Navegue até a pasta do projeto:**
    ```bash
    cd ApiDefinitiva
    ```
3.  **Configure o Token Secreto:**
    - No arquivo `appsettings.json`, defina um valor para a chave `"SecretToken"`.
4.  **Execute a aplicação:**
    ```bash
    dotnet watch run
    ```
    A API estará rodando no endereço informado no terminal (ex: `http://localhost:5053`).

## 🔑 Endpoint da API

### `GET /`

- **Descrição:** Retorna a cotação mais recente do Dólar.
- **Header Obrigatório:** `baseToken: <seu-token-configurado-no-appsettings>`
- **Resposta de Sucesso (200 OK):**
  ```json
  {
    "value": [
      {
        "cotacaoCompra": 5.40460,
        "cotacaoVenda": 5.40520,
        "dataHoraCotacao": "2025-08-13 13:10:27.976"
      }
    ]
  }
