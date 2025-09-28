namespace CoolCardGames.Library.Core.CardTypes;

public sealed record Joker0          : CardValue { public readonly static Joker0          Instance = new(); private Joker0()          : base(Rank.Joker0, Suit.Joker) { } }
public sealed record Joker1          : CardValue { public readonly static Joker1          Instance = new(); private Joker1()          : base(Rank.Joker1, Suit.Joker) { } }

public sealed record AceOfHearts     : CardValue { public readonly static AceOfHearts     Instance = new(); private AceOfHearts()     : base(Rank.Ace,    Suit.Hearts) { } }
public sealed record TwoOfHearts     : CardValue { public readonly static TwoOfHearts     Instance = new(); private TwoOfHearts()     : base(Rank.Two,    Suit.Hearts) { } }
public sealed record ThreeOfHearts   : CardValue { public readonly static ThreeOfHearts   Instance = new(); private ThreeOfHearts()   : base(Rank.Three,  Suit.Hearts) { } }
public sealed record FourOfHearts    : CardValue { public readonly static FourOfHearts    Instance = new(); private FourOfHearts()    : base(Rank.Four,   Suit.Hearts) { } }
public sealed record FiveOfHearts    : CardValue { public readonly static FiveOfHearts    Instance = new(); private FiveOfHearts()    : base(Rank.Five,   Suit.Hearts) { } }
public sealed record SixOfHearts     : CardValue { public readonly static SixOfHearts     Instance = new(); private SixOfHearts()     : base(Rank.Six,    Suit.Hearts) { } }
public sealed record SevenOfHearts   : CardValue { public readonly static SevenOfHearts   Instance = new(); private SevenOfHearts()   : base(Rank.Seven,  Suit.Hearts) { } }
public sealed record EightOfHearts   : CardValue { public readonly static EightOfHearts   Instance = new(); private EightOfHearts()   : base(Rank.Eight,  Suit.Hearts) { } }
public sealed record NineOfHearts    : CardValue { public readonly static NineOfHearts    Instance = new(); private NineOfHearts()    : base(Rank.Nine,   Suit.Hearts) { } }
public sealed record TenOfHearts     : CardValue { public readonly static TenOfHearts     Instance = new(); private TenOfHearts()     : base(Rank.Ten,    Suit.Hearts) { } }
public sealed record JackOfHearts    : CardValue { public readonly static JackOfHearts    Instance = new(); private JackOfHearts()    : base(Rank.Jack,   Suit.Hearts) { } }
public sealed record QueenOfHearts   : CardValue { public readonly static QueenOfHearts   Instance = new(); private QueenOfHearts()   : base(Rank.Queen,  Suit.Hearts) { } }
public sealed record KingOfHearts    : CardValue { public readonly static KingOfHearts    Instance = new(); private KingOfHearts()    : base(Rank.King,   Suit.Hearts) { } }

public sealed record AceOfSpades     : CardValue { public readonly static AceOfSpades     Instance = new(); private AceOfSpades()     : base(Rank.Ace,    Suit.Spades) { } }
public sealed record TwoOfSpades     : CardValue { public readonly static TwoOfSpades     Instance = new(); private TwoOfSpades()     : base(Rank.Two,    Suit.Spades) { } }
public sealed record ThreeOfSpades   : CardValue { public readonly static ThreeOfSpades   Instance = new(); private ThreeOfSpades()   : base(Rank.Three,  Suit.Spades) { } }
public sealed record FourOfSpades    : CardValue { public readonly static FourOfSpades    Instance = new(); private FourOfSpades()    : base(Rank.Four,   Suit.Spades) { } }
public sealed record FiveOfSpades    : CardValue { public readonly static FiveOfSpades    Instance = new(); private FiveOfSpades()    : base(Rank.Five,   Suit.Spades) { } }
public sealed record SixOfSpades     : CardValue { public readonly static SixOfSpades     Instance = new(); private SixOfSpades()     : base(Rank.Six,    Suit.Spades) { } }
public sealed record SevenOfSpades   : CardValue { public readonly static SevenOfSpades   Instance = new(); private SevenOfSpades()   : base(Rank.Seven,  Suit.Spades) { } }
public sealed record EightOfSpades   : CardValue { public readonly static EightOfSpades   Instance = new(); private EightOfSpades()   : base(Rank.Eight,  Suit.Spades) { } }
public sealed record NineOfSpades    : CardValue { public readonly static NineOfSpades    Instance = new(); private NineOfSpades()    : base(Rank.Nine,   Suit.Spades) { } }
public sealed record TenOfSpades     : CardValue { public readonly static TenOfSpades     Instance = new(); private TenOfSpades()     : base(Rank.Ten,    Suit.Spades) { } }
public sealed record JackOfSpades    : CardValue { public readonly static JackOfSpades    Instance = new(); private JackOfSpades()    : base(Rank.Jack,   Suit.Spades) { } }
public sealed record QueenOfSpades   : CardValue { public readonly static QueenOfSpades   Instance = new(); private QueenOfSpades()   : base(Rank.Queen,  Suit.Spades) { } }
public sealed record KingOfSpades    : CardValue { public readonly static KingOfSpades    Instance = new(); private KingOfSpades()    : base(Rank.King,   Suit.Spades) { } }

public sealed record AceOfDiamonds   : CardValue { public readonly static AceOfDiamonds   Instance = new(); private AceOfDiamonds()   : base(Rank.Ace,    Suit.Diamonds) { } }
public sealed record TwoOfDiamonds   : CardValue { public readonly static TwoOfDiamonds   Instance = new(); private TwoOfDiamonds()   : base(Rank.Two,    Suit.Diamonds) { } }
public sealed record ThreeOfDiamonds : CardValue { public readonly static ThreeOfDiamonds Instance = new(); private ThreeOfDiamonds() : base(Rank.Three,  Suit.Diamonds) { } }
public sealed record FourOfDiamonds  : CardValue { public readonly static FourOfDiamonds  Instance = new(); private FourOfDiamonds()  : base(Rank.Four,   Suit.Diamonds) { } }
public sealed record FiveOfDiamonds  : CardValue { public readonly static FiveOfDiamonds  Instance = new(); private FiveOfDiamonds()  : base(Rank.Five,   Suit.Diamonds) { } }
public sealed record SixOfDiamonds   : CardValue { public readonly static SixOfDiamonds   Instance = new(); private SixOfDiamonds()   : base(Rank.Six,    Suit.Diamonds) { } }
public sealed record SevenOfDiamonds : CardValue { public readonly static SevenOfDiamonds Instance = new(); private SevenOfDiamonds() : base(Rank.Seven,  Suit.Diamonds) { } }
public sealed record EightOfDiamonds : CardValue { public readonly static EightOfDiamonds Instance = new(); private EightOfDiamonds() : base(Rank.Eight,  Suit.Diamonds) { } }
public sealed record NineOfDiamonds  : CardValue { public readonly static NineOfDiamonds  Instance = new(); private NineOfDiamonds()  : base(Rank.Nine,   Suit.Diamonds) { } }
public sealed record TenOfDiamonds   : CardValue { public readonly static TenOfDiamonds   Instance = new(); private TenOfDiamonds()   : base(Rank.Ten,    Suit.Diamonds) { } }
public sealed record JackOfDiamonds  : CardValue { public readonly static JackOfDiamonds  Instance = new(); private JackOfDiamonds()  : base(Rank.Jack,   Suit.Diamonds) { } }
public sealed record QueenOfDiamonds : CardValue { public readonly static QueenOfDiamonds Instance = new(); private QueenOfDiamonds() : base(Rank.Queen,  Suit.Diamonds) { } }
public sealed record KingOfDiamonds  : CardValue { public readonly static KingOfDiamonds  Instance = new(); private KingOfDiamonds()  : base(Rank.King,   Suit.Diamonds) { } }

public sealed record AceOfClubs      : CardValue { public readonly static AceOfClubs      Instance = new(); private AceOfClubs()      : base(Rank.Ace,    Suit.Clubs) { } }
public sealed record TwoOfClubs      : CardValue { public readonly static TwoOfClubs      Instance = new(); private TwoOfClubs()      : base(Rank.Two,    Suit.Clubs) { } }
public sealed record ThreeOfClubs    : CardValue { public readonly static ThreeOfClubs    Instance = new(); private ThreeOfClubs()    : base(Rank.Three,  Suit.Clubs) { } }
public sealed record FourOfClubs     : CardValue { public readonly static FourOfClubs     Instance = new(); private FourOfClubs()     : base(Rank.Four,   Suit.Clubs) { } }
public sealed record FiveOfClubs     : CardValue { public readonly static FiveOfClubs     Instance = new(); private FiveOfClubs()     : base(Rank.Five,   Suit.Clubs) { } }
public sealed record SixOfClubs      : CardValue { public readonly static SixOfClubs      Instance = new(); private SixOfClubs()      : base(Rank.Six,    Suit.Clubs) { } }
public sealed record SevenOfClubs    : CardValue { public readonly static SevenOfClubs    Instance = new(); private SevenOfClubs()    : base(Rank.Seven,  Suit.Clubs) { } }
public sealed record EightOfClubs    : CardValue { public readonly static EightOfClubs    Instance = new(); private EightOfClubs()    : base(Rank.Eight,  Suit.Clubs) { } }
public sealed record NineOfClubs     : CardValue { public readonly static NineOfClubs     Instance = new(); private NineOfClubs()     : base(Rank.Nine,   Suit.Clubs) { } }
public sealed record TenOfClubs      : CardValue { public readonly static TenOfClubs      Instance = new(); private TenOfClubs()      : base(Rank.Ten,    Suit.Clubs) { } }
public sealed record JackOfClubs     : CardValue { public readonly static JackOfClubs     Instance = new(); private JackOfClubs()     : base(Rank.Jack,   Suit.Clubs) { } }
public sealed record QueenOfClubs    : CardValue { public readonly static QueenOfClubs    Instance = new(); private QueenOfClubs()    : base(Rank.Queen,  Suit.Clubs) { } }
public sealed record KingOfClubs     : CardValue { public readonly static KingOfClubs     Instance = new(); private KingOfClubs()     : base(Rank.King,   Suit.Clubs) { } }