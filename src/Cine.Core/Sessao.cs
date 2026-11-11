namespace Cine.Core;

/// <summary>Uma sessão de cinema: filme, preço da inteira e lotação da sala.</summary>
public sealed record Sessao
{
    public Sessao(string filme, decimal precoInteira, int lotacao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filme);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(precoInteira, 0m);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lotacao, 0);

        if (decimal.Round(precoInteira, 2) != precoInteira)
        {
            throw new ArgumentOutOfRangeException(nameof(precoInteira), precoInteira, "O preço deve ter no máximo duas casas decimais.");
        }

        Filme = filme;
        PrecoInteira = precoInteira;
        Lotacao = lotacao;
    }

    public string Filme { get; }

    public decimal PrecoInteira { get; }

    public int Lotacao { get; }

    /// <summary>Meia é metade da inteira, arredondada para o centavo (0,005 sobe).</summary>
    public decimal PrecoDe(TipoDeIngresso tipo) => tipo == TipoDeIngresso.Inteira
        ? PrecoInteira
        : decimal.Round(PrecoInteira / 2m, 2, MidpointRounding.AwayFromZero);
}
