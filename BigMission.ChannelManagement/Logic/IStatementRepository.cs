namespace BigMission.ChannelManagement.Shared.Logic;

public interface IStatementRepository
{
    public Task<Statements> GetStatementsAsync(int statementId);
    public Task SetStatementsAsync(Statements s);
}
