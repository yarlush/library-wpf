using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LibraryWpf.Views;

public class AboutView : UserControl
{
    public AboutView()
    {
        Background = new SolidColorBrush(Color.FromRgb(244, 246, 249));

        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
        header.Children.Add(new TextBlock
        {
            Text = "О программе",
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121))
        });
        header.Children.Add(new TextBlock
        {
            Text = "Информационная система «Библиотека»",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126)),
            Margin = new Thickness(0, 4, 0, 0)
        });
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        var card = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(24),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 231, 236)),
            BorderThickness = new Thickness(1),
            MaxWidth = 820,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = "Курсовая работа по дисциплине «Базы данных»",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121)),
            Margin = new Thickness(0, 0, 0, 12)
        });
        stack.Children.Add(Paragraph(
            "Приложение предназначено для автоматизации учёта библиотечного фонда, регистрации читателей, " +
            "оформления выдачи и возврата книг, бронирования и формирования отчётности."));
        stack.Children.Add(Paragraph(
            "Технологии: C#, WPF, .NET 10, Entity Framework Core 10, Microsoft SQL Server 2019 Express (LocalDB)."));
        stack.Children.Add(Paragraph(
            "Состав: главная панель, справочники книг, читателей, экземпляров, авторов, издательств, жанров " +
            "и сотрудников, журнал выдач, бронирования, связи книг и авторов, операции обслуживания."));
        stack.Children.Add(Paragraph(
            "Выполнил: студент группы Б723 Лушников Ярослав Александрович."));
        stack.Children.Add(Paragraph(
            "Направление: 02.03.03 «Математическое обеспечение и администрирование " +
            "информационных систем»."));
        stack.Children.Add(Paragraph(
            "Руководитель: к.т.н., доцент Бильгаева Л.П. Улан-Удэ, 2026."));
        card.Child = stack;
        Grid.SetRow(card, 1);
        root.Children.Add(card);

        Content = root;
    }

    private static TextBlock Paragraph(string text)
        => new()
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            LineHeight = 22,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            Margin = new Thickness(0, 0, 0, 10)
        };
}
