namespace TeamFlowDesk.Services.Ui;

public class DetailSection
{
    public DetailSection(string title, string content)
    {
        Title = title;
        Content = content;
    }

    public string Title { get; }

    public string Content { get; }
}