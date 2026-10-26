using System.Globalization;
using System.Text;

namespace agot_bg_website.Services.Statistics;

/// <summary>CSV exports for the public statistics page.</summary>
public static class GameStatisticsCsv
{
    public static byte[] HouseStatistics(
        GameStatisticsOverview overview,
        string scenarioLabel
    ) =>
        Encode(
            Csv(
                [
                    "Scenario",
                    "House",
                    "Human seats",
                    "Wins",
                    "Win rate (%)",
                    "Top win rate",
                ],
                overview.Houses.Select(house =>
                    new string?[]
                    {
                        scenarioLabel,
                        house.Name,
                        Number(house.Seats),
                        Number(house.Wins),
                        house.WinRate is { } rate
                            ? (rate * 100).ToString("F1", CultureInfo.InvariantCulture)
                            : "",
                        house.IsTop ? "Yes" : "No",
                    }
                )
            )
        );

    public static byte[] Games(IEnumerable<LatestGame> games) =>
        Encode(
            Csv(
                [
                    "Game ID",
                    "Name",
                    "Scenario",
                    "Game type",
                    "Winner house",
                    "Winner",
                    "Final round",
                    "Finished at (UTC)",
                ],
                games.Select(game =>
                    new string?[]
                    {
                        game.Id.ToString("D"),
                        game.Name,
                        game.ScenarioLabel,
                        game.IsPbem ? "Play By E-Mail" : "Live",
                        game.WinnerHouseName,
                        game.WinnerUserName,
                        game.Round?.ToString(CultureInfo.InvariantCulture),
                        game.FinishedAt
                            .ToUniversalTime()
                            .ToString("O", CultureInfo.InvariantCulture),
                    }
                )
            )
        );

    private static string Csv(IEnumerable<string> headers, IEnumerable<string?[]> rows)
    {
        var output = new StringBuilder();
        AppendRow(output, headers);
        foreach (var row in rows)
        {
            AppendRow(output, row);
        }

        return output.ToString();
    }

    private static void AppendRow(StringBuilder output, IEnumerable<string?> fields)
    {
        var first = true;
        foreach (var field in fields)
        {
            if (!first)
            {
                output.Append(',');
            }

            output.Append(Escape(field ?? ""));
            first = false;
        }

        output.Append("\r\n");
    }

    private static string Escape(string value)
    {
        var firstDataCharacter = value.FirstOrDefault(c => !char.IsWhiteSpace(c));
        if (firstDataCharacter is '=' or '+' or '-' or '@')
        {
            value = "'" + value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static byte[] Encode(string csv)
    {
        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(csv);
        var output = new byte[preamble.Length + content.Length];
        preamble.CopyTo(output, 0);
        content.CopyTo(output, preamble.Length);
        return output;
    }
}
