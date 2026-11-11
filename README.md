# BilheteriaCine

[![CI](https://github.com/LucasL32323/BilheteriaCine/actions/workflows/ci.yml/badge.svg)](https://github.com/LucasL32323/BilheteriaCine/actions/workflows/ci.yml)

Bilheteria de cinema em C# (.NET 10) com as regras que fazem diferença no balcão: preço de inteira e meia (com arredondamento de centavo), **cota de 40% de meia-entrada por sessão**, acompanhante de pessoa com deficiência e compra "tudo ou nada".

## Como o TDD foi aplicado

Cada regra nasceu de um teste que falhava (Red), ganhou o código para passar (Green) e só então foi limpa (Refactor).

| Etapa | Exemplo real do projeto |
| --- | --- |
| **Red** | Os testes foram escritos antes de existir qualquer classe de `Cine.Core`; o projeto de testes nem compilava. Um deles: `Comprar_CompraMistaQueEstouraACota_RecusaAInteiraTambem`. |
| **Green** | `Bilheteria.Comprar` passou a validar lugares, acompanhante e cota *antes* de alterar qualquer contador. |
| **Refactor** | A decisão "este tipo de ingresso consome a cota?" saiu da bilheteria e foi para `PoliticaDeMeiaEntrada.ConsomeCota`, sem mudar nenhum teste de comportamento. |

Os casos de borda viraram testes explícitos: sessão pequena em que 40% arredonda para zero meias (`Comprar_SessaoPequenaComCotaZero_RecusaQualquerMeiaDaCota`), preço com meio centavo (`PrecoDe_TipoDeIngresso_AplicaMeiaEArredondaParaCentavo`, que cobre R$ 31,55 → meia de R$ 15,78) e qual motivo aparece quando duas regras falham juntas (`Comprar_SemLugaresECotaEstourada_InformaPrimeiroAFaltaDeLugares`).

## Arquitetura

```
BilheteriaCine/
├── src/
│   ├── Cine.Core/   regras de negócio, sem dependências
│   └── Cine.Cli/    linha de comando
└── tests/
    └── Cine.Tests/  xUnit
```

```
Cine.Cli ──▶ Cine.Core ◀── Cine.Tests

Cine.Core:  Bilheteria ──▶ Sessao (preços)
                │
                └──▶ PoliticaDeMeiaEntrada (cota) ──▶ TipoDeIngresso
```

## Regras de negócio

1. Existem seis tipos de ingresso: inteira, meia-estudante, meia-idoso, meia-PCD, meia-acompanhante de PCD e meia-jovem de baixa renda.
2. Toda meia custa metade da inteira, arredondada para o centavo (0,005 sobe). O total da compra é a soma dos ingressos já arredondados.
3. O preço da inteira é maior que zero e tem no máximo duas casas decimais; a lotação é maior que zero.
4. A cota de meia-entrada é de 40% da lotação, arredondada para baixo (sala de 7 lugares → 2 meias; sala de 2 lugares → 0).
5. Estudante, jovem de baixa renda, PCD e acompanhante consomem a cota. A meia do idoso não consome (ver decisões técnicas).
6. Cada acompanhante precisa de um PCD na mesma compra, na proporção de um para um.
7. A compra é atômica: se qualquer regra falhar, nenhum ingresso é vendido e nenhum contador muda.
8. A ordem das verificações é: lugares disponíveis → acompanhante → cota. O motivo informado é o da primeira regra violada.
9. Uma compra precisa ter ao menos um ingresso.

## Decisões técnicas

- **A política é um objeto** (`PoliticaDeMeiaEntrada`) e não constantes espalhadas: percentual e tipos que entram na cota são configuráveis, então ajustar a regra não mexe na `Bilheteria`.
- **Fundamento da cota**: o padrão segue o art. 1º, §10, da Lei 12.933/2013 (40% dos ingressos disponíveis para meia-entrada) e o §8º (meia para pessoa com deficiência e seu acompanhante). Deixar o idoso fora da cota é uma **decisão de modelagem**, apoiada no fato de a meia do idoso ter origem própria, no Estatuto da Pessoa Idosa (Lei 10.741/2003). Se o seu caso exigir contar o idoso, basta criar a política com ele na lista. Este projeto é um exercício de modelagem de software e não substitui orientação jurídica.
- **`decimal` para dinheiro** e `MidpointRounding.AwayFromZero` explícito, porque o padrão do .NET (arredondar para o par) daria resultado diferente do que o cliente espera no balcão.
- **`ResultadoDaCompra` em vez de exceções** para as recusas: sessão lotada ou cota esgotada são resultados normais do negócio. Exceções ficam para entrada inválida (lista vazia, preço negativo).
- **Fora do escopo**: lugares marcados, promoções (o benefício legal não se acumula com outras promoções) e pagamento.
- `TreatWarningsAsErrors` ligado nos projetos de produção. No projeto de testes ele fica desligado, porque os analisadores do xUnit mudam entre versões.

## Como rodar

```bash
dotnet test

# preço da inteira, lotação e ingressos (tipo ou tipo:quantidade)
dotnet run --project src/Cine.Cli -- comprar 32 100 inteira:2 estudante idoso
dotnet run --project src/Cine.Cli -- cota 120
```

Tipos aceitos: `inteira`, `estudante`, `idoso`, `pcd`, `acompanhante`, `jovem`. Exemplo de saída:

```
  Inteira                  R$ 32,00
  Inteira                  R$ 32,00
  MeiaEstudante            R$ 16,00
  MeiaIdoso                R$ 16,00
  Total                    R$ 96,00
Lugares disponíveis: 96 | Meias restantes na cota: 39
```
