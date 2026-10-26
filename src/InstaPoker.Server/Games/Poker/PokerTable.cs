using InstaPoker.Core.Games;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Server.Games;

public class PokerTable : GameTable
{

    private Room _room;
    private List<ClientConnection> _players = [];                   //seat order
    private Dictionary<ClientConnection, GameCard[]> _hands = [];
    private Dictionary<ClientConnection, int> _balances = [];
    private HashSet<ClientConnection> _folded = [];
    private Dictionary<ClientConnection, int> _betsThisRound = [];
    private ClientConnection _dealer = null!; 
    private List<GameCard> _deck = [];
    private Stack<GameCard> _stack;
    private List<GameCard> _tableCards = [];
    private Random _rng = new();
    private int _dealerIndex;
    private int _pot;
    private int _currentBet;
    
    public PokerTable(Room room)
    {
        _room = room;
        _deck.AddRange(CardMethods.CreateDeck());
        _players.AddRange(room.ConnectedUsers);
    }

    // fired when we receive OwnerStartGameNotification
    public override async Task StartGame() {
        // send balance requests
        await GetBalances();
        // CalculateState();

    }

    private async Task GetBalances() {
        List<(ClientConnection,Task<BalanceQueryResponse>)> balanceTasks = [];
        foreach (ClientConnection conn in _players) {
            Task<BalanceQueryResponse> task = conn.SendRequest<BalanceQueryRequest, BalanceQueryResponse>(new BalanceQueryRequest());
            balanceTasks.Add((conn,task));
        }
        await Task.WhenAll(balanceTasks.Select(x => x.Item2));
        foreach ((ClientConnection conn,Task<BalanceQueryResponse> task) in balanceTasks) {
            _balances[conn] = task.Result.Balance;
        }
    }

    private void SetupDeck()
    {
        _deck = CardMethods.CreateDeck().ToList();
        _rng.Shuffle(_deck);
        _stack = new Stack<GameCard>(_deck);
        _tableCards.Clear();
        _folded.Clear();
        _pot = 0;
        _currentBet = 0;
    }

    private void DetermineRoles(out ClientConnection sb, out ClientConnection bb)
    {
        _dealerIndex = _rng.Next(_players.Count);
        _dealer = _players[_dealerIndex];
        sb = _players[(_dealerIndex + 1) % _players.Count];
        bb = _players[(_dealerIndex + 2) % _players.Count];
    }

    private void DealCards()
    {
        foreach (ClientConnection conn in _players)
            _hands[conn] = [_stack.Pop(),_stack.Pop()];
    }

    private async Task SendCards()
    {
        foreach (ClientConnection conn in _players)
        {
            await conn.MessageWriter.WriteAsync(new DealCardsNotification
            {
                Card1 = _hands[conn][0],
                Card2 = _hands[conn][1],
            });
        }
    }

    // private void CalculateState() {
    //     rng.Shuffle(deck);
    //     stack = new Stack<GameCard>(deck);
    //     dealer = conns[rng.Next(conns.Count)];
    //
    //     for (int i = 0; i < 2; i++) {
    //         foreach (ClientConnection conn in conns) {
    //             if (i == 0) {
    //                 hands[conn] = new GameCard[2];
    //             }
    //             hands[conn][i] = stack.Pop();
    //         }
    //     }
    //
    //     // card burn
    //     _ = stack.Pop();
    //     // 3 community cards
    //     for (int i = 0; i < 3; i++) {
    //         tableCards.Add(stack.Pop());
    //     }
    //
    //     _ = stack.Pop(); // burn
    //     tableCards.Add(stack.Pop()); // 4th card
    //     _ = stack.Pop(); // burn
    //     tableCards.Add(stack.Pop()); // 5th card
    // }
    
}