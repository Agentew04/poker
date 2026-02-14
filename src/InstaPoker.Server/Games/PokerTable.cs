using InstaPoker.Core.Games;
using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Server.Games;

public class PokerTable : GameTable {

    private List<ClientConnection> conns = [];
    private Dictionary<ClientConnection, GameCard[]> hands = [];
    private Dictionary<ClientConnection, int> balances = [];
    private ClientConnection dealer = null!; 
    private List<GameCard> deck = [];
    private Stack<GameCard> stack;
    private List<GameCard> tableCards = [];
    private Random rng = new();
    
    public PokerTable(Room room) {
        deck.AddRange(CardMethods.CreateDeck());
        conns.AddRange(room.ConnectedUsers);
    }

    // fired when we receive OwnerStartGameNotification
    public override async Task StartGame() {
        // send balance requests
        await GetBalances();
        CalculateState();

    }

    private async Task GetBalances() {
        List<(ClientConnection,Task<BalanceQueryResponse>)> balanceTasks = [];
        foreach (ClientConnection conn in conns) {
            Task<BalanceQueryResponse> task = conn.SendRequest<BalanceQueryRequest, BalanceQueryResponse>(new BalanceQueryRequest());
            balanceTasks.Add((conn,task));
        }
        await Task.WhenAll(balanceTasks.Select(x => x.Item2));
        foreach ((ClientConnection,Task<BalanceQueryResponse>) balResponse in balanceTasks) {
            BalanceQueryResponse resp = balResponse.Item2.Result;
            balances[balResponse.Item1] = resp.Balance;
        }
    }

    private void CalculateState() {
        rng.Shuffle(deck);
        stack = new Stack<GameCard>(deck);
        dealer = conns[rng.Next(conns.Count)];

        for (int i = 0; i < 2; i++) {
            foreach (ClientConnection conn in conns) {
                if (i == 0) {
                    hands[conn] = new GameCard[2];
                }
                hands[conn][i] = stack.Pop();
            }
        }

        // card burn
        _ = stack.Pop();
        // 3 community cards
        for (int i = 0; i < 3; i++) {
            tableCards.Add(stack.Pop());
        }

        _ = stack.Pop(); // burn
        tableCards.Add(stack.Pop()); // 4th card
        _ = stack.Pop(); // burn
        tableCards.Add(stack.Pop()); // 5th card
    }
}