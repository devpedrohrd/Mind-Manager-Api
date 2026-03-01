<div align="center">

# 🧠 Mind Manager

**Sistema de Gerenciamento de Consultas Psicológicas para Instituições de Ensino**

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![License](https://img.shields.io/badge/Licença-MIT-green?style=for-the-badge)

<br/>

[Funcionalidades](#-funcionalidades) •
[Tecnologias](#-tecnologias) •
[Arquitetura](#-arquitetura) •
[Como Executar](#-como-executar) •
[Endpoints](#-endpoints-da-api) •
[Contribuição](#-contribuição)

</div>

---

## 📖 Sobre o Projeto

O **Mind Manager** é uma API REST desenvolvida para auxiliar o setor de psicologia de instituições de ensino no gerenciamento completo do atendimento psicológico. O sistema oferece uma plataforma centralizada para que psicólogos possam acompanhar seus pacientes — alunos, contratados, professores e responsáveis — de forma organizada e segura.

### 🎯 Objetivos

- **Centralizar o atendimento psicológico**: Reunir em um único sistema todas as informações relevantes sobre pacientes, consultas, sessões e anamneses clínicas.
- **Facilitar o agendamento**: Permitir que psicólogos agendem, acompanhem e gerenciem consultas com praticidade, incluindo sessões individuais, atividades coletivas e registros administrativos.
- **Garantir o registro clínico**: Oferecer ferramentas para registro detalhado de sessões terapêuticas (queixas, intervenções, encaminhamentos) e anamneses completas (histórico familiar, infância, adolescência, doenças e acompanhamentos).
- **Controlar o acesso por perfis**: Assegurar que apenas profissionais autorizados tenham acesso a dados sensíveis, com controle baseado em papéis (Administrador, Psicólogo, Paciente).
- **Apoiar a tomada de decisão**: Disponibilizar buscas com filtros avançados e paginação para análise de dados de atendimentos por período, status e tipo.

### 🏥 Finalidades

| Finalidade | Descrição |
|---|---|
| **Gestão de Pacientes** | Cadastro e acompanhamento de perfis de pacientes com informações acadêmicas, pessoais e clínicas (tipo de paciente, curso, turno, dificuldades, transtornos). |
| **Agendamento de Consultas** | Criação e gerenciamento de agendamentos com controle de status (pendente, confirmado, finalizado, cancelado, ausência) e tipos de atendimento. |
| **Registro de Sessões** | Documentação detalhada de cada sessão terapêutica, vinculada ao agendamento, com campos para queixas, intervenções e encaminhamentos. |
| **Anamnese Clínica** | Formulário completo de anamnese com seções para histórico familiar, desenvolvimento na infância e adolescência, histórico de doenças e acompanhamentos anteriores. |
| **Notificações por E-mail** | Sistema de envio de e-mails para recuperação de senha e lembretes de consultas via SMTP. |
| **Segurança e Privacidade** | Autenticação JWT com tokens de acesso e refresh, hash de senhas com BCrypt e controle granular de permissões. |

---

## ✨ Funcionalidades

### 🔐 Autenticação e Segurança
- Registro e login com tokens **JWT** (access token de 15min + refresh token de 7 dias)
- Hash de senhas com **BCrypt** (cost factor 12)
- Recuperação de senha por **e-mail** com token temporário (1h de validade)
- **3 níveis de acesso**: Administrador, Psicólogo e Paciente
- Políticas de autorização: `AdminOnly`, `PsychologistOrAdmin`, `AllUsers`
- Validação de ownership de recursos (usuários só acessam seus próprios dados)

### 👤 Gestão de Usuários e Perfis
- CRUD completo de usuários com busca por filtros e paginação
- Perfis de **Psicólogo** com CRP (registro profissional) e especialidades
- Perfis de **Paciente** com informações acadêmicas (curso, turno, turma) e clínicas (dificuldades, transtornos psicológicos)
- Tipos de paciente: Aluno, Contratado, Responsável, Professor

### 📅 Agendamentos
- Criação, atualização e exclusão de agendamentos
- Tipos: Sessão Individual, Atividade Coletiva, Registro Administrativo
- Status: Pendente, Confirmado, Finalizado, Cancelado, Ausência
- Busca por período: hoje, semana, mês
- Filtros avançados com paginação

### 📝 Sessões Terapêuticas
- Registro de sessões vinculadas a agendamentos
- Campos: queixa, intervenção, encaminhamento, evolução
- Busca com filtros por paciente, psicólogo e período

### 🏥 Anamnese Clínica
- Formulário completo com múltiplas seções
- Histórico familiar, infância, adolescência
- Registro de doenças e acompanhamentos anteriores
- Vinculação ao perfil do paciente

### 📧 Sistema de E-mails
- Envio via SMTP com **MailKit**
- Templates HTML customizados
- Recuperação de senha por e-mail
- Infraestrutura para lembretes de consulta

---

## 🛠 Tecnologias

| Tecnologia | Versão | Utilização |
|---|---|---|
| **.NET** | 9.0 | Framework principal |
| **ASP.NET Core** | 9.0 | API REST |
| **Entity Framework Core** | 9.0.0 | ORM e migrações |
| **PostgreSQL** (Npgsql) | 9.0.0 | Banco de dados relacional |
| **JWT Bearer** | 9.0.0 | Autenticação por tokens |
| **BCrypt.Net-Next** | 4.0.3 | Hash de senhas |
| **FluentValidation** | 11.9.1 | Validação de dados |
| **MailKit** | 4.15.0 | Envio de e-mails SMTP |
| **Serilog** | 8.0.1 | Logging estruturado |
| **Swashbuckle** | 6.4.7 | Documentação Swagger/OpenAPI |
| **DotNetEnv** | 3.1.1 | Variáveis de ambiente |
| **Docker** | Multi-stage | Containerização |

---

## 🏛 Arquitetura

O projeto segue uma **arquitetura em camadas** com princípios de **Domain-Driven Design (DDD)**:

```
src/
├── 🌐 Api/                  → Camada de Apresentação
│   ├── Controllers/          → Endpoints REST
│   └── Middlewares/          → Tratamento global de exceções
│
├── ⚙️ Application/           → Camada de Aplicação
│   ├── Authorization/        → Serviços de autorização
│   ├── Mappers/              → Mapeamento entre entidades e DTOs
│   └── Services/             → Regras de negócio e orquestração
│
├── 🏗️ Domain/                → Camada de Domínio
│   ├── Entities/             → Entidades do negócio
│   ├── DTO/                  → Records imutáveis (Commands, Queries, Responses)
│   ├── Interfaces/           → Contratos (repositórios, serviços)
│   ├── Validators/           → Validações de domínio
│   ├── ValueObjects/         → Objetos de valor (Email, Cpf)
│   └── Exceptions/           → Exceções de domínio tipadas
│
└── 🗄️ Infrastructure/        → Camada de Infraestrutura
    ├── Persistence/          → DbContext e configurações EF Core
    ├── Repository/           → Implementação dos repositórios
    ├── Specifications/       → Padrão Specification (filtros e paginação)
    └── UnitOfWork/           → Padrão Unit of Work (transações)
```

### 📐 Padrões de Projeto Utilizados

| Padrão | Descrição |
|---|---|
| **Repository** | Abstração do acesso a dados com repositórios específicos por entidade |
| **Unit of Work** | Gerenciamento de transações agrupando operações de múltiplos repositórios |
| **Specification** | Encapsulamento de filtros, ordenação e paginação em objetos reutilizáveis |
| **Value Objects** | Objetos imutáveis com validação embutida (`Email`, `Cpf`) |
| **Domain Exceptions** | Exceções tipadas (`NotFoundException`, `BusinessException`, `ValidationException`, `ForbiddenException`) |
| **Chain of Responsibility** | Tratamento global de exceções com handlers encadeados por tipo |
| **DTO Records** | Records imutáveis para Commands, Queries e Responses |

### 🗂️ Diagrama de Entidades

```
User (1) ──────── (0..1) PsychologistProfile
  │                           │
  │                           ├──── (1:N) Appointments
  │                           └──── (1:N) Sessions
  │
  └──────── (0..1) PatientProfile
                        │
                        ├──── (1:N) Appointments
                        ├──── (1:N) Sessions
                        └──── (1:N) Anamneses

Appointment (1) ──── (0..1) Session
      │
      └──── (1:N) EmailSchedules
```

---

## 🚀 Como Executar

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (ou Docker)
- Conta de e-mail SMTP (Gmail ou outro) para funcionalidades de e-mail

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/mind-manager.git
cd mind-manager
```

### 2. Configure as variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto com base no template:

```env
# Banco de Dados
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=mind_manager;Username=postgres;Password=sua_senha

# JWT
JWT_SECRET_KEY=sua-chave-secreta-com-pelo-menos-32-caracteres
JWT_ISSUER=MindManager
JWT_AUDIENCE=MindManagerUsers

# E-mail (SMTP)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=seu-email@gmail.com
SMTP_PASSWORD=sua-senha-de-app
SMTP_FROM=seu-email@gmail.com
```

### 3. Execute as migrações

```bash
dotnet ef database update
```

### 4. Execute o projeto

```bash
dotnet run
```

A API estará disponível em `http://localhost:5000` e a documentação Swagger em `http://localhost:5000/swagger`.

### 🐳 Executando com Docker

```bash
# Build da imagem
docker build -t mind-manager .

# Executar o container
docker run -d \
  -p 8080:8080 \
  -e DB_CONNECTION_STRING="Host=host.docker.internal;Port=5432;Database=mind_manager;Username=postgres;Password=sua_senha" \
  -e JWT_SECRET_KEY="sua-chave-secreta-com-pelo-menos-32-caracteres" \
  -e JWT_ISSUER="MindManager" \
  -e JWT_AUDIENCE="MindManagerUsers" \
  --name mind-manager \
  mind-manager
```

---

## 📡 Endpoints da API

### 🔑 Autenticação (`/api`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/register` | Público | Criar conta de paciente |
| `POST` | `/login` | Público | Login (retorna access + refresh token) |
| `POST` | `/refresh` | Autenticado | Renovar tokens |
| `POST` | `/forgot-password` | Público | Solicitar reset de senha por e-mail |
| `POST` | `/reset-password` | Público | Redefinir senha com token |

### 👥 Usuários (`/api/users`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/` | Todos | Buscar usuários com filtros e paginação |
| `GET` | `/{id}` | Todos | Buscar por ID |
| `PUT` | `/{id}` | Todos | Atualizar usuário |
| `DELETE` | `/{id}` | Todos | Excluir usuário |

### 🪪 Perfis (`/api/profile`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/patient` | Psicólogo, Admin | Criar perfil de paciente |
| `PATCH` | `/patient/{id}` | Psicólogo, Admin | Atualizar perfil de paciente |
| `GET` | `/patients/search` | Psicólogo, Admin, Paciente | Buscar perfis de pacientes |
| `GET` | `/` | Psicólogo, Paciente | Buscar próprio perfil |
| `POST` | `/psychologist` | Psicólogo, Admin | Criar perfil de psicólogo |
| `PATCH` | `/psychologist/{id}` | Psicólogo, Admin | Atualizar perfil de psicólogo |

### 📅 Agendamentos (`/api/appointments`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/search` | Admin, Psicólogo | Buscar com filtros e paginação |
| `GET` | `/period/{period}` | Admin, Psicólogo | Buscar por período (today/week/month) |
| `GET` | `/my` | Psicólogo, Paciente | Agendamentos do usuário logado |
| `GET` | `/{id}` | Todos | Buscar por ID |
| `POST` | `/` | Admin, Psicólogo | Criar agendamento |
| `PATCH` | `/{id}` | Admin, Psicólogo | Atualizar agendamento |
| `DELETE` | `/{id}` | Admin, Psicólogo | Excluir agendamento |

### 📝 Sessões (`/api/sessions`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/` | Admin, Psicólogo | Criar sessão |
| `GET` | `/{id}` | Todos | Buscar por ID |
| `GET` | `/search` | Todos | Buscar com filtros |
| `PATCH` | `/{id}` | Admin, Psicólogo | Atualizar sessão |
| `DELETE` | `/{id}` | Admin, Psicólogo | Excluir sessão |

### 🏥 Anamnese (`/api/anamnesis`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/` | Psicólogo | Criar anamnese |
| `GET` | `/{id}` | Psicólogo | Buscar por ID |
| `PATCH` | `/{id}` | Psicólogo | Atualizar anamnese |
| `DELETE` | `/{id}` | Psicólogo | Excluir anamnese |

---

## 📁 Estrutura de Pastas

```
Mind-Manager/
├── 📄 Program.cs                    → Configuração e inicialização da aplicação
├── 📄 Mind-Manager.csproj           → Dependências e configuração do projeto
├── 📄 Mind-Manager.sln              → Arquivo de solução
├── 📄 Dockerfile                    → Build multi-stage para containerização
├── 📄 appsettings.json              → Configurações da aplicação
├── 📄 appsettings.template.json     → Template de configuração
├── 📁 Migrations/                   → Migrações do Entity Framework
├── 📁 Properties/                   → Configurações de lançamento
└── 📁 src/
    ├── 📁 Api/
    │   ├── 📁 Controllers/          → 6 controllers REST
    │   └── 📁 Middlewares/          → Exception handler global
    ├── 📁 Application/
    │   ├── 📁 Authorization/        → Serviço de autorização customizado
    │   ├── 📁 Mappers/              → Mapeadores de entidade ↔ DTO
    │   └── 📁 Services/             → Serviços de aplicação
    ├── 📁 Domain/
    │   ├── 📁 DTO/                  → Commands, Queries e Responses (records)
    │   ├── 📁 Entities/             → User, Profiles, Appointment, Session, Anamnesis
    │   ├── 📁 Exceptions/           → Exceções de domínio tipadas
    │   ├── 📁 Interfaces/           → Contratos de repositórios e serviços
    │   ├── 📁 Validators/           → Validações de domínio
    │   └── 📁 ValueObjects/         → Email, Cpf
    └── 📁 Infrastructure/
        ├── 📁 Persistence/          → ApplicationDbContext com Fluent API
        ├── 📁 Repository/           → Implementações dos repositórios
        ├── 📁 Specifications/       → Filtros e paginação reutilizáveis
        └── 📁 UnitOfWork/           → Gerenciamento de transações
```

---

## 🤝 Contribuição

Contribuições são bem-vindas! Para contribuir:

1. Faça um **fork** do projeto
2. Crie uma **branch** para sua feature (`git checkout -b feature/nova-feature`)
3. Faça **commit** das suas alterações (`git commit -m 'feat: adiciona nova feature'`)
4. Faça **push** para a branch (`git push origin feature/nova-feature`)
5. Abra um **Pull Request**

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">

Feito com ❤️ para apoiar o cuidado com a saúde mental nas instituições de ensino.

</div>
