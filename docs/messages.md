# Mensagens

Esse arquivo tem uma lista das mensagens/pacotes que devem existir.
Outras mensagens podem ser adicionadas, essa lista é aumentada de 
acordo com a demanda.

## Arquitetura

Para possibilitar uma comunicação fluida entre o servidor e os jogadores,
existem 3 tipos de pacotes diferentes:
* **Request:** uma mensagem é do tipo request quando ela pede algo do
outro lado da conexão. É esperado que seja recebido uma **Response** correspondente.
* **Response:** retorna dados que um request pediu.
* **Notification:** uma notificação de um dos comunicantes que algo aconteceu
ou uma ação foi realizada. Não é esperado uma resposta.

Exemplos:
1.  * Cliente A manda Request de Entrar em Sala com código.
    * Servidor manda Response dizendo se foi possível entrar na sala ou não.
      * Se sim, manda lista de usuários e configurações na Response
    * Cliente B (já na sala) manda Notification para servidor, indicando que
    saiu da sala. _[Cliente B não espera receber uma resposta de servidor]_
    * Servidor manda Notification para cliente A dizendo que Cliente B saiu da sala. 
    _[Servidor não espera que Cliente A responda nada]_
2. * Cliente A manda Request para criar sala.
   * Servidor manda Response com código da sala.
   * Cliente B entra em sala (procedimento anterior, notificando A).
   * Cliente A manda Notification para Servidor com novas configurações da sala.
   * Servidor manda Notification para Cliente B com as novas configurações da sala.

## Salas

### Criar sala

Espera-se o retorno de um código de 6 dígitos da sala criada. Parâmetros
necessários podem ser definidos pelo dev do servidor (autenticação etc.).
A sala criada deve ter uma série de configurações padrão já definidas

### Configurações da Sala

O dono da sala(usuário que mandou o pacote `Criar Sala`) pode mandar um
pedido para recuperar as configurações da sala presentes no servidor
e um pacote para autualizá-los.
As configurações da sala atualmente devem conter os seguintes elementos:
* MaxPlayers: número máximo de players da sala, contando o dono.
* SmallBlind: quantia de dinheiro que o small blind deve apostar. 
Reflete no big blind também (2x small blind)
* MaxBet: quantia máxima de uma aposta. Valores menores ou iguais a 0
significam que não há limite.
* IsAllInEnabled: define se os usuários podem dar all-in.

### Atualização de Jogadores

Deve haver uma ou mais mensagens para sinalizar atualizações na lista
de jogadores em uma sala. Exemplos:
* jogador 'Carlos' entrou.
* jogador 'Felipe' foi expulso.
* dono da sala agora é 'Joao' (servidor decide quem é o novo
dono quando o anterior sai da sala).

Apenas o servidor manda Notifications falando quem é o novo dono.
Ninguém tem permissão para voluntariamente trocar o dono da sala. Usuários
podem apenas mandar Notifications falando que saíram da sala ou receberem
atualizações do servidor.

### Entrar em sala

Deve haver uma mensagem para o jogador entrar em uma sala. É passado
como parâmetro o código da sala desejada. O servidor deve responder se
a operação foi um sucesso ou não. Caso seja um sucesso, o servidor deve
mandar na response junto a lista completa de usuários na sala e as configurações
atuais.

### Começo de Jogo

Quando o dono da sala criar em 'Start Game', um pacote deve ser mandado
a todos os jogadores para que os clientes atualizem sua interface
e esperem os novos tipos de mensagens. 