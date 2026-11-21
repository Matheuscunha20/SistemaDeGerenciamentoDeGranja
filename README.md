# AgroArca

> *Cuide do seu rebanho como um bom pastor.*

[![CI](https://github.com/Matheuscunha20/SistemaDeGerenciamentoDeGranja/actions/workflows/ci.yml/badge.svg)](https://github.com/Matheuscunha20/SistemaDeGerenciamentoDeGranja/actions/workflows/ci.yml)

Sistema web de gestão de propriedades rurais para qualquer criação (bovinos, suínos, aves, ovinos, caprinos, equinos e bubalinos).
Controla o rebanho, a sanidade, a ocupação das instalações, o estoque de alimento e a equipe, com um painel que mostra o que precisa de atenção.

Desenvolvido em **C# / ASP.NET Core 8 MVC**, **Entity Framework Core** e **MySQL**, com arquitetura em camadas e testes automatizados.

---

## Funcionalidades

**Painel**
- Alertas automáticos: vacinas atrasadas, alimento sem estoque ou com estoque baixo, galpões lotados e animais em tratamento
- Ocupação total dos galpões, animais por espécie e estado de saúde do rebanho

**Rebanho**
- Animais com espécie, raça, sexo, idade calculada, peso, galpão e lote
- 7 espécies e cerca de 55 raças comuns já cadastradas; a lista de raças muda conforme a espécie escolhida
- Busca e filtros por espécie, galpão e estado de saúde
- Galpões com controle de capacidade: barra de ocupação e bloqueio de galpão lotado
- Lotes para manejo: adicionar vários animais de uma vez, peso total e médio, encerrar e reabrir

**Sanidade**
- Vacinas com intervalo de reforço e cálculo automático da próxima dose
- Agenda de doses atrasadas e dos próximos dias, com "aplicar dose" em um clique
- Vacinação em grupo (mesma vacina para vários animais, filtrando por espécie, galpão ou lote)
- Ocorrências de saúde (doença, ferimento, tratamento, exame, parto), atualizando o estado do animal

**Estoque de alimento**
- Entradas (compra, ajuste) e saídas (consumo, perda, ajuste) com responsável
- Saldo atual, estoque mínimo, consumo médio diário e previsão de quantos dias o estoque dura
- Gasto com compras e último preço pago
- Não permite estoque negativo

**Equipe**
- Funcionários com validação de CPF (dígitos verificadores), cargos e tempo de casa
- Desligamento e reativação mantendo o histórico

**Financeiro**
- Receitas e despesas com categoria, forma de pagamento e lote (opcional)
- Resumo do período (receitas, despesas e saldo) e totais por categoria
- Períodos prontos (este mês, mês passado, 30 dias, ano) e filtros
- Categorias padrão de fazenda, editáveis
- Contas a pagar e a receber, parcelas mensais e fluxo de caixa com previsão
- Compra no estoque vira despesa; venda de animais (um a um ou do lote) vira receita
- Resultados: recebido x pago dos últimos meses, para onde foi o dinheiro e lucro por lote

**Acesso e equipe da fazenda**
- Cada cliente (fazenda) vê apenas os próprios dados
- Três perfis: **Dono** (tudo, inclusive equipe), **Gerente** (opera tudo, menos a equipe) e **Funcionário** (dia a dia, sem excluir e sem alterar cadastros)
- Convite por link (WhatsApp ou como preferir), válido por 7 dias
- O dono muda perfis, remove e reativa acessos e renomeia a fazenda

**Geral**
- Login e cadastro em português, com bloqueio temporário após várias senhas erradas
- Datas e números no padrão brasileiro
- Proteções de exclusão: nada que tenha histórico é apagado por engano

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Web | ASP.NET Core 8 MVC, Razor Pages (Identity), Bootstrap 5 |
| Dados | Entity Framework Core 8, Pomelo (MySQL), migrations |
| Banco | MySQL 8 no Docker |
| Autenticação | ASP.NET Core Identity |
| Testes | xUnit, EF Core InMemory |
| CI | GitHub Actions |

---

## Arquitetura

```
SistemaGranja.Domain     Entidades e regras de negócio (Animal, Galpao, Lote, Vacina, Estoque...)
SistemaGranja.Data       AppDbContext, migrations e dados iniciais (espécies, raças, vacinas)
SistemaGranja.Services   Regras de aplicação: saldos, agenda de vacinas, ocupação, painel
SistemaGranja.Web        Controllers, telas (Razor) e configuração
SistemaGranja.Tests      Testes de domínio e de serviços
```

O fluxo é sempre **Controller → Service → AppDbContext**. Os controllers cuidam da tela e da validação do formulário; as regras ficam nos serviços, o que permite testá-las sem subir a aplicação.

---

## Como rodar

**Pré-requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) e [Docker Desktop](https://www.docker.com/products/docker-desktop/).

**1. Clonar o repositório**
```bash
git clone https://github.com/Matheuscunha20/SistemaDeGerenciamentoDeGranja.git
cd SistemaDeGerenciamentoDeGranja
```

**2. Definir a senha do banco**

Copie o arquivo de exemplo e troque a senha dentro dele:
```bash
cp .env.example .env        # no PowerShell: copy .env.example .env
```

**3. Subir o MySQL**
```bash
docker compose up -d
```
O banco fica na porta **3307** para não conflitar com outro MySQL instalado.

**4. Informar a conexão ao sistema** (use a mesma senha do `.env`)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3307;Database=SistemaGranjaDB;Uid=root;Pwd=SUA_SENHA;" --project SistemaGranja.Web
```
A senha fica no *User Secrets* da sua máquina, fora do código.

**5. Rodar**
```bash
dotnet run --project SistemaGranja.Web
```
Ou abra o `SistemaGranjaSol.sln` no Visual Studio e aperte **F5**.

Na primeira execução, o sistema cria as tabelas e cadastra espécies, raças e vacinas comuns. Depois é só **criar uma conta** e entrar.

---

## Colocar no ar (produção)

O sistema roda em container Docker (`Dockerfile`). Para subir tudo (sistema + MySQL) num servidor:

```bash
cp .env.example .env        # e troque as senhas
docker compose -f docker-compose.producao.yml up -d --build
```

O sistema fica na porta **8080**. O HTTPS é feito por um proxy na frente (Caddy, Nginx ou a própria plataforma de hospedagem).

| Variável de ambiente | Para que serve |
|---|---|
| `ConnectionStrings__DefaultConnection` | Conexão com o MySQL (obrigatória) |
| `Email__Servidor`, `Email__Porta`, `Email__Usuario`, `Email__Senha`, `Email__Remetente` | Envio de e-mails (confirmação, esqueci a senha, convite) |
| `DataProtection__Pasta` | Pasta das chaves de login (já vem `/app/chaves` no container; monte um volume) |
| `Proxy__ConfiarEmCabecalhos` | `true` atrás de proxy que cuida do HTTPS |
| `Banco__AplicarMigracoesAoIniciar` | `true` (padrão): atualiza o banco ao iniciar |

- **`/saude`**: responde `200` quando o sistema e o banco estão bem (usado pela hospedagem e pelo monitoramento).
- **Logs**: uma linha por evento, com data e hora, na saída do container (`docker compose logs app`).
- **Segurança**: cookies só por HTTPS, cabeçalhos de proteção, limite de 10 tentativas por minuto no login e no cadastro.
- O GitHub Actions monta a imagem e sobe o sistema com MySQL a cada mudança, conferindo se as páginas abrem.

## Testes

```bash
dotnet test
```

Os testes rodam com um banco em memória, sem precisar do MySQL. Eles cobrem:
- Validação de CPF, idade dos animais e tempo de casa dos funcionários
- Cálculo de saldo, consumo médio e estoque baixo
- Agenda de vacinas (dose atrasada some quando o reforço é aplicado)
- Ocupação de galpões, movimentação de lotes e proteções de exclusão
- Dados iniciais que não se duplicam

A cada push e pull request, o GitHub Actions compila o projeto e roda todos os testes.

---

## Autor

**Matheus Pereira da Silva Cunha**
Estudante de Engenharia de Computação (UNIVESP)

[LinkedIn](https://linkedin.com/in/matheus-cunha-dev) · [GitHub](https://github.com/Matheuscunha20)
