# DigiBank com MySQL

Este repositório contém o projeto **DigiBank**, um sistema bancário simulado desenvolvido em **C#**, agora integrado ao banco de dados **MySQL**. O objetivo do projeto é aplicar conceitos de programação orientada a objetos (POO) em C#, juntamente com a integração de um banco de dados real para gerenciar contas, transações e extratos bancários. O sistema oferece funcionalidades como criação de contas, login de usuários, depósitos, saques e consulta de extratos.

## Funcionalidades

### Tela Principal
A tela principal permite ao usuário escolher entre as seguintes opções:
1. Criar uma nova conta.
2. Entrar com CPF e senha para acessar uma conta existente.
3. Sair da aplicação.

### Criar Conta
Ao criar uma conta, o usuário deve fornecer:
- Nome
- CPF
- Senha

A conta será criada no banco de dados com um número de conta e agência gerados aleatoriamente, e o saldo inicial será 0.

### Login
Para fazer login, o usuário precisa inserir o CPF e a senha. Caso as informações estejam corretas, o sistema mostrará o nome do usuário, número da conta, número da agência e saldo atual.

### Conta Logada
Depois de fazer login, o usuário pode acessar as seguintes opções:
1. Realizar um depósito.
2. Realizar um saque.
3. Consultar o extrato.
4. Sair da conta e retornar à tela principal.

### Depósito
O usuário pode realizar um depósito em sua conta. O valor depositado será somado ao saldo da conta e registrado no extrato.

### Saque
O usuário pode realizar um saque, desde que o saldo da conta seja suficiente. O valor sacado será subtraído do saldo e registrado no extrato.

### Extrato
O extrato mostra o histórico de transações da conta, incluindo depósitos e saques realizados. O valor total das transações será exibido no final.


