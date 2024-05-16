namespace BigMission.ChannelManagement.Logic;

public class Statements
{
    public int Id { get; set; }
    public List<List<Comparison>> StatementRows { get; set; } = [];
}
