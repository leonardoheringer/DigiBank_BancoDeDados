using System;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Reflection.PortableExecutable;

class DigiBank
{
    private static string conexaoBancoDeDados = "Server=localhost;Database=DigiBank_teste;User ID=root;Password=Z5R2M9IQ;";

    public static void Main(string[] args)
    {
        TelaPrincipal();
    }

    public static void TelaPrincipal()
    {
        int opcao = 0;
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.White;

        Console.Clear();

        Console.WriteLine("                                                                    ");
        Console.WriteLine("                      Digite a Opção desejada :                     ");
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                          1 - Criar Conta                           ");
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                     2 - Entrar com CPF e Senha                     ");
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                              3 - Sair                              ");
        Console.WriteLine("               ======================================               ");
        opcao = int.Parse(Console.ReadLine());

        switch (opcao)
        {
            case 1:
                TelaCriarConta();
                break;
            case 2:
                TelaLogin();
                break;
            case 3:
                Console.WriteLine("Obrigado por usar o DigiBank!");
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Opção inválida");
                break;
        }
    }

    public static void TelaCriarConta()
    {
        Console.Clear();
        Console.WriteLine("                                                                     ");
        Console.WriteLine("                           Digite seu nome :                         ");
        string nome = Console.ReadLine();
        Console.WriteLine("               ======================================                ");
        Console.WriteLine("                             Digite o CPF                            ");
        string cpf = Console.ReadLine();
        Console.WriteLine("               ======================================                ");
        Console.WriteLine("                           Digite sua Senha                          ");
        string senha = Console.ReadLine();
        Console.WriteLine("               ======================================                ");

        // Gerar número da conta e número da agência aleatórios
        var (numeroConta, numeroAgencia) = GerarNumeroContaEAgencia();

        // Definir saldo inicial
        double saldoInicial = 0.0;

        // Cria a conta no banco de dados
        using (MySqlConnection connection = new MySqlConnection(conexaoBancoDeDados))
        {
            try
            {
                connection.Open();

                // Iniciar transação para garantir consistência dos dados
                using (var transaction = connection.BeginTransaction())
                {
                    // Inserir nova pessoa na tabela Pessoas
                    string queryPessoa = @"
                    INSERT INTO Pessoas (Nome, CPF, Senha) 
                    VALUES (@nome, @cpf, @senha)";
                    MySqlCommand commandPessoa = new MySqlCommand(queryPessoa, connection, transaction);
                    commandPessoa.Parameters.AddWithValue("@nome", nome);
                    commandPessoa.Parameters.AddWithValue("@cpf", cpf);
                    commandPessoa.Parameters.AddWithValue("@senha", senha);
                    int rowsPessoaAffected = commandPessoa.ExecuteNonQuery();

                    if (rowsPessoaAffected > 0)
                    {
                        // Obter o ID da pessoa recém-criada
                        string queryPessoaId = "SELECT LAST_INSERT_ID();";
                        MySqlCommand commandPessoaId = new MySqlCommand(queryPessoaId, connection, transaction);
                        int pessoaId = Convert.ToInt32(commandPessoaId.ExecuteScalar());

                        // Inserir nova conta na tabela Contas
                        string queryConta = @"
                        INSERT INTO Contas (NumeroConta, Agencia, Saldo, PessoaID) 
                        VALUES (@numeroConta, @Agencia, @saldo, @pessoaId)";
                        MySqlCommand commandConta = new MySqlCommand(queryConta, connection, transaction);
                        commandConta.Parameters.AddWithValue("@numeroConta", numeroConta);
                        commandConta.Parameters.AddWithValue("@Agencia", numeroAgencia);
                        commandConta.Parameters.AddWithValue("@saldo", saldoInicial);
                        commandConta.Parameters.AddWithValue("@pessoaId", pessoaId);
                        int rowsContaAffected = commandConta.ExecuteNonQuery();

                        if (rowsContaAffected > 0)
                        {
                            transaction.Commit(); // Confirma a transação
                            Console.WriteLine("Conta criada com sucesso!");
                        }
                        else
                        {
                            transaction.Rollback(); // Reverte a transação
                            Console.WriteLine("Erro ao criar a conta.");
                        }
                    }
                    else
                    {
                        transaction.Rollback(); // Reverte a transação
                        Console.WriteLine("Erro ao criar a pessoa no banco de dados.");
                    }
                }

                Thread.Sleep(1000);
                TelaPrincipal();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Erro ao tentar se conectar ao banco de dados. Verifique a conexão e tente novamente.");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado: {ex.Message}");
            }
        }
    }


    public static (string numeroConta, string numeroAgencia) GerarNumeroContaEAgencia()
    {
        Random random = new Random();

        // Gerar número da conta de 10 dígitos
        string numeroConta = random.Next(10000, 99999).ToString() + random.Next(10000, 99999).ToString();

        // Gerar número da agência de 4 dígitos
        string numeroAgencia = random.Next(1000, 9999).ToString();

        return (numeroConta, numeroAgencia);
    }

    public static void TelaLogin()
    {
        Console.Clear();
        Console.WriteLine("                                                                    ");
        Console.WriteLine("                            Digite o CPF:                           ");
        string cpf = Console.ReadLine();
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                         Digite sua senha:                          ");
        string senha = Console.ReadLine();
        Console.WriteLine("               ======================================               ");

        // Verificar no banco de dados
        using (MySqlConnection connection = new MySqlConnection(conexaoBancoDeDados))
        {
            try
            {
                connection.Open();
                string query = @"
                SELECT p.PessoaID, p.Nome, c.Saldo, c.NumeroConta, c.Agencia 
                FROM Pessoas p
                INNER JOIN Contas c ON p.PessoaID = c.PessoaID
                WHERE p.CPF = @cpf AND p.Senha = @senha";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@cpf", cpf.Trim());
                command.Parameters.AddWithValue("@senha", senha.Trim());

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        int pessoaId = reader.GetInt32("PessoaID");
                        string nome = reader.GetString("Nome");
                        double saldo = reader.GetDouble("Saldo");
                        string numeroConta = reader.GetString("NumeroConta");
                        string numeroAgencia = reader.GetString("Agencia");

                        Console.Clear();
                        Console.WriteLine($"Bem-vindo, {nome}!");
                        Console.WriteLine($"Conta: {numeroConta} | Agência: {numeroAgencia} | Saldo: {saldo:F2}");

                        // Chama a tela da conta logada
                        TelaContaLogada(pessoaId);
                    }
                    else
                    {
                        Console.WriteLine("Pessoa não encontrada ou senha incorreta.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }


    public static void TelaContaLogada(int pessoaId)
    {
        int opcao = 0;

        // Conectar ao banco de dados e obter as informações da pessoa e seu saldo
        using (MySqlConnection connection = new MySqlConnection(conexaoBancoDeDados))
        {
            try
            {
                connection.Open();

                // Consultar o nome da pessoa e saldo atual da conta com base no pessoaId
                string query = @"
                    SELECT 
                        p.Nome,
                        c.Saldo,
                        c.Agencia,  
                        c.NumeroConta
                    FROM 
                        Pessoas p
                    INNER JOIN 
                        Contas c ON p.PessoaID = c.PessoaID
                    WHERE 
                        p.PessoaID = @pessoaId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@pessoaId", pessoaId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();  // Lê a primeira linha retornada

                        string nome = reader.GetString("Nome");
                        double saldo = reader.GetDouble("Saldo");
                        string numeroConta = reader.GetString("NumeroConta");
                        string numeroAgencia = reader.GetString("Agencia");

                        // Exibe as informações da pessoa, saldo e a data/hora atual
                        Console.Clear();
                        Console.WriteLine($"        Banco: DigiBank | Agência: {numeroAgencia} | Conta: {numeroConta}");
                        Console.WriteLine($"     Seja bem vindo, {nome} | Saldo: {saldo:F2} | {DateTime.Now:dd/MM/yyyy HH:mm}");
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine("Pessoa não encontrada ou erro ao recuperar dados da conta.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                return;
            }
        }

        // Exibe as opções da conta
        Console.WriteLine("                                                                    ");
        Console.WriteLine("                      Digite a Opção desejada :                     ");
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                     1 - Realizar um Deposito                       ");
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                       2 - Realizar um Saque                        ");
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                            3 - Extrato                             ");
        Console.WriteLine("               ======================================               ");
        Console.WriteLine("                              4 - Sair                              ");
        Console.WriteLine("               ======================================               ");

        opcao = int.Parse(Console.ReadLine());
        switch (opcao)
        {
            case 1:
                TelaDeposito(pessoaId);
                break;
            case 2:
                TelaSaque(pessoaId);
                break;
            case 3:
                TelaExtrato(pessoaId);
                break;
            case 4:
                TelaPrincipal();
                break;
            default:
                Console.WriteLine("                            Opção Inválida.                         ");
                break;
        }
    }

    public static void TelaDeposito(int pessoaId)
    {
        Console.Clear();
        Console.WriteLine("                    Digite o valor do deposito:                      ");
        double valor = double.Parse(Console.ReadLine());
        Console.WriteLine("               ======================================                ");

        // Realizar depósito
        using (MySqlConnection connection = new MySqlConnection(conexaoBancoDeDados))
        {
            try
            {
                connection.Open();
                string query = "UPDATE Contas SET Saldo = Saldo + @valor WHERE PessoaID = @pessoaId";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@valor", valor);
                command.Parameters.AddWithValue("@pessoaId", pessoaId);
                command.ExecuteNonQuery();

                // Inserir no extrato
                query = "INSERT INTO Extrato (ContaID, TipoTransacao, Valor) " +
                        "SELECT ContaID, 'Deposito', @valor FROM Contas WHERE PessoaID = @pessoaId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@valor", valor);
                command.Parameters.AddWithValue("@pessoaId", pessoaId);
                command.ExecuteNonQuery();

                Console.Clear();
                Console.WriteLine("              =======================================               ");
                Console.WriteLine("                  Deposito Realizado com Sucesso                    ");
                Console.WriteLine("              =======================================               ");
                Console.WriteLine("                                                                    ");
                Thread.Sleep(1000);
                TelaContaLogada(pessoaId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }

    public static void TelaSaque(int pessoaId)
    {
        Console.Clear();
        Console.WriteLine("                     Digite o valor do Saque:                        ");
        double valor = double.Parse(Console.ReadLine());
        Console.WriteLine("               ======================================                ");

        // Realizar saque
        using (MySqlConnection connection = new MySqlConnection(conexaoBancoDeDados))
        {
            try
            {
                connection.Open();

                // Verificar saldo
                string query = "SELECT Saldo FROM Contas WHERE PessoaID = @pessoaId";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@pessoaId", pessoaId);

                double saldo = Convert.ToDouble(command.ExecuteScalar());

                if (saldo >= valor)
                {
                    // Atualizar saldo
                    query = "UPDATE Contas SET Saldo = Saldo - @valor WHERE PessoaID = @pessoaId";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@valor", valor);
                    command.Parameters.AddWithValue("@pessoaId", pessoaId);
                    command.ExecuteNonQuery();

                    // Inserir no extrato
                    query = "INSERT INTO Extrato (ContaID, TipoTransacao, Valor) " +
                            "SELECT ContaID, 'Saque', @valor FROM Contas WHERE PessoaID = @pessoaId";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@valor", -valor);
                    command.Parameters.AddWithValue("@pessoaId", pessoaId);
                    command.ExecuteNonQuery();

                    Console.Clear();
                    Console.WriteLine("              =======================================               ");
                    Console.WriteLine("                     Saque Realizado com Sucesso                   ");
                    Console.WriteLine("              =======================================               ");
                    Console.WriteLine("                                                                    ");
                    Thread.Sleep(1000);
                    TelaContaLogada(pessoaId);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("              =======================================               ");
                    Console.WriteLine("                   Saldo insuficiente para saque!                   ");
                    Console.WriteLine("              =======================================               ");
                    Thread.Sleep(1000);
                    TelaContaLogada(pessoaId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }

    public static void TelaExtrato(int pessoaId)
    {
        Console.Clear();

        double subtotal = 0; // Variável para armazenar o subtotal

        // Exibir extrato
        using (MySqlConnection connection = new MySqlConnection(conexaoBancoDeDados))
        {
            try
            {
                connection.Open();
                string query = "SELECT TipoTransacao, Valor, DataTransacao FROM Extrato " +
                               "WHERE ContaID = (SELECT ContaID FROM Contas WHERE PessoaID = @pessoaId)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@pessoaId", pessoaId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("                              EXTRATO                               ");
                        Console.WriteLine("              =======================================               ");
                        while (reader.Read())
                        {
                            Console.WriteLine($"                    Data: {reader.GetDateTime("DataTransacao")}   ");
                            Console.WriteLine($"                   Tipo de Movimentação: {reader.GetString("TipoTransacao")}        ");
                            Console.WriteLine($"                    Valor da Movimentação: {reader.GetDouble("Valor")}              ");
                            Console.WriteLine("              =======================================               ");

                            // Atualiza o subtotal
                            subtotal += reader.GetDouble("Valor");
                        }

                        // Exibe o subtotal

                        Console.WriteLine($"                         SUB TOTAl: {subtotal:F2}                        ");
                        Console.WriteLine("              =======================================               ");
                    }
                    else
                    {
                        Console.WriteLine("              =======================================               ");
                        Console.WriteLine("                   Não há extrato a ser exibido!                    ");
                        Console.WriteLine("              =======================================               ");
                    }
                }

                Console.ReadKey();
                TelaContaLogada(pessoaId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }
}





