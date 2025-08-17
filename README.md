# 🌐 API de Cotação de Moedas (.NET com JWT e Cache)

API desenvolvida em **.NET 6** como parte de um desafio técnico para vaga de desenvolvedor júnior.  
A principal função desta API é consultar os dados de **cotação do Dólar em tempo real**, disponibilizados pelo **Banco Central do Brasil**, e servi-los de forma **segura e otimizada**.

---

## ✨ Funcionalidades

- **Autenticação JWT**  
  Implementa um sistema de segurança robusto e padrão de mercado com JSON Web Tokens.

- **Endpoint de Login (`POST /login`)**  
  Rota pública para autenticação e geração de tokens JWT com validade de **2 horas**.

- **Endpoint de Cotação (`GET /`)**  
  Rota privada que fornece a **cotação atual do Dólar**, acessível apenas com um **token JWT válido**.

- **Otimização com Caching**  
  Utiliza `IMemoryCache` para armazenar a resposta da API do Banco Central, reduzindo drasticamente a latência e o número de chamadas externas.

- **CORS**  
  Configurado para permitir requisições exclusivamente do **frontend da aplicação**.

- **Documentação Interativa**  
  Interface do **Swagger** configurada para suportar o fluxo de autenticação com **Bearer Token**.

---

## 🚀 Tecnologias Utilizadas

- **.NET 6** (padrão de **APIs Mínimas**)  
- **C#**  
- **ASP.NET Core**  
- **Autenticação JWT (JSON Web Token)**  
- **.NET Secret Manager** para armazenamento seguro de chaves em desenvolvimento  
- **Swashbuckle.AspNetCore (Swagger)**  
- **Docker** para containerização e deploy  

---

## 🔧 Como Rodar o Projeto Localmente

### ✅ Pré-requisitos
- **.NET 6 SDK** instalado  
- Um editor de código como **VS Code** com a extensão **C# Dev Kit**

### ⚡ Instalação e Execução

1. **Clone o repositório**  
   ```bash
   git clone https://github.com/ihfdias/cotacao-backend.git
