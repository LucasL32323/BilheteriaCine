using System.Globalization;
using Cine.Core;

return Executar(args);

static int Executar(string[] args)
{
    if (args.Length == 0)
    {
        ImprimirAjuda();
        return 0;
    }

    try
    {
        return args[0].ToLowerInvariant() switch
        {
            "comprar" => Comprar(args[1..]),
            "cota" => Cota(args[1..]),
            _ => ComandoDesconhecido(args[0]),
        };
    }
    catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
    {
        Console.Error.WriteLine($"Entrada inválida: {ex.Message.Split(" (Parameter", StringSplitOptions.None)[0]}");
        return 1;
    }
}

static int Comprar(string[] argumentos)
{
    if (argumentos.Length < 3)
    {
        Console.Error.WriteLine("Uso: comprar <precoDaInteira> <lotacao> <tipo[:quantidade]> ...");
        return 1;
    }

    var preco = decimal.Parse(argumentos[0].Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    var lotacao = int.Parse(argumentos[1], CultureInfo.InvariantCulture);
    var ingressos = argumentos[2..].SelectMany(LerTipo).ToList();

    var sessao = new Sessao("Sessão", preco, lotacao);
    var bilheteria = new Bilheteria(sessao);

    var resultado = bilheteria.Comprar(ingressos);
    if (!resultado.Sucesso)
    {
        Console.WriteLine($"Compra recusada: {Descrever(resultado.Motivo)}");
        return 2;
    }

    foreach (var ingresso in resultado.Ingressos)
    {
        Console.WriteLine($"  {ingresso.Tipo,-22} {Dinheiro(ingresso.Preco),10}");
    }

    Console.WriteLine($"  {"Total",-22} {Dinheiro(resultado.Total),10}");
    Console.WriteLine($"Lugares disponíveis: {bilheteria.Disponiveis} | Meias restantes na cota: {bilheteria.CotaRestante}");

    return 0;
}

static int Cota(string[] argumentos)
{
    if (argumentos.Length != 1)
    {
        Console.Error.WriteLine("Uso: cota <lotacao>");
        return 1;
    }

    var lotacao = int.Parse(argumentos[0], CultureInfo.InvariantCulture);
    Console.WriteLine($"Sala de {lotacao} lugares: até {PoliticaDeMeiaEntrada.Padrao.CotaMaxima(lotacao)} meias na cota de 40%.");

    return 0;
}

static IEnumerable<TipoDeIngresso> LerTipo(string texto)
{
    var partes = texto.Split(':');
    var quantidade = partes.Length > 1 ? int.Parse(partes[1], CultureInfo.InvariantCulture) : 1;
    ArgumentOutOfRangeException.ThrowIfLessThan(quantidade, 1);

    var tipo = partes[0].ToLowerInvariant() switch
    {
        "inteira" => TipoDeIngresso.Inteira,
        "estudante" => TipoDeIngresso.MeiaEstudante,
        "idoso" => TipoDeIngresso.MeiaIdoso,
        "pcd" => TipoDeIngresso.MeiaPcd,
        "acompanhante" => TipoDeIngresso.MeiaAcompanhantePcd,
        "jovem" => TipoDeIngresso.MeiaJovemBaixaRenda,
        _ => throw new ArgumentException($"Tipo de ingresso desconhecido: {partes[0]}."),
    };

    return Enumerable.Repeat(tipo, quantidade);
}

static string Descrever(MotivoDaRecusa motivo) => motivo switch
{
    MotivoDaRecusa.SemLugares => "não há lugares suficientes na sessão.",
    MotivoDaRecusa.CotaDeMeiaEsgotada => "a cota de meia-entrada da sessão não comporta esta compra.",
    MotivoDaRecusa.AcompanhanteSemPcd => "cada acompanhante precisa de uma meia de pessoa com deficiência na mesma compra.",
    _ => "motivo não informado.",
};

static string Dinheiro(decimal valor) =>
    "R$ " + valor.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

static int ComandoDesconhecido(string comando)
{
    Console.Error.WriteLine($"Comando desconhecido: {comando}");
    ImprimirAjuda();
    return 1;
}

static void ImprimirAjuda()
{
    Console.WriteLine("BilheteriaCine");
    Console.WriteLine("  comprar <precoDaInteira> <lotacao> <tipo[:quantidade]> ...");
    Console.WriteLine("      tipos: inteira, estudante, idoso, pcd, acompanhante, jovem");
    Console.WriteLine("  cota <lotacao>");
}
