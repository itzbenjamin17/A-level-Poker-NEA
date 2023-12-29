Imports System.Threading

'Improve bluffing
Public Class Game_Interface
    Const SmallBlind As Integer = 1
    Const BigBlind As Integer = 2
    Const HighestCard As Integer = 4
    Const SecondHighestCard As Integer = 3
    Const ThirdHighestCard As Integer = 2
    Const FourthHighestCard As Integer = 1
    Const FifthHighestCard As Integer = 0
    Public Shared ReadOnly Rand As New Random

    'Adjusts the probability of betting, the higher the number, the lower chance of betting when bluffing
    Const BetProbAdjuster As Integer = 320
    'Adjusts the probability of calling, the higher the number, the lower chance of betting when bluffing
    Const CallRaiseAdjuster As Integer = 180
    'Adjusts the probability of bluffing, the higher number, the higher chance of bluffing
    Const BluffProbAdjuster As Integer = 20
    'Adjusts how much of the chips the bot bets, the higher the number the higher amount of chips it bets
    Const BetAmountAdjuster As Single = 0.7
    'Adjusts how much of the chips the bot raises, the higher the number the higher amount of chips it raises to
    Const RaiseAmountAdjuster As Single = 0.25
    'Adjusts how important the position on the round is, the higher the number the higher importance of being early in the round
    Const BetChanceMultiplier As Integer = 15
    'Min amount computers will bet
    Const MinimumBetAmount As Integer = 5
    'The higher the number the less impact odds of win has
    Const BluffThreshold As Integer = 50
    Const PercentageMultiplier As Integer = 100
    'Controls the iterations on the monte carlo simulation
    Const Iterations As Integer = 5000
    Private ReadOnly player_Name As String
    Private ReadOnly HiddenCard As New Bitmap(".\Resources\Empty Card.png")
    Public PlayerInput As String
    Public PlayerInputNumber As Integer
    Public Sub New(name As String)
        InitializeComponent()
        Me.player_Name = name
    End Sub
    Public Sub Game_Interface_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MsgBox("Welcome to Texas Hold'em poker, the game will begin now ")
        'Create players
        Dim Player As New Player With {
            .Name = player_Name
        }
        Dim Computer1 As New Computer("Computer1")
        Dim Computer2 As New Computer("Computer2")
        Dim Computer3 As New Computer("Computer3")
        Dim Computer4 As New Computer("Computer4")
        Dim GamePot As Integer = 0
        Dim Players As New List(Of Player) From
            {Player, Computer1, Computer2, Computer3, Computer4}
        Dim Table As New List(Of Player)(Players)

        Dim GameOver As Boolean = False

        'Fist player in the table
        Dim Dealer As Integer = 0
        'Second player in the table and so on
        Dim SmallBlindPlayer As Integer = 1
        Dim BigBlindPlayer As Integer = 2
        Dim NextPlayer As Integer = 3

        While Not GameOver
            'Resetting the cards
            For Each item In Players
                HideCards(item)
            Next
            picCC1.Image = HiddenCard
            picCC2.Image = HiddenCard
            picCC3.Image = HiddenCard
            picCC4.Image = HiddenCard
            picCC5.Image = HiddenCard
            Application.DoEvents()
            'Checking if we have 1 player left

            If Table.Count = 1 Then
                GameOver = True
                ShowTheWin(Table(0))
                MsgBox($"{Table(0).Name} is the last man standing, {Table(0).Name} wins!!!")
                Exit While
            End If

            'creating new deck and shuffling it
            Dim deck As New Deck
            deck.Shuffle()
            'Dealing out the 2 hole cards
            For Each item In Players
                deck.Deal2Cards(item.HoleCards)
            Next
            DisplayCards(Player)

            Dim CommunityCards As New List(Of Card)
            Dim GameState As String = "Pre-Flop"
            Dim CurrentHighestBet As Integer = BigBlind

            'my rules
            'if they cannot pay their blind they are forced to go all in and the game continues as normal for everyone else
            Table(SmallBlindPlayer).PaySmallBlind(GamePot)
            Table(BigBlindPlayer).PayBigBlind(GamePot)
            RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)
            Dim NumOfPlayers As Integer = Table.Count

            While True
                'while anyone in the table hasnt matched the currenthighestbet and isnt all in and hasnt folded
                While Table.Any(Function(item) (item.CurrentBet < CurrentHighestBet AndAlso item.HasFolded = False) AndAlso item.IsAllIn = False)
                    NextPlayerTurn(Table, NextPlayer, CurrentHighestBet, GamePot, GameState, CommunityCards, NumOfPlayers, Dealer)
                    RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)
                End While

                'New round so current bets are 0
                For Each item In Table
                    item.ResetCurrentBet()
                Next
                'No one would have bet in the new round highest bet is 0
                CurrentHighestBet = 0
                HideComputerChoice()
                RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)

                'Flop
                GameState = "Flop"
                'dealing table cards
                deck.Deal3Cards(CommunityCards)
                picCC1.Image = New Bitmap(".\Resources\" & CommunityCards(0).ToString() & ".png")
                picCC2.Image = New Bitmap(".\Resources\" & CommunityCards(1).ToString() & ".png")
                picCC3.Image = New Bitmap(".\Resources\" & CommunityCards(2).ToString() & ".png")
                Application.DoEvents()
                BettingRound(Table, NextPlayer, CurrentHighestBet, GamePot, GameState, CommunityCards, NumOfPlayers, Dealer, Player, Computer1, Computer2, Computer3, Computer4)
                RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)

                'Turn
                GameState = "Turn"
                deck.DealCard(CommunityCards)
                picCC4.Image = New Bitmap(".\Resources\" & CommunityCards(3).ToString() & ".png")
                Application.DoEvents()
                BettingRound(Table, NextPlayer, CurrentHighestBet, GamePot, GameState, CommunityCards, NumOfPlayers, Dealer, Player, Computer1, Computer2, Computer3, Computer4)
                RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)

                'River
                GameState = "River"
                deck.DealCard(CommunityCards)
                picCC5.Image = New Bitmap(".\Resources\" & CommunityCards(4).ToString() & ".png")
                Application.DoEvents()
                BettingRound(Table, NextPlayer, CurrentHighestBet, GamePot, GameState, CommunityCards, NumOfPlayers, Dealer, Player, Computer1, Computer2, Computer3, Computer4)
                RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)

                'Showdown
                For Each item In Table
                    If item.HasFolded = False Then
                        DisplayCards(item)
                    End If
                Next
                GetWinner(Table, GamePot, CommunityCards)
                For Each item In Table
                    If item.Chips = 0 Then
                        item.IsOut = True
                        ShowTheOut(item)
                    End If
                Next

                Table.Clear()
                'Add players who are not out into the table
                For Each item In Players
                    If item.Chips > 0 Then
                        Table.Add(item)
                    End If
                Next
                If Table.Count = 1 Then
                    Exit While
                End If

                'it was messing up NextRound so i have to put this here
                For Each item In Players
                    item.ResetAttributes()
                    ResetFold(item)
                Next

                'Find the positions of the new roles
                NextRound(NextPlayer, SmallBlindPlayer, BigBlindPlayer, Dealer, Table)
                Exit While
            End While
        End While
    End Sub

    Public Sub BettingRound(ByRef Table As List(Of Player), ByRef NextPlayer As Integer, ByRef CurrentHighestBet As Integer, ByRef GamePot As Integer, ByRef GameState As String, ByRef CommunityCards As List(Of Card), ByRef NumOfPlayers As Integer, ByRef Dealer As Integer, ByRef Player As Player, ByRef Computer1 As Computer, ByRef Computer2 As Computer, ByRef Computer3 As Computer, ByRef Computer4 As Computer)
        Dim CurrentHighestBetCopy As Integer = CurrentHighestBet

        'getting the next player (next active player to the left of dealer) we can do it this way because when we are doing turns
        'we check if they have folded, so if this player cannot play for whatever reason it will go the the next player
        If Dealer = Table.Count - 1 Then
            NextPlayer = 0
        Else
            NextPlayer = Dealer + 1
        End If

        'at the start of each betting round after the preflop every active player has a turn
        For Each item In Table
            NextPlayerTurn(Table, NextPlayer, CurrentHighestBet, GamePot, GameState, CommunityCards, NumOfPlayers, Dealer)
            CurrentHighestBetCopy = CurrentHighestBet
            RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)
        Next
        'while anyone in the table hasnt matched the currenthighestbet and isnt all in and hasnt folded
        While Table.Any(Function(item) item.CurrentBet < CurrentHighestBetCopy AndAlso item.HasFolded = False AndAlso item.IsAllIn = False)
            NextPlayerTurn(Table, NextPlayer, CurrentHighestBet, GamePot, GameState, CommunityCards, NumOfPlayers, Dealer)
            CurrentHighestBetCopy = CurrentHighestBet
            RefreshNumbers(Player, Computer1, Computer2, Computer3, Computer4, GamePot)
        End While

        For Each item In Table
            item.ResetCurrentBet()
        Next
        CurrentHighestBet = 0
        HideComputerChoice()
        Application.DoEvents()
    End Sub
    Public Shared Function GetComputerPosition(Computer As Computer, Table As List(Of Player), Dealer As Integer) As Integer
        'Finding where the computer is placed on the table
        Dim Count As Integer = 1
        Dim position As Integer = Dealer + 1
        If position = Table.Count Then
            position = 0
        End If
        While True
            If Table(position) Is Computer Then
                Exit While
            End If
            position += 1
            Count += 1
            If position = Table.Count Then
                position = 0
            End If
        End While
        Return Count
    End Function
    Public Sub NextPlayerTurn(ByRef Table As List(Of Player), ByRef NextPlayer As Integer, ByRef CurrentHighestBet As Integer, ByRef GamePot As Integer, GameState As String, CommunityCards As List(Of Card), Players As Integer, Dealer As Integer)
        Dim count As Integer = 0
        'Count is the amount of players that can have a turn
        Dim tempCurrentHighestBet = CurrentHighestBet
        For item = 0 To Table.Count - 1
            If Table(item).HasFolded = False AndAlso Table(item).IsAllIn = False Then
                count += 1
            End If
        Next

        'there was a case where i had an infinte loop when there were 2 players
        'and one went all in (bigblind) and the smallblind couldnt match the bet because of this so i added the AndAlso
        'the AndAlso checks to see if every player is either all in, out, folded or has matched the currentbet, before we can skip to the showdown
        If count = 1 AndAlso Table.All(Function(item) item.IsAllIn = True OrElse item.HasFolded = True OrElse item.IsOut = True OrElse item.CurrentBet >= tempCurrentHighestBet) Then
            GetNextPlayer(NextPlayer, Table)
            Exit Sub
        End If

        If Not Table(NextPlayer).HasFolded Then 'Just making sure the next player hasn't folded
            If TypeOf Table(NextPlayer) Is Computer Then 'If the next player is a computer its a little different so we have to do this
                Dim computer As Computer = CType(Table(NextPlayer), Computer)
                Dim position As Integer = -1
                If GameState <> "Pre-Flop" Then
                    position = GetComputerPosition(computer, Table, Dealer)
                End If

                computer.GetChoice(CurrentHighestBet, GamePot, GameState, CommunityCards, Players, position)
                'If they have folded we update their HasFolded and go to the next player
                'show they have folded on the UI
                If computer.ComputerChoice = "fold" Then
                    computer.DoChoice(CurrentHighestBet, GamePot)
                    ShowTheFold(Table(NextPlayer))
                    GetNextPlayer(NextPlayer, Table)
                    Exit Sub
                End If
                computer.DoChoice(CurrentHighestBet, GamePot)
                If computer.ComputerChoice <> "skip" Then
                    UpdateComputerChoice(computer)
                End If

            ElseIf TypeOf Table(NextPlayer) Is Player Then
                Dim Decision As Decision = GetPlayerChoice(Table(NextPlayer), CurrentHighestBet)
                If Decision.Input = "fold" Then
                    Table(NextPlayer).DoChoice(CurrentHighestBet, GamePot, Decision)
                    ShowTheFold(Table(NextPlayer))
                    GetNextPlayer(NextPlayer, Table)
                    PlayerInput = Nothing
                    PlayerInputNumber = Nothing
                    Exit Sub
                End If
                Table(NextPlayer).DoChoice(CurrentHighestBet, GamePot, Decision)
                PlayerInput = Nothing
                PlayerInputNumber = Nothing
            End If
        End If
        'if they folded we go on to the next player anyway
        GetNextPlayer(NextPlayer, Table)
    End Sub
    Structure Decision
        Public Input As String
        Public Amount As Integer
    End Structure
    Public Function GetPlayerChoice(Player As Player, CurrentHighestBet As Integer) As Decision
        Dim Decision As New Decision
        'Doing OrElse IsOut because if they are out I dont want to remove them from the game table
        'getting the new blind players would be difficult so if they cannot pay the blinds
        'we will skip them and update the UI to show they are out
        If Player.IsAllIn OrElse (Player.CurrentBet >= CurrentHighestBet And CurrentHighestBet <> 0) OrElse Player.IsOut Then
            Decision.Input = "skip"
        Else
            If Player.CurrentBet = CurrentHighestBet Then
                Decision = GetBetCheck(Player)
            ElseIf Player.CurrentBet < CurrentHighestBet Then
                Decision = GetRaiseCallFold(Player, CurrentHighestBet)
            End If
        End If
        Return Decision
    End Function
    Public Sub UpdateComputerChoice(Computer As Computer)
        Select Case Computer.Number
            Case 1
                TxtComputer1Choice.Text = Computer.ComputerChoice
            Case 2
                TxtComputer2Choice.Text = Computer.ComputerChoice
            Case 3
                TxtComputer3Choice.Text = Computer.ComputerChoice
            Case 4
                TxtComputer4Choice.Text = Computer.ComputerChoice
        End Select
        Application.DoEvents()
    End Sub
    Public Sub HideComputerChoice()
        Thread.Sleep(200)
        TxtComputer1Choice.Text = ""
        TxtComputer2Choice.Text = ""
        TxtComputer3Choice.Text = ""
        TxtComputer4Choice.Text = ""
        Application.DoEvents()
    End Sub
    Public Function GetBetCheck(Player As Player) As Decision
        'Gets user decision from the UI
        Dim Decision As New Decision
        'Shows the two buttons
        ShowBetCheck()

        'Loop until one of the buttons is clicked
        Do Until Decision.Input <> ""
            Application.DoEvents() 'Allow the UI to respond
            'Check if the Fold button is clicked
            If PlayerInput = "check" Then
                Decision.Input = "check"
            End If

            'Check if the Bet button is clicked
            If PlayerInput = "bet" Then
                Decision.Input = "bet"
                PlayerInputNumber = Val(TxtBetAmount.Text)
                Decision.Amount = PlayerInputNumber

                If Decision.Amount < BigBlind Then
                    txtPlayerChoice.Show()
                    txtPlayerChoice.Text = "The minimum bet for this game is 2 chips"
                    Decision.Input = ""
                    PlayerInput = ""

                ElseIf Decision.Amount > Player.Chips Then
                    txtPlayerChoice.Show()
                    txtPlayerChoice.Text = "You do not have enough chips"
                    Decision.Input = ""
                    PlayerInput = ""

                ElseIf Decision.Amount = 0 Then
                    txtPlayerChoice.Show()
                    txtPlayerChoice.Text = "You cannot bet 0"
                    Decision.Input = ""
                    PlayerInput = ""
                End If
            End If
        Loop
        'Hide the buttons after a choice is made
        HideBetCheck()
        Return Decision
    End Function
    Public Function GetRaiseCallFold(Player As Player, CurrentHighestBet As Integer) As Decision
        Dim Decision As New Decision
        'Shows the two buttons
        ShowCallRaiseFold()
        'Loop until one of the buttons is clicked
        Do Until Decision.Input <> ""
            Application.DoEvents() 'Allow the UI to respond
            'Check if the Fold button is clicked
            If PlayerInput = "call" Then
                Decision.Input = "call"
            End If

            'Check if the Raise button is clicked
            If PlayerInput = "raise" Then
                Decision.Input = "raise"
                PlayerInputNumber = Val(TxtRaiseAmount.Text)
                Decision.Amount = PlayerInputNumber

                If Decision.Amount < CurrentHighestBet Then
                    txtPlayerChoice.Show()
                    txtPlayerChoice.Text = ($"Raise amount less than the current highest bet ({CurrentHighestBet})")
                    Decision.Input = ""
                    PlayerInput = ""

                ElseIf Decision.Amount > Player.Chips Then
                    txtPlayerChoice.Show()
                    txtPlayerChoice.Text = "You do not have enough chips"
                    Decision.Input = ""
                    PlayerInput = ""
                End If
            End If
            If PlayerInput = "fold" Then
                Decision.Input = "fold"
            End If
        Loop
        'Hide the buttons after a choice is made
        HideCallRaiseFold()
        Return Decision
    End Function
    Public Sub ShowBetCheck()
        BtnCheck.Show()
        BtnBet.Show()
        labelBet.Show()
        TxtBetAmount.Show()
        BtnCall.Hide()
        BtnRaise.Hide()
        TxtRaiseAmount.Hide()
        labelRaise.Hide()
        BtnFold.Hide()
    End Sub
    Public Sub ShowCallRaiseFold()
        BtnBet.Hide()
        labelBet.Hide()
        BtnCheck.Hide()
        TxtBetAmount.Hide()
        BtnRaise.Show()
        TxtRaiseAmount.Show()
        labelRaise.Show()
        BtnFold.Show()
        BtnCall.Show()
    End Sub
    Public Sub HideBetCheck()
        BtnBet.Hide()
        labelBet.Hide()
        TxtBetAmount.Hide()
        BtnCheck.Hide()
        txtPlayerChoice.Hide()
        Application.DoEvents()
    End Sub
    Public Sub HideCallRaiseFold()
        BtnCall.Hide()
        BtnRaise.Hide()
        TxtRaiseAmount.Hide()
        labelRaise.Hide()
        BtnFold.Hide()
        txtPlayerChoice.Hide()
        Application.DoEvents()
    End Sub
    Public Sub RefreshNumbers(Player As Player, Computer1 As Computer, Computer2 As Computer, Computer3 As Computer, Computer4 As Computer, GamePot As Integer)
        PlayerChips.Text = "Chips: " & Convert.ToString(Player.Chips)
        labelPlayerChipsIn.Text = "Chips put in this round: " & Convert.ToString(Player.CurrentBet)
        Computer1Chips.Text = "Chips: " & Convert.ToString(Computer1.Chips)
        labelComputer1ChipsIn.Text = "Chips put in this round: " & Convert.ToString(Computer1.CurrentBet)
        Computer2Chips.Text = "Chips: " & Convert.ToString(Computer2.Chips)
        labelComputer2ChipsIn.Text = "Chips put in this round: " & Convert.ToString(Computer2.CurrentBet)
        Computer3Chips.Text = "Chips: " & Convert.ToString(Computer3.Chips)
        labelComputer3ChipsIn.Text = "Chips put in this round: " & Convert.ToString(Computer3.CurrentBet)
        Computer4Chips.Text = "Chips: " & Convert.ToString(Computer4.Chips)
        labelComputer4ChipsIn.Text = "Chips put in this round: " & Convert.ToString(Computer4.CurrentBet)
        labelPot.Text = "Pot: " & Convert.ToString(GamePot)
        Application.DoEvents()
    End Sub
    Public Sub ResetFold(Player As Player)
        If TypeOf Player Is Computer Then
            Select Case CType(Player, Computer).Number
                Case 1
                    picComputer1Folded.Hide()
                Case 2
                    picComputer2Folded.Hide()
                Case 3
                    picComputer3Folded.Hide()
                Case 4
                    picComputer4Folded.Hide()
            End Select
        Else
            picPlayerFolded.Hide()
        End If
        Application.DoEvents()
    End Sub
    Public Sub ShowTheFold(Player As Player)
        If Player.HasFolded = True Then
            'change for fold
            If TypeOf Player Is Computer Then
                Select Case CType(Player, Computer).Number
                    Case 1
                        picComputer1Folded.Show()
                        TxtComputer1Choice.Text = ""
                    Case 2
                        picComputer2Folded.Show()
                        TxtComputer2Choice.Text = ""
                    Case 3
                        picComputer3Folded.Show()
                        TxtComputer3Choice.Text = ""
                    Case 4
                        picComputer4Folded.Show()
                        TxtComputer4Choice.Text = ""
                End Select
            Else
                picPlayerFolded.Show()
            End If
        Else
            'we do nothing
        End If
        Application.DoEvents()
    End Sub
    Public Sub ShowTheWin(Player As Player)
        If TypeOf Player Is Computer Then
            Select Case CType(Player, Computer).Number
                Case 1
                    picComputer1Win.Show()
                    picComputer1Card1.Hide()
                    picComputer1Card2.Hide()
                Case 2
                    picComputer2Win.Show()
                    picComputer2Card1.Hide()
                    picComputer2Card2.Hide()
                Case 3
                    picComputer3Win.Show()
                    picComputer3Card1.Hide()
                    picComputer3Card2.Hide()
                Case 4
                    picComputer4Win.Show()
                    picComputer4Card1.Hide()
                    picComputer4Card2.Hide()
            End Select
        Else
            picPlayerWin.Show()
            picPlayerHand1.Hide()
            picPlayerHand2.Hide()
        End If
    End Sub
    Public Sub ShowTheOut(Player As Player)
        If Player.IsOut Then
            If TypeOf Player Is Computer Then
                Select Case CType(Player, Computer).Number
                    Case 1
                        picComputer1Out.Show()
                    Case 2
                        picComputer2Out.Show()
                    Case 3
                        picComputer3Out.Show()
                    Case 4
                        picComputer4Out.Show()
                End Select
            Else
                picPlayerOut.Show()
            End If
        Else
            'we do nothing
        End If
    End Sub
    Sub DisplayCards(Player As Player)
        If TypeOf Player Is Computer Then
            Select Case CType(Player, Computer).Number
                Case 1
                    picComputer1Card1.Image = New Bitmap(".\Resources\" & Player.HoleCards(0).ToString() & ".png")
                    picComputer1Card2.Image = New Bitmap(".\Resources\" & Player.HoleCards(1).ToString() & ".png")
                Case 2
                    picComputer2Card1.Image = New Bitmap(".\Resources\" & Player.HoleCards(0).ToString() & ".png")
                    picComputer2Card2.Image = New Bitmap(".\Resources\" & Player.HoleCards(1).ToString() & ".png")
                Case 3
                    picComputer3Card1.Image = New Bitmap(".\Resources\" & Player.HoleCards(0).ToString() & ".png")
                    picComputer3Card2.Image = New Bitmap(".\Resources\" & Player.HoleCards(1).ToString() & ".png")
                Case 4
                    picComputer4Card1.Image = New Bitmap(".\Resources\" & Player.HoleCards(0).ToString() & ".png")
                    picComputer4Card2.Image = New Bitmap(".\Resources\" & Player.HoleCards(1).ToString() & ".png")
            End Select
        Else
            picPlayerHand1.Image = New Bitmap(".\Resources\" & Player.HoleCards(0).ToString() & ".png")
            picPlayerHand2.Image = New Bitmap(".\Resources\" & Player.HoleCards(1).ToString() & ".png")
        End If
        Application.DoEvents()
    End Sub
    Sub HideCards(Player As Player)
        If TypeOf Player Is Computer Then
            Select Case CType(Player, Computer).Number
                Case 1
                    picComputer1Card1.Image = HiddenCard
                    picComputer1Card2.Image = HiddenCard
                Case 2
                    picComputer2Card1.Image = HiddenCard
                    picComputer2Card2.Image = HiddenCard
                Case 3
                    picComputer3Card1.Image = HiddenCard
                    picComputer3Card2.Image = HiddenCard
                Case 4
                    picComputer4Card1.Image = HiddenCard
                    picComputer4Card2.Image = HiddenCard
            End Select
        Else
            picPlayerHand1.Image = HiddenCard
            picPlayerHand2.Image = HiddenCard
        End If
        Application.DoEvents()
    End Sub
    Shared Sub GetNextPlayer(ByRef NextPlayer As Integer, Table As List(Of Player))
        'Infinite loop to find the next player who hasnt folded and they are the next player
        While True
            NextPlayer += 1
            If NextPlayer > Table.Count - 1 Then
                NextPlayer = 0
            End If
            If Table(NextPlayer).HasFolded Then
            Else
                Exit While
            End If
        End While
    End Sub
    Public Shared Sub NextRound(ByRef NextPlayer As Integer, ByRef SmallBlindPlayer As Integer, ByRef BigBlindPlayer As Integer, ByRef Dealer As Integer, Table As List(Of Player))
        'If there are 2 players we alternate the smallblind and biglind players and the next player has to be the small blind
        If Table.Count = 2 Then
            Dealer += 1
            If Dealer > Table.Count - 1 Then
                Dealer = 0
                SmallBlindPlayer = 1
                BigBlindPlayer = 0
                NextPlayer = 1
            Else
                SmallBlindPlayer = 0
                NextPlayer = 0
                BigBlindPlayer = 1
            End If
        Else
            While True
                Dealer += 1
                If Dealer > Table.Count - 1 Then
                    Dealer = 0
                End If
                If Table(Dealer).HasFolded Then
                    'they folded so we continue to find the next active player
                Else
                    'we found the next player who hasnt folded so they are the dealer
                    Exit While
                End If
            End While

            SmallBlindPlayer = Dealer + 1
            While True
                If SmallBlindPlayer > Table.Count - 1 Then
                    SmallBlindPlayer = 0
                End If
                If Table(SmallBlindPlayer).HasFolded Then
                    'they folded so we continue to find the next active player
                Else
                    Exit While
                End If
                SmallBlindPlayer += 1
            End While

            BigBlindPlayer = SmallBlindPlayer + 1
            While True
                If BigBlindPlayer > Table.Count - 1 Then
                    BigBlindPlayer = 0
                End If
                If Table(BigBlindPlayer).HasFolded Then
                    'they folded so we continue to find the next active player
                Else
                    Exit While
                End If
                BigBlindPlayer += 1
            End While

            NextPlayer = BigBlindPlayer + 1
            While True
                If NextPlayer > Table.Count - 1 Then
                    NextPlayer = 0
                End If
                If Table(NextPlayer).HasFolded Then
                    'they folded so we continue to find the next active player
                Else
                    Exit While
                End If
                NextPlayer += 1
            End While
        End If
    End Sub
    Public Shared Sub GetWinner(Table As List(Of Player), ByRef Pot As Integer, CommunityCards As List(Of Card))
        Dim ShowDownPlayers As New List(Of Player)
        Dim Winners As New List(Of Player)

        'there was a case where a player had folded but still won so i have to check they havent folded here
        For Each Player In Table
            If Player.HasFolded = False Then
                ShowDownPlayers.Add(Player)
            End If
        Next

        For Each Player In ShowDownPlayers
            Player.EvaluateHand(CommunityCards)
        Next

        'The Player with the greatest handvalue will be at the end
        SortPlayers(ShowDownPlayers)
        Dim BestHandValue As Integer = ShowDownPlayers(0).HandValue
        For Each Player In ShowDownPlayers
            If Player.HandValue = BestHandValue Then
                Winners.Add(Player)
            End If
        Next

        If Winners.Count = 1 Then
            MsgBox($"{Winners(0).Name} has a {pokerHandValues(Winners(0).HandValue)}")

            MsgBox($"{Winners(0).Name} wins!!")
            Winners(0).Chips += Pot
            Pot = 0

        Else
            Dim Winner As List(Of Player) = Tiebreak(Winners, BestHandValue)
            If Winner.Count = 1 Then
                MsgBox($"{Winner(0).Name} has the best {pokerHandValues(Winner(0).HandValue)}")
                MsgBox($"{Winner(0).Name} wins!!")
                Winner(0).Chips += Pot
                Pot = 0

            Else
                Dim WinnersString As String = ""
                For p = 0 To Winner.Count - 2
                    WinnersString += Winner(p).Name + "," + " "
                Next
                WinnersString += $"{Winner(Winner.Count - 1).Name}"
                MsgBox($"{WinnersString} have an equal {pokerHandValues(Winner(0).HandValue)}")
                MsgBox("Tie")
                'for my version any left over chips stay in the pot for the next round
                Dim WinnerCount As Integer = Winner.Count
                Dim Payout As Integer = Pot \ WinnerCount
                Pot -= Payout * WinnerCount
                For Each Player In Winner
                    Player.Chips += Payout
                Next
            End If
        End If
    End Sub
    Public Shared Function Tiebreak(WinnersList As List(Of Player), BestHandValue As Integer) As List(Of Player)
        Dim Winner As New List(Of Player)
        Select Case BestHandValue
            Case 1 'Royal Flush
                Return WinnersList

            Case 2 'Straight Flush
                Dim HighestValue As Integer = 0
                For Each Player In WinnersList
                    If Player.BestHand(HighestCard).Value > HighestValue Then
                        HighestValue = Player.BestHand(HighestCard).Value
                        Winner.Clear()
                        Winner.Add(Player)
                    ElseIf Player.BestHand(HighestCard).Value = HighestValue Then
                        Winner.Add(Player)
                    End If
                Next

            Case 3 'Four of a Kind
                Dim HighestKind As Integer = 0
                Dim KickerCard As New Card
                Dim HighestKicker As Integer
                Dim FourOfAKindGroup As List(Of Card)
                For Each Player In WinnersList
                    FourOfAKindGroup = Player.BestHand.
                        GroupBy(Function(card) card.Rank).
                        OrderByDescending(Function(group) group.Count).
                        First.
                        ToList
                    KickerCard = Player.BestHand.Except(FourOfAKindGroup).Single
                    If FourOfAKindGroup(0).Value > HighestKind Then
                        HighestKind = FourOfAKindGroup(0).Value
                        Winner.Clear()
                        Winner.Add(Player)
                    ElseIf FourOfAKindGroup(0).Value = HighestKind Then
                        If KickerCard.Value > HighestKicker Then
                            HighestKicker = KickerCard.Value
                            Winner.Clear()
                            Winner.Add(Player)
                        ElseIf KickerCard.Value = HighestKicker Then
                            Winner.Add(Player)
                        End If
                    End If
                Next

            Case 4 'FullHouse
                Dim ThreeOfKind As New List(Of Card)
                Dim TwoOfKind As New List(Of Card)
                Dim Highest3 As Integer = 0
                Dim Highest2 As Integer = 0
                For Each Player In WinnersList
                    ThreeOfKind = Player.BestHand.GroupBy(Function(card) card.Rank).
                        OrderByDescending(Function(group) group.Count).
                        First.
                        ToList
                    TwoOfKind = Player.BestHand.Except(ThreeOfKind).ToList
                    If ThreeOfKind(0).Value > Highest3 Then
                        Highest3 = ThreeOfKind(0).Value
                        Highest2 = TwoOfKind(0).Value
                        Winner.Clear()
                        Winner.Add(Player)
                    ElseIf ThreeOfKind(0).Value = Highest3 Then
                        If TwoOfKind(0).Value > Highest2 Then
                            Highest2 = TwoOfKind(0).Value
                            Winner.Clear()
                            Winner.Add(Player)
                        ElseIf TwoOfKind(0).Value = Highest2 Then
                            Winner.Add(Player)
                        End If
                    End If
                Next

            Case 5 'Flush
                'Meaning the value of the highest card, second highest card etc etc
                Dim HighestValue As Integer = 0
                Dim FifthHighestValue As Integer = 0
                Dim SecondHighestValue As Integer = 0
                Dim ThirdHighestValue As Integer = 0
                Dim FourthHighestValue As Integer = 0
                For Each Player In WinnersList
                    If Player.BestHand(HighestCard).Value > HighestValue Then
                        HighestValue = Player.BestHand(HighestCard).Value
                        SecondHighestValue = Player.BestHand(SecondHighestCard).Value
                        ThirdHighestValue = Player.BestHand(ThirdHighestCard).Value
                        FourthHighestValue = Player.BestHand(FourthHighestCard).Value
                        FifthHighestValue = Player.BestHand(FifthHighestCard).Value
                        Winner.Clear()
                        Winner.Add(Player)

                    ElseIf Player.BestHand(HighestCard).Value = HighestValue Then
                        If Player.BestHand(SecondHighestCard).Value > SecondHighestValue Then
                            HighestValue = Player.BestHand(HighestCard).Value
                            SecondHighestValue = Player.BestHand(SecondHighestCard).Value
                            ThirdHighestValue = Player.BestHand(ThirdHighestCard).Value
                            FourthHighestValue = Player.BestHand(FourthHighestCard).Value
                            FifthHighestValue = Player.BestHand(FifthHighestCard).Value
                            Winner.Clear()
                            Winner.Add(Player)

                        ElseIf Player.BestHand(SecondHighestCard).Value = SecondHighestValue Then
                            If Player.BestHand(ThirdHighestCard).Value > ThirdHighestValue Then
                                HighestValue = Player.BestHand(HighestCard).Value
                                SecondHighestValue = Player.BestHand(SecondHighestCard).Value
                                ThirdHighestValue = Player.BestHand(ThirdHighestCard).Value
                                FourthHighestValue = Player.BestHand(FourthHighestCard).Value
                                FifthHighestValue = Player.BestHand(FifthHighestCard).Value
                                Winner.Clear()
                                Winner.Add(Player)

                            ElseIf Player.BestHand(ThirdHighestCard).Value = ThirdHighestValue Then
                                If Player.BestHand(FourthHighestCard).Value > FourthHighestValue Then
                                    HighestValue = Player.BestHand(HighestCard).Value
                                    SecondHighestValue = Player.BestHand(SecondHighestCard).Value
                                    ThirdHighestValue = Player.BestHand(ThirdHighestCard).Value
                                    FourthHighestValue = Player.BestHand(FourthHighestCard).Value
                                    FifthHighestValue = Player.BestHand(FifthHighestCard).Value
                                    Winner.Clear()
                                    Winner.Add(Player)

                                ElseIf Player.BestHand(FourthHighestCard).Value = FourthHighestValue Then
                                    If Player.BestHand(FifthHighestCard).Value > FifthHighestValue Then
                                        HighestValue = Player.BestHand(HighestCard).Value
                                        SecondHighestValue = Player.BestHand(SecondHighestCard).Value
                                        ThirdHighestValue = Player.BestHand(ThirdHighestCard).Value
                                        FourthHighestValue = Player.BestHand(FourthHighestCard).Value
                                        FifthHighestValue = Player.BestHand(FifthHighestCard).Value
                                        Winner.Clear()
                                        Winner.Add(Player)

                                    ElseIf Player.BestHand(FifthHighestCard).Value = FifthHighestValue Then
                                        Winner.Add(Player)
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next

            Case 6 'Straight
                Dim HighestValue As Integer = 0
                For Each Player In WinnersList
                    If Player.BestHand(HighestCard).Value > HighestValue Then
                        HighestValue = Player.BestHand(HighestCard).Value
                        Winner.Clear()
                        Winner.Add(Player)
                    ElseIf Player.BestHand(HighestCard).Value = HighestValue Then
                        Winner.Add(Player)
                    End If
                Next

            Case 7 'Three of a Kind
                Dim ThreeOfKindCards As List(Of Card)
                Dim KickerCards As List(Of Card)
                Dim HighestKickerCard As New Card
                Dim SecondHighestKickerCard As New Card
                Dim Highest3Value As Integer = 0
                Dim HighestKicker As Integer = 0
                Dim SecondHighestKicker As Integer = 0

                For Each Player In WinnersList
                    ThreeOfKindCards = Player.BestHand.GroupBy(Function(card) card.Rank).
        OrderByDescending(Function(group) group.Count).
        Where(Function(group) group.Count() >= 3).
        SelectMany(Function(group) group.Take(3)).ToList()
                    KickerCards = Player.BestHand.Except(ThreeOfKindCards).ToList()
                    SortCards(KickerCards)

                    If ThreeOfKindCards(0).Value > Highest3Value Then
                        Highest3Value = ThreeOfKindCards(0).Value
                        HighestKickerCard = KickerCards(1)
                        HighestKicker = HighestKickerCard.Value
                        SecondHighestKickerCard = KickerCards(0)
                        SecondHighestKicker = KickerCards(0).Value
                        Winner.Clear()
                        Winner.Add(Player)

                    ElseIf ThreeOfKindCards(0).Value = Highest3Value Then
                        If KickerCards(1).Value > HighestKicker Then
                            Highest3Value = ThreeOfKindCards(0).Value
                            HighestKickerCard = KickerCards(1)
                            HighestKicker = HighestKickerCard.Value
                            SecondHighestKickerCard = KickerCards(0)
                            SecondHighestKicker = KickerCards(0).Value
                            Winner.Clear()
                            Winner.Add(Player)

                        ElseIf KickerCards(1).Value = HighestKicker Then
                            If KickerCards(0).Value > SecondHighestKicker Then
                                Highest3Value = ThreeOfKindCards(0).Value
                                HighestKickerCard = KickerCards(1)
                                HighestKicker = HighestKickerCard.Value
                                SecondHighestKickerCard = KickerCards(0)
                                SecondHighestKicker = KickerCards(0).Value
                                Winner.Clear()
                                Winner.Add(Player)

                            ElseIf KickerCards(0).Value = SecondHighestKicker Then
                                Winner.Clear()
                            End If
                        End If
                    End If
                Next

            Case 8 'Two Pair
                'Pair1 has the higher pair, Kicker contains the non pair card
                Dim Pair1 As New List(Of Card)
                Dim HighestPairVal As Integer = 0
                Dim Pair2 As New List(Of Card)
                Dim SecondHighestPairVal As Integer = 0
                Dim Kicker As New Card
                Dim HighestKickerVal = 0
                Dim TwoPairGroups As IEnumerable(Of IGrouping(Of String, Card))
                Dim pair1Rank As String
                Dim pair2Rank As String

                For Each Player In WinnersList
                    TwoPairGroups = Player.BestHand.
                        GroupBy(Function(card) card.Rank).
        Where(Function(group) group.Count() = 2).
        OrderByDescending(Function(group) group.Key)

                    TwoPairGroups = TwoPairGroups.OrderByDescending(Function(group) CardValues(group.Key(0)))
                    pair1Rank = TwoPairGroups.First().Key
                    pair2Rank = TwoPairGroups.Skip(1).First().Key
                    Kicker = Player.BestHand.First(Function(card) card.Rank <> pair1Rank AndAlso card.Rank <> pair2Rank)
                    Pair1 = TwoPairGroups.First().ToList()
                    Pair2 = TwoPairGroups.Skip(1).First().ToList()

                    If Pair1(0).Value > HighestPairVal Then
                        HighestPairVal = Pair1(0).Value
                        SecondHighestPairVal = Pair2(0).Value
                        HighestKickerVal = Kicker.Value
                        Winner.Clear()
                        Winner.Add(Player)

                    ElseIf Pair1(0).Value = HighestPairVal Then
                        If Pair2(0).Value > SecondHighestPairVal Then
                            HighestPairVal = Pair1(0).Value
                            SecondHighestPairVal = Pair2(0).Value
                            HighestKickerVal = Kicker.Value
                            Winner.Clear()
                            Winner.Add(Player)

                        ElseIf Pair2(0).Value = SecondHighestPairVal Then
                            If Kicker.Value > HighestKickerVal Then
                                HighestPairVal = Pair1(0).Value
                                SecondHighestPairVal = Pair2(0).Value
                                HighestKickerVal = Kicker.Value
                                Winner.Clear()
                                Winner.Add(Player)
                            ElseIf Kicker.Value = HighestKickerVal Then
                                Winner.Add(Player)
                            End If
                        End If
                    End If
                Next

            Case 9 'One Pair
                Dim Pair As New List(Of Card)
                Dim NonPairs As New List(Of Card)
                Dim HighestKicker, SecondHighestKicker, ThirdHighestKicker As New Card
                Dim HighestPairVal As Integer = 0
                Dim HighestKickerVal As Integer = 0
                Dim SecondHighestKickerVal As Integer = 0
                Dim ThirdHighestKickerVal As Integer = 0

                For Each Player In WinnersList
                    Pair = Player.BestHand.GroupBy(Function(card) card.Rank).
                        OrderByDescending(Function(group) group.Count).
                        Where(Function(group) group.Count() = 2).
                        SelectMany(Function(group) group).ToList()
                    NonPairs = Player.BestHand.Except(Pair).ToList
                    SortCards(NonPairs)
                    HighestKicker = NonPairs(2)
                    SecondHighestKicker = NonPairs(1)
                    ThirdHighestKicker = NonPairs(0)

                    If Pair(0).Value > HighestPairVal Then
                        HighestPairVal = Pair(0).Value
                        HighestKickerVal = HighestKicker.Value
                        SecondHighestKickerVal = SecondHighestKicker.Value
                        ThirdHighestKickerVal = ThirdHighestKicker.Value
                        Winner.Clear()
                        Winner.Add(Player)

                    ElseIf Pair(0).Value = HighestPairVal Then
                        If HighestKicker.Value > HighestKickerVal Then
                            HighestPairVal = Pair(0).Value
                            HighestKickerVal = HighestKicker.Value
                            SecondHighestKickerVal = SecondHighestKicker.Value
                            ThirdHighestKickerVal = ThirdHighestKicker.Value
                            Winner.Clear()
                            Winner.Add(Player)

                        ElseIf HighestKicker.Value = HighestKickerVal Then
                            If SecondHighestKicker.Value > SecondHighestKickerVal Then
                                HighestPairVal = Pair(0).Value
                                HighestKickerVal = HighestKicker.Value
                                SecondHighestKickerVal = SecondHighestKicker.Value
                                ThirdHighestKickerVal = ThirdHighestKicker.Value
                                Winner.Clear()
                                Winner.Add(Player)

                            ElseIf SecondHighestKicker.Value = SecondHighestKickerVal Then
                                If ThirdHighestKicker.Value > ThirdHighestKickerVal Then
                                    HighestPairVal = Pair(0).Value
                                    HighestKickerVal = HighestKicker.Value
                                    SecondHighestKickerVal = SecondHighestKicker.Value
                                    ThirdHighestKickerVal = ThirdHighestKicker.Value
                                    Winner.Clear()
                                    Winner.Add(Player)

                                ElseIf ThirdHighestKicker.Value = ThirdHighestKickerVal Then
                                    Winner.Add(Player)
                                End If
                            End If
                        End If
                    End If
                Next

            Case 10 'High Card
                Dim HighestCardValue As Integer = 0
                Dim SecondHighestCardValue As Integer = 0
                Dim ThirdHighestCardValue As Integer = 0
                Dim FourthHighestCardValue As Integer = 0
                Dim FifthHighestCardValue As Integer = 0

                For Each Player In WinnersList
                    If Player.BestHand(HighestCard).Value > HighestCardValue Then
                        HighestCardValue = Player.BestHand(HighestCard).Value
                        SecondHighestCardValue = Player.BestHand(SecondHighestCard).Value
                        ThirdHighestCardValue = Player.BestHand(ThirdHighestCard).Value
                        FourthHighestCardValue = Player.BestHand(FourthHighestCard).Value
                        FifthHighestCardValue = Player.BestHand(FifthHighestCard).Value
                        Winner.Clear()
                        Winner.Add(Player)

                    ElseIf Player.BestHand(HighestCard).Value = HighestCardValue Then
                        If Player.BestHand(SecondHighestCard).Value > SecondHighestCardValue Then
                            HighestCardValue = Player.BestHand(HighestCard).Value
                            SecondHighestCardValue = Player.BestHand(SecondHighestCard).Value
                            ThirdHighestCardValue = Player.BestHand(ThirdHighestCard).Value
                            FourthHighestCardValue = Player.BestHand(FourthHighestCard).Value
                            FifthHighestCardValue = Player.BestHand(FifthHighestCard).Value
                            Winner.Clear()
                            Winner.Add(Player)

                        ElseIf Player.BestHand(SecondHighestCard).Value = SecondHighestCardValue Then
                            If Player.BestHand(ThirdHighestCard).Value > ThirdHighestCardValue Then
                                HighestCardValue = Player.BestHand(HighestCard).Value
                                SecondHighestCardValue = Player.BestHand(SecondHighestCard).Value
                                ThirdHighestCardValue = Player.BestHand(ThirdHighestCard).Value
                                FourthHighestCardValue = Player.BestHand(FourthHighestCard).Value
                                FifthHighestCardValue = Player.BestHand(FifthHighestCard).Value
                                Winner.Clear()
                                Winner.Add(Player)

                            ElseIf Player.BestHand(ThirdHighestCard).Value = ThirdHighestCardValue Then
                                If Player.BestHand(FourthHighestCard).Value > FourthHighestCardValue Then
                                    HighestCardValue = Player.BestHand(HighestCard).Value
                                    SecondHighestCardValue = Player.BestHand(SecondHighestCard).Value
                                    ThirdHighestCardValue = Player.BestHand(ThirdHighestCard).Value
                                    FourthHighestCardValue = Player.BestHand(FourthHighestCard).Value
                                    FifthHighestCardValue = Player.BestHand(FifthHighestCard).Value
                                    Winner.Clear()
                                    Winner.Add(Player)

                                ElseIf Player.BestHand(FourthHighestCard).Value = FourthHighestCardValue Then
                                    If Player.BestHand(FifthHighestCard).Value > FifthHighestCardValue Then
                                        HighestCardValue = Player.BestHand(HighestCard).Value
                                        SecondHighestCardValue = Player.BestHand(SecondHighestCard).Value
                                        ThirdHighestCardValue = Player.BestHand(ThirdHighestCard).Value
                                        FourthHighestCardValue = Player.BestHand(FourthHighestCard).Value
                                        FifthHighestCardValue = Player.BestHand(FifthHighestCard).Value
                                        Winner.Clear()
                                        Winner.Add(Player)

                                    ElseIf Player.BestHand(FifthHighestCard).Value = FifthHighestCardValue Then
                                        Winner.Add(Player)
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
        End Select
        Return Winner
    End Function

    Public Shared ReadOnly pokerHandValues As New Dictionary(Of Integer, String) From {
        {1, "Royal Flush"},
        {2, "Straight Flush"},
        {3, "Four of a Kind"},
        {4, "Full House"},
        {5, "Flush"},
        {6, "Straight"},
        {7, "Three of a Kind"},
        {8, "Two Pair"},
        {9, "One Pair"},
        {10, "High Card"}
    }
    Public Shared ReadOnly CardValues As New Dictionary(Of String, Integer) From {
    {"2", 2},
    {"3", 3},
    {"4", 4},
    {"5", 5},
    {"6", 6},
    {"7", 7},
    {"8", 8},
    {"9", 9},
    {"1", 10},
    {"J", 11},
    {"Q", 12},
    {"K", 13},
    {"A", 14}
}
    'Generating a list of lists containing the combinations of the given list of r length
    Public Shared Function Combinations(iterable As List(Of Card), r As Integer) As List(Of List(Of Card))
        Dim result As New List(Of List(Of Card))(21)
        Dim n As Integer = iterable.Count
        If r > n Then
            Return result
        End If
        Dim indices(r - 1) As Integer
        For i As Integer = 0 To r - 1
            indices(i) = i
        Next
        While indices(0) <= n - r
            Dim combination As New List(Of Card)
            For index = 0 To indices.Length - 1
                combination.Add(iterable(indices(index)))
            Next
            result.Add(combination)
            Dim i As Integer = r - 1
            While i >= 0 AndAlso indices(i) = i + n - r
                i -= 1
            End While
            If i < 0 Then
                Exit While
            End If
            indices(i) += 1
            For j As Integer = i + 1 To r - 1
                indices(j) = indices(j - 1) + 1
            Next
        End While
        Return result
    End Function
    Public Shared Sub SortPlayers(list As List(Of Player))
        If list.Count > 1 Then
            Dim MiddleOfList As Integer = list.Count \ 2
            Dim LeftList As New List(Of Player)
            Dim RightList As New List(Of Player)

            For Player = 0 To MiddleOfList - 1
                LeftList.Add(list(Player))
            Next
            For Player = MiddleOfList To list.Count - 1
                RightList.Add(list(Player))
            Next
            SortPlayers(LeftList)
            SortPlayers(RightList)

            Dim i As Integer = 0
            Dim j As Integer = 0
            Dim k As Integer = 0
            While i < LeftList.Count And j < RightList.Count
                If LeftList(i).HandValue < RightList(j).HandValue Then
                    list(k) = LeftList(i)
                    i += 1
                    k += 1
                Else
                    list(k) = RightList(j)
                    j += 1
                    k += 1
                End If
            End While
            While i < LeftList.Count
                list(k) = LeftList(i)
                k += 1
                i += 1
            End While
            While j < RightList.Count
                list(k) = RightList(j)
                j += 1
                k += 1
            End While
        End If
    End Sub
    Public Shared Sub SortCards(list As List(Of Card))
        'Merge sort
        'Code for sorting a list of cards by value (ascending order)
        'Only does something if the given list is longer than 1 card
        If list.Count > 1 Then
            Dim MiddleOfList As Integer = list.Count \ 2
            Dim LeftList As New List(Of Card)
            Dim RightList As New List(Of Card)
            'Splits the input list into 2
            For Card = 0 To MiddleOfList - 1
                LeftList.Add(list(Card))
            Next
            For Card = MiddleOfList To list.Count - 1
                RightList.Add(list(Card))
            Next
            'Recursion
            'Do the sort algorithm on the first half
            SortCards(LeftList)
            'Do the sort algorithm on the second half
            SortCards(RightList)
            'After this the first half of list and second half of the list are sorted
            'Merge
            'Left most element in left list
            Dim i As Integer = 0
            'Left most element in right list
            Dim j As Integer = 0
            'Left most element in merged list
            Dim k As Integer = 0
            While i < LeftList.Count And j < RightList.Count
                'Compares the value of the card at i in the left list with the value of the card at j in the right list
                'If the value of the card in the left array is smaller we put that card first in the merged array
                If LeftList(i).Value < RightList(j).Value Then
                    list(k) = LeftList(i)
                    'Move onto the next items in the respective lists
                    i += 1
                    k += 1
                    'If the value of the card in the right array is smaller we put that card first in the merged array
                Else
                    list(k) = RightList(j)
                    'Move onto the next items in the respective lists
                    j += 1
                    k += 1
                End If
            End While
            'If we have transferred everything from the right array and there are still cards in the left array
            'We transfer everything from the left array into the merged array
            While i < LeftList.Count
                list(k) = LeftList(i)
                k += 1
                i += 1
            End While
            'Same thing
            While j < RightList.Count
                list(k) = RightList(j)
                j += 1
                k += 1
            End While
        End If
    End Sub
    Public Class Card
        Private stSuit As String
        Private stRank As String
        Private stValue As Integer
        Public Property Rank As String
            Set(value As String)
                Dim Ranks() As String = {"Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King"}
                If Ranks.Contains(value) = False Then
                    MsgBox("Invalid rank for a card")
                Else
                    stRank = value
                End If

            End Set
            Get
                Return stRank
            End Get
        End Property
        Public Property Suit As String
            Set(value As String)
                Dim Suits() As String = {"Spades", "Hearts", "Diamonds", "Clubs"}
                If Suits.Contains(value) = False Then
                    MsgBox("Invalid suit for a card")
                Else
                    stSuit = value
                End If

            End Set
            Get
                Return stSuit
            End Get
        End Property
        Public Property Value As Integer
            Set(value As Integer)
                If value < 1 Or value > 14 Then
                    MsgBox("Invalid value for a poker card")
                Else
                    stValue = value
                End If
            End Set
            Get
                Return stValue
            End Get
        End Property
        'Code for printing out the string representation of cards
        Public Overrides Function ToString() As String
            Return stRank & " of " & stSuit
        End Function
    End Class
    Public Class Deck
        Private ReadOnly Cards As New List(Of Card)
        Public Property Count
            Set(value)
                MsgBox("You cannot set the amount of cards in the deck")
            End Set
            Get
                Return Cards.Count
            End Get
        End Property
        Public Sub New()
            'Creates Deck
            Dim Suits() As String
            Suits = {"Spades", "Hearts", "Diamonds", "Clubs"}
            Dim Ranks() As String
            Ranks = {"Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King"}
            For s = 0 To Suits.Length - 1
                For r = 0 To Ranks.Length - 1
                    Dim card As New Card With {
                        .Suit = Suits(s),
                        .Rank = Ranks(r),
                        .Value = CardValues(Ranks(r)(0))
                    }
                    Cards.Add(card)
                Next
            Next
            Shuffle()
        End Sub

        Function Shuffle()
            'FIsher yates algorithm
            Dim temp As Card
            For index = Cards.Count - 1 To 1 Step -1
                Dim j As Integer = Rand.Next(0, index + 1)
                temp = Cards(j)
                Cards(j) = Cards(index)
                Cards(index) = temp
            Next
            Return Cards
        End Function
        Function DealCard(hand As List(Of Card)) As List(Of Card)
            'Adding a card to the hand from the beginning of the deck
            hand.Add(Cards(0))
            Cards.RemoveAt(0)
            Return hand
        End Function
        Function Deal2Cards(hand As List(Of Card)) As List(Of Card)
            hand.Add(Cards(0))
            Cards.RemoveAt(0)
            hand.Add(Cards(0))
            Cards.RemoveAt(0)
            Return hand
        End Function
        Function Deal3Cards(hand As List(Of Card)) As List(Of Card)
            hand.Add(Cards(0))
            Cards.RemoveAt(0)
            hand.Add(Cards(0))
            Cards.RemoveAt(0)
            hand.Add(Cards(0))
            Cards.RemoveAt(0)
            Return hand
        End Function
        Sub RemoveCard(card As Card)
            ' Find the index of the card in the deck
            Dim index As Integer = Cards.FindIndex(Function(c) c.Suit = card.Suit AndAlso c.Rank = card.Rank)
            ' If the card is found, remove it from the deck
            If index >= 0 Then
                Cards.RemoveAt(index)
            End If
        End Sub
    End Class
    Class Player
        Public Name As String
        Protected _HoleCards As New List(Of Card)
        Protected _BestHand As New List(Of Card)
        Protected intHandValue As Integer
        Protected intChips As Integer = 100
        Protected intCurrentBet As Integer = 0
        Protected boolHasFolded As Boolean = False
        Protected boolIsAllIn As Boolean = False
        Protected boolIsOut As Boolean = False
        Public Property IsAllIn As Boolean
            Set(value As Boolean)
                MsgBox("You cant set this property")
            End Set
            Get
                Return boolIsAllIn
            End Get
        End Property
        Public Property IsOut As Boolean
            Set(value As Boolean)
                If intChips <> 0 And value = True Then
                    MsgBox("Invalid use of property")
                Else
                    boolIsOut = value
                End If
            End Set
            Get
                Return boolIsOut
            End Get
        End Property
        Public Property HasFolded As Boolean
            Set(value As Boolean)
                MsgBox("You cannot set whether someone has folded")
            End Set
            Get
                Return boolHasFolded
            End Get
        End Property
        Public Property BestHand As List(Of Card)
            Set(value As List(Of Card))
                MsgBox("You cannot set the besthand of a player/computer")
            End Set
            Get
                Return _BestHand
            End Get
        End Property
        Public Property HoleCards As List(Of Card)
            Get
                Return _HoleCards
            End Get
            Set(value As List(Of Card))
                If value.Count > 2 Then
                    MsgBox("you cannot set the holecards to something greater than 2 cards")
                Else
                    _HoleCards = value
                End If

            End Set
        End Property
        Public Property HandValue As Integer
            Set(value As Integer)
                MsgBox("You cannot set the value of a players hand")
            End Set
            Get
                Return intHandValue
            End Get
        End Property
        Public Property CurrentBet As Integer
            Get
                Return intCurrentBet
            End Get
            Set(value As Integer)
                MsgBox("You cannot set the current bet of a player/computer")
            End Set
        End Property
        Public Property Chips As Integer
            Set(value As Integer)
                If value > 500 Or value < 0 Then
                    MsgBox("Chips were set to an invalid amount")
                    End
                Else
                    intChips = value
                End If
            End Set
            Get
                Return intChips
            End Get
        End Property
        Public Sub PaySmallBlind(ByRef Pot As Integer)
            If intChips <= SmallBlind Then
                'current highest bet = 2 at the start anyway
                AllIn(Pot, CurrentHighestBet:=2)
            Else
                Pot += SmallBlind
                intChips -= SmallBlind
                intCurrentBet = SmallBlind
            End If
        End Sub
        Public Sub PayBigBlind(ByRef Pot As Integer)
            If intChips <= BigBlind Then
                AllIn(Pot, CurrentHighestBet:=2)
            Else
                Pot += BigBlind
                intChips -= BigBlind
                intCurrentBet = BigBlind
            End If
        End Sub
        Public Overridable Sub ResetAttributes()
            intCurrentBet = 0
            _BestHand = New List(Of Card)
            _HoleCards = New List(Of Card)
            intHandValue = Nothing
            boolHasFolded = False
            boolIsAllIn = False
        End Sub
        Public Sub ResetCurrentBet()
            intCurrentBet = 0
        End Sub
        Public Sub DoChoice(ByRef CurrentHighestBet As Integer, ByRef Pot As Integer, Decision As Decision)
            Dim Amount As Integer = Decision.Amount
            Select Case Decision.Input
                Case "skip"
                    Exit Sub
                Case "raise"
                    Dim AmountToPut = Amount - intCurrentBet
                    If intChips - AmountToPut = 0 Then
                        AllIn(Pot, CurrentHighestBet)
                    Else
                        Raise(Pot, CurrentHighestBet, Amount)
                    End If
                Case "call"
                    Dim AmountToCall As Integer = CurrentHighestBet - intCurrentBet
                    If AmountToCall >= intChips Then
                        AllIn(Pot, CurrentHighestBet)
                    Else
                        CallBet(Pot, CurrentHighestBet)
                    End If
                Case "check"
                Case "all in"
                    AllIn(Pot, CurrentHighestBet)
                Case "bet"
                    If intChips - Amount = 0 Then
                        AllIn(Pot, CurrentHighestBet)
                    Else
                        Bet(Pot, CurrentHighestBet, Amount)
                    End If
                Case "fold"
                    boolHasFolded = True
                Case Else
                    MsgBox("Invalid Choice error")
                    End
            End Select
        End Sub
        Protected Sub Raise(ByRef Pot As Integer, ByRef CurrentHighestBet As Integer, Amount As Integer)
            Dim AmountToPut = Amount - intCurrentBet
            intChips -= AmountToPut
            Pot += AmountToPut
            intCurrentBet = Amount
            If intCurrentBet > CurrentHighestBet Then
                CurrentHighestBet = Amount
            End If
        End Sub
        Protected Overridable Sub AllIn(ByRef Pot As Integer, ByRef CurrentHighestBet As Integer)
            intCurrentBet += intChips
            Pot += intChips
            intChips = 0
            boolIsAllIn = True
            If intCurrentBet > CurrentHighestBet Then
                CurrentHighestBet = intCurrentBet
            End If
        End Sub
        Protected Overridable Sub CallBet(ByRef Pot As Integer, ByRef CurrentHighestBet As Integer)
            Dim Amount = CurrentHighestBet - intCurrentBet
            If Amount > intChips Then
                intCurrentBet = intChips
                intChips = 0
                Pot += Amount
            Else
                intCurrentBet = CurrentHighestBet
                Pot += Amount
                intChips -= Amount
            End If
            If intChips = 0 Then
                boolIsAllIn = True
            End If
        End Sub
        Private Sub Bet(ByRef Pot As Integer, ByRef CurrentHighestBet As Integer, Amount As Integer)
            Pot += Amount
            intChips -= Amount
            intCurrentBet += Amount
            If intCurrentBet > CurrentHighestBet Then
                CurrentHighestBet = Amount
            End If
        End Sub
        Protected Shared Sub CheckRoyalFlush(Hand As List(Of Card), FirstCardSuit As String, ByRef RoyalFlushHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If Hand.All(Function(card) card.Suit = FirstCardSuit AndAlso card.Value >= 10) Then
                RoyalFlushHands.Add(Hand)
                valueOfHand = 1
            End If
        End Sub
        Protected Shared Sub CheckStraightFlush(Hand As List(Of Card), FirstCardSuit As String, ByRef StraightFlushHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If valueOfHand > 1 Then
                'Need to check if the last card is the same as the first because it wont be correct otherwise
                If FirstCardSuit = Hand(4).Suit Then
                    Dim StraightFlushConditionMetCounter As Integer = 0
                    For Card = 0 To Hand.Count - 2
                        If Hand(Card).Suit = FirstCardSuit AndAlso Hand(Card).Value + 1 = Hand(Card + 1).Value Then
                            StraightFlushConditionMetCounter += 1
                        End If
                    Next
                    If StraightFlushConditionMetCounter = 4 Then
                        StraightFlushHands.Add(Hand)
                        valueOfHand = 2
                    ElseIf Hand(0).Rank = "2" AndAlso Hand(1).Rank = "3" Then
                        If Hand(2).Rank = "4" AndAlso Hand(3).Rank = "5" Then
                            If Hand(4).Rank = "Ace" AndAlso Hand.All(Function(card) card.Suit = FirstCardSuit) Then
                                StraightFlushHands.Add(Hand)
                                valueOfHand = 2
                            End If
                        End If
                    End If
                End If
            Else
            End If
        End Sub
        Protected Shared Sub CheckFourOf(Hand As List(Of Card), ByRef FourOfAKindHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If valueOfHand > 2 Then
                If Hand.
                    GroupBy(Function(card) card.Rank).
                    OrderByDescending(Function(group) group.Count).
                    First().Count() = 4 Then
                    FourOfAKindHands.Add(Hand)
                    valueOfHand = 3
                End If
            Else
            End If
        End Sub
        Protected Shared Sub CheckFullHouse(Hand As List(Of Card), ByRef FullHouseHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            'Groups the cards by rank
            'Orders them into 3,2 list of cards
            'If the first group has 3 cards and the second group has 2 cards and in each group they have the same suit
            'It is a full House
            If valueOfHand > 3 AndAlso Hand.GroupBy(Function(card) card.Rank).
        OrderByDescending(Function(group) group.Count()).
        Select(Function(group) group).First().Count() = 3 AndAlso
        Hand.GroupBy(Function(card) card.Rank).
        OrderByDescending(Function(group) group.Count()).
        Select(Function(group) group).
        Skip(1).First().Count() = 2 Then
                FullHouseHands.Add(Hand)
                valueOfHand = 4
            End If
        End Sub
        Protected Shared Sub CheckFlush(Hand As List(Of Card), FirstCardSuit As String, ByRef FlushHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If valueOfHand > 4 AndAlso Hand.All(Function(card) card.Suit = FirstCardSuit) Then
                valueOfHand = 5
                FlushHands.Add(Hand)
            End If
        End Sub
        Protected Shared Sub CheckStraight(Hand As List(Of Card), ByRef StraightHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If valueOfHand > 5 Then
                Dim StraightConditionMetCounter = 0
                For Card = 0 To Hand.Count - 2
                    If Hand(Card).Value + 1 = Hand(Card + 1).Value Then
                        StraightConditionMetCounter += 1
                    End If
                Next
                If StraightConditionMetCounter = 4 Then
                    StraightHands.Add(Hand)
                    valueOfHand = 6
                ElseIf Hand(0).Rank = "2" AndAlso Hand(1).Rank = "3" Then
                    If Hand(2).Rank = "4" AndAlso Hand(3).Rank = "5" Then
                        If Hand(4).Rank = "Ace" Then
                            StraightHands.Add(Hand)
                            valueOfHand = 6
                        End If
                    End If
                End If
            Else
            End If
        End Sub
        Protected Shared Sub CheckThreeOf(Hand As List(Of Card), ByRef ThreeOfAKindHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If valueOfHand > 6 Then
                If Hand.
                    GroupBy(Function(card) card.Rank).
                    OrderByDescending(Function(group) group.Count()).
                    First().Count() = 3 Then
                    ThreeOfAKindHands.Add(Hand)
                    valueOfHand = 7
                End If
            Else
            End If
        End Sub
        Protected Shared Sub CheckTwoPair(Hand As List(Of Card), ByRef TwoPairHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If valueOfHand > 7 Then
                If Hand.
                   GroupBy(Function(card) card.Rank).
                   OrderByDescending(Function(group) group.Count()).First().Count() = 2 AndAlso
                   Hand.
                   GroupBy(Function(card) card.Rank).
                   OrderByDescending(Function(group) group.Count()).Skip(1).First().Count() = 2 Then
                    TwoPairHands.Add(Hand)
                    valueOfHand = 8
                End If
            End If
        End Sub
        Protected Shared Sub CheckOnePair(Hand As List(Of Card), ByRef OnePairHands As List(Of List(Of Card)), ByRef valueOfHand As Integer)
            If valueOfHand > 8 Then
                If Hand.
                  GroupBy(Function(card) card.Rank).
                  OrderByDescending(Function(group) group.Count()).First.Count() = 2 Then
                    OnePairHands.Add(Hand)
                    valueOfHand = 9
                End If
            Else
            End If
        End Sub
        Protected Shared Sub CheckHighCard(Hand As List(Of Card), ByRef HighestCardHand As List(Of Card), ByRef valueOfHand As Integer, ByRef HighestCardValue As Integer, ByRef SecondHighestCardValue As Integer, ByRef ThirdHighestCardValue As Integer, ByRef FourthHighestCardValue As Integer, ByRef FifthHighestCardValue As Integer)
            If valueOfHand > 9 AndAlso Hand(HighestCard).Value > HighestCardValue Then
                HighestCardValue = Hand(HighestCard).Value
                SecondHighestCardValue = Hand(SecondHighestCard).Value
                ThirdHighestCardValue = Hand(ThirdHighestCard).Value
                FourthHighestCardValue = Hand(FourthHighestCard).Value
                FifthHighestCardValue = Hand(FifthHighestCard).Value
                HighestCardHand = Hand
                valueOfHand = 10

            ElseIf valueOfHand > 9 AndAlso Hand(HighestCard).Value = HighestCardValue Then
                If Hand(SecondHighestCard).Value > HighestCardHand(SecondHighestCard).Value Then
                    HighestCardValue = Hand(HighestCard).Value
                    SecondHighestCardValue = Hand(SecondHighestCard).Value
                    ThirdHighestCardValue = Hand(ThirdHighestCard).Value
                    FourthHighestCardValue = Hand(FourthHighestCard).Value
                    FifthHighestCardValue = Hand(FifthHighestCard).Value
                    HighestCardHand = Hand

                ElseIf Hand(SecondHighestCard).Value = HighestCardHand(SecondHighestCard).Value Then
                    If Hand(ThirdHighestCard).Value > HighestCardHand(ThirdHighestCard).Value Then
                        HighestCardValue = Hand(HighestCard).Value
                        SecondHighestCardValue = Hand(SecondHighestCard).Value
                        ThirdHighestCardValue = Hand(ThirdHighestCard).Value
                        FourthHighestCardValue = Hand(FourthHighestCard).Value
                        FifthHighestCardValue = Hand(FifthHighestCard).Value
                        HighestCardHand = Hand

                    ElseIf Hand(ThirdHighestCard).Value = HighestCardHand(ThirdHighestCard).Value Then
                        If Hand(FourthHighestCard).Value > HighestCardHand(FourthHighestCard).Value Then
                            HighestCardValue = Hand(HighestCard).Value
                            SecondHighestCardValue = Hand(SecondHighestCard).Value
                            ThirdHighestCardValue = Hand(ThirdHighestCard).Value
                            FourthHighestCardValue = Hand(FourthHighestCard).Value
                            FifthHighestCardValue = Hand(FifthHighestCard).Value
                            HighestCardHand = Hand

                        ElseIf Hand(FourthHighestCard).Value = HighestCardHand(FourthHighestCard).Value Then
                            If Hand(FifthHighestCard).Value > HighestCardHand(FifthHighestCard).Value Then
                                HighestCardValue = Hand(HighestCard).Value
                                SecondHighestCardValue = Hand(SecondHighestCard).Value
                                ThirdHighestCardValue = Hand(ThirdHighestCard).Value
                                FourthHighestCardValue = Hand(FourthHighestCard).Value
                                FifthHighestCardValue = Hand(FifthHighestCard).Value
                                HighestCardHand = Hand
                            End If
                        End If
                    End If
                End If
            End If
        End Sub

        Protected Sub GetBestStraightFlush(StraightFlushHands As List(Of List(Of Card)), ByRef HighestValue As Integer)
            For pos = 0 To StraightFlushHands.Count - 1
                If StraightFlushHands(pos)(HighestCard).Value > HighestValue Then
                    _BestHand = StraightFlushHands(pos)
                    HighestValue = StraightFlushHands(pos)(HighestCard).Value
                End If
            Next
        End Sub
        Protected Sub GetBestFourOf(FourOfAKindHands As List(Of List(Of Card)), ByRef HighestKind As Integer, ByRef HighestKicker As Integer)
            Dim FourOfAKindGroup As List(Of Card)
            Dim KickerCard As New Card
            For Each list In FourOfAKindHands
                FourOfAKindGroup = list.GroupBy(Function(card) card.Rank).
                    OrderByDescending(Function(group) group.Count).
                    First().
                    ToList()
                KickerCard = list.Except(FourOfAKindGroup).Single
                If FourOfAKindGroup(0).Value > HighestKind Then
                    HighestKind = FourOfAKindGroup(0).Value
                    _BestHand = list
                ElseIf FourOfAKindGroup(0).Value = HighestKind Then
                    If KickerCard.Value > HighestKicker Then
                        HighestKicker = KickerCard.Value
                        _BestHand = list
                    End If
                End If
            Next
        End Sub
        Protected Sub GetBestFullHouse(FullHouseHands As List(Of List(Of Card)), ByRef Highest3 As Integer, ByRef Highest2 As Integer)
            Dim ThreeOfKind As New List(Of Card)
            Dim TwoOfKind As New List(Of Card)
            For Each list In FullHouseHands
                ThreeOfKind = list.GroupBy(Function(card) card.Rank).
                    OrderByDescending(Function(group) group.Count).
                    First().
                    ToList()
                If ThreeOfKind(0).Value > Highest3 Then
                    Highest3 = ThreeOfKind(0).Value
                    _BestHand = list
                ElseIf ThreeOfKind(0).Value = Highest3 Then
                    TwoOfKind = list.Except(ThreeOfKind).ToList
                    If TwoOfKind(0).Value > Highest2 Then
                        Highest2 = TwoOfKind(0).Value
                        _BestHand = list
                    End If
                End If
            Next
        End Sub
        Protected Sub GetBestFlush(FlushHands As List(Of List(Of Card)), ByRef HighestValue As Integer, ByRef SecondHighestValue As Integer, ByRef ThirdHighestValue As Integer, ByRef FourthHighestValue As Integer, ByRef FifthHighestValue As Integer)
            For Each list In FlushHands
                If list(HighestCard).Value > HighestValue Then
                    HighestValue = list(HighestCard).Value
                    SecondHighestValue = list(SecondHighestCard).Value
                    ThirdHighestValue = list(ThirdHighestCard).Value
                    FourthHighestValue = list(FourthHighestCard).Value
                    FifthHighestValue = list(FifthHighestCard).Value
                    _BestHand = list

                ElseIf list(HighestCard).Value = HighestValue Then
                    If list(SecondHighestCard).Value > SecondHighestValue Then
                        HighestValue = list(HighestCard).Value
                        SecondHighestValue = list(SecondHighestCard).Value
                        ThirdHighestValue = list(ThirdHighestCard).Value
                        FourthHighestValue = list(FourthHighestCard).Value
                        FifthHighestValue = list(FifthHighestCard).Value
                        _BestHand = list

                    ElseIf list(SecondHighestCard).Value = SecondHighestValue Then
                        If list(ThirdHighestCard).Value > ThirdHighestValue Then
                            HighestValue = list(HighestCard).Value
                            SecondHighestValue = list(SecondHighestCard).Value
                            ThirdHighestValue = list(ThirdHighestCard).Value
                            FourthHighestValue = list(FourthHighestCard).Value
                            FifthHighestValue = list(FifthHighestCard).Value
                            _BestHand = list

                        ElseIf list(ThirdHighestCard).Value = ThirdHighestValue Then
                            If list(FourthHighestCard).Value > FourthHighestValue Then
                                HighestValue = list(HighestCard).Value
                                SecondHighestValue = list(SecondHighestCard).Value
                                ThirdHighestValue = list(ThirdHighestCard).Value
                                FourthHighestValue = list(FourthHighestCard).Value
                                FifthHighestValue = list(FifthHighestCard).Value
                                _BestHand = list

                            ElseIf list(FourthHighestCard).Value = FourthHighestValue Then
                                If list(FifthHighestCard).Value > FifthHighestValue Then
                                    HighestValue = list(HighestCard).Value
                                    SecondHighestValue = list(SecondHighestCard).Value
                                    ThirdHighestValue = list(ThirdHighestCard).Value
                                    FourthHighestValue = list(FourthHighestCard).Value
                                    FifthHighestValue = list(FifthHighestCard).Value
                                    _BestHand = list
                                End If
                            End If
                        End If
                    End If
                End If
            Next
        End Sub
        Protected Sub GetBestStraight(StraightHands As List(Of List(Of Card)), ByRef HighestValue As Integer)
            For pos = 0 To StraightHands.Count - 1
                If StraightHands(pos)(HighestCard).Value > HighestValue Then
                    HighestValue = StraightHands(pos)(HighestCard).Value
                    _BestHand = StraightHands(pos)
                End If
            Next
        End Sub
        Protected Sub GetBestThreeOf(ThreeOfAKindHands As List(Of List(Of Card)), ByRef Highest3Value As Integer, ByRef HighestKicker As Integer, ByRef SecondHighestKicker As Integer, ByRef HighestKickerCard As Card, ByRef SecondHighestKickerCard As Card)
            Dim ThreeOfKindCards As List(Of Card)
            Dim KickerCards As List(Of Card)
            For pos = 0 To ThreeOfAKindHands.Count - 1
                ThreeOfKindCards = ThreeOfAKindHands(pos).
                    GroupBy(Function(card) card.Rank).
    OrderByDescending(Function(group) group.Count).
    Where(Function(group) group.Count() >= 3).
    SelectMany(Function(group) group.Take(3)).
    ToList()
                KickerCards = ThreeOfAKindHands(pos).Except(ThreeOfKindCards).ToList()
                SortCards(KickerCards)

                If ThreeOfKindCards(0).Value > Highest3Value Then
                    Highest3Value = ThreeOfKindCards(0).Value
                    HighestKickerCard = KickerCards(1)
                    HighestKicker = HighestKickerCard.Value
                    SecondHighestKickerCard = KickerCards(0)
                    SecondHighestKicker = KickerCards(0).Value
                    _BestHand = ThreeOfAKindHands(pos)

                ElseIf ThreeOfKindCards(0).Value = Highest3Value Then
                    If KickerCards(1).Value > HighestKicker Then
                        Highest3Value = ThreeOfKindCards(0).Value
                        HighestKickerCard = KickerCards(1)
                        HighestKicker = HighestKickerCard.Value
                        SecondHighestKickerCard = KickerCards(0)
                        SecondHighestKicker = KickerCards(0).Value
                        _BestHand = ThreeOfAKindHands(pos)

                    ElseIf KickerCards(1).Value = HighestKicker Then
                        If KickerCards(0).Value > SecondHighestKicker Then
                            Highest3Value = ThreeOfKindCards(0).Value
                            HighestKickerCard = KickerCards(1)
                            HighestKicker = HighestKickerCard.Value
                            SecondHighestKickerCard = KickerCards(0)
                            SecondHighestKicker = KickerCards(0).Value
                            _BestHand = ThreeOfAKindHands(pos)
                        End If
                    End If
                End If
            Next
        End Sub
        Protected Sub GetBestTwoPair(TwoPairHands As List(Of List(Of Card)), ByRef HighestPairVal As Integer, ByRef SecondHighestPairVal As Integer, ByRef HighestKickerVal As Integer)
            'Pair1 has the higher pair, Kicker contains the non pair card
            Dim Pair1 As New List(Of Card)
            Dim Pair2 As New List(Of Card)
            Dim TwoPairGroups As IEnumerable(Of IGrouping(Of String, Card))
            Dim pair1Rank As String
            Dim pair2Rank As String
            Dim Kicker As New Card

            For Each Cards In TwoPairHands
                TwoPairGroups = Cards.
                    GroupBy(Function(card) card.Rank).
                    Where(Function(group) group.Count() = 2).
                    OrderByDescending(Function(group) group.Key)
                pair1Rank = TwoPairGroups.First().Key
                pair2Rank = TwoPairGroups.Skip(1).First().Key
                Kicker = Cards.First(Function(card) card.Rank <> pair1Rank AndAlso card.Rank <> pair2Rank)
                Pair1 = TwoPairGroups.First().ToList()
                Pair2 = TwoPairGroups.Skip(1).First().ToList()

                If Pair1(0).Value > HighestPairVal Then
                    HighestPairVal = Pair1(0).Value
                    SecondHighestPairVal = Pair2(0).Value
                    HighestKickerVal = Kicker.Value
                    _BestHand = Cards

                ElseIf Pair1(0).Value = HighestPairVal Then
                    If Pair2(0).Value > SecondHighestPairVal Then
                        HighestPairVal = Pair1(0).Value
                        SecondHighestPairVal = Pair2(0).Value
                        HighestKickerVal = Kicker.Value
                        _BestHand = Cards

                    ElseIf Pair2(0).Value = SecondHighestPairVal Then
                        If Kicker.Value > HighestKickerVal Then
                            HighestPairVal = Pair1(0).Value
                            SecondHighestPairVal = Pair2(0).Value
                            HighestKickerVal = Kicker.Value
                            _BestHand = Cards
                        End If
                    End If
                End If
            Next
        End Sub
        Protected Sub GetBestOnePair(OnePairHands As List(Of List(Of Card)), ByRef HighestPairVal As Integer, ByRef HighestKickerVal As Integer, ByRef SecondHighestKickerVal As Integer, ByRef ThirdHighestKickerVal As Integer)
            Dim Pair As New List(Of Card)
            Dim NonPairs As New List(Of Card)
            Dim HighestKicker, SecondHighestKicker, ThirdHighestKicker As New Card
            For cards = 0 To OnePairHands.Count - 1
                Pair = OnePairHands(cards).
                    GroupBy(Function(card) card.Rank).
                    OrderByDescending(Function(group) group.Count).
                    Where(Function(group) group.Count() = 2).
                    SelectMany(Function(group) group).
                    ToList()
                NonPairs = OnePairHands(cards).Except(Pair).ToList
                SortCards(NonPairs)

                HighestKicker = NonPairs(2)
                SecondHighestKicker = NonPairs(1)
                ThirdHighestKicker = NonPairs(0)

                If Pair(0).Value > HighestPairVal Then
                    HighestPairVal = Pair(0).Value
                    HighestKickerVal = HighestKicker.Value
                    SecondHighestKickerVal = SecondHighestKicker.Value
                    ThirdHighestKickerVal = ThirdHighestKicker.Value
                    _BestHand = OnePairHands(cards)

                ElseIf Pair(0).Value = HighestPairVal Then
                    If HighestKicker.Value > HighestKickerVal Then
                        HighestPairVal = Pair(0).Value
                        HighestKickerVal = HighestKicker.Value
                        SecondHighestKickerVal = SecondHighestKicker.Value
                        ThirdHighestKickerVal = ThirdHighestKicker.Value
                        _BestHand = OnePairHands(cards)

                    ElseIf HighestKicker.Value = HighestKickerVal Then
                        If SecondHighestKicker.Value > SecondHighestKickerVal Then
                            HighestPairVal = Pair(0).Value
                            HighestKickerVal = HighestKicker.Value
                            SecondHighestKickerVal = SecondHighestKicker.Value
                            ThirdHighestKickerVal = ThirdHighestKicker.Value
                            _BestHand = OnePairHands(cards)

                        ElseIf SecondHighestKicker.Value = SecondHighestKickerVal Then
                            If ThirdHighestKicker.Value > ThirdHighestKickerVal Then
                                HighestPairVal = Pair(0).Value
                                HighestKickerVal = HighestKicker.Value
                                SecondHighestKickerVal = SecondHighestKicker.Value
                                ThirdHighestKickerVal = ThirdHighestKicker.Value
                                _BestHand = OnePairHands(cards)
                            End If
                        End If
                    End If
                End If
            Next
        End Sub
        Public Sub EvaluateHand(CommunityCards As List(Of Card))
            'So if its any type of hand it will be picked up by this condition
            Dim valueOfHand As Integer = 11
            Dim PossibleCards As List(Of Card)
            'Adds the 2 lists together
            PossibleCards = _HoleCards.Concat(CommunityCards).ToList
            Dim HighestCardValue As Integer = -1
            Dim SecondHighestCardValue As Integer = -1
            Dim ThirdHighestCardValue As Integer = -1
            Dim FourthHighestCardValue As Integer = -1
            Dim FifthHighestCardValue As Integer = -1
            Dim HighestCardHand As New List(Of Card)
            Dim RoyalFlushHands As New List(Of List(Of Card))
            Dim StraightFlushHands As New List(Of List(Of Card))
            Dim FourOfAKindHands As New List(Of List(Of Card))
            Dim FullHouseHands As New List(Of List(Of Card))
            Dim FlushHands As New List(Of List(Of Card))
            Dim StraightHands As New List(Of List(Of Card))
            Dim ThreeOfAKindHands As New List(Of List(Of Card))
            Dim TwoPairHands As New List(Of List(Of Card))
            Dim OnePairHands As New List(Of List(Of Card))
            Dim HandCombinations As List(Of List(Of Card))
            HandCombinations = Combinations(PossibleCards, 5)

            'Looping through each hand possibillity
            For Possibillty = 0 To HandCombinations.Count - 1
                SortCards(HandCombinations(Possibillty))
                Dim FirstCardSuit As String = HandCombinations(Possibillty)(0).Suit
                CheckRoyalFlush(HandCombinations(Possibillty), FirstCardSuit, RoyalFlushHands, valueOfHand)
                CheckStraightFlush(HandCombinations(Possibillty), FirstCardSuit, StraightFlushHands, valueOfHand)
                CheckFourOf(HandCombinations(Possibillty), FourOfAKindHands, valueOfHand)
                CheckFullHouse(HandCombinations(Possibillty), FullHouseHands, valueOfHand)
                CheckFlush(HandCombinations(Possibillty), FirstCardSuit, FlushHands, valueOfHand)
                CheckStraight(HandCombinations(Possibillty), StraightHands, valueOfHand)
                CheckThreeOf(HandCombinations(Possibillty), ThreeOfAKindHands, valueOfHand)
                CheckTwoPair(HandCombinations(Possibillty), TwoPairHands, valueOfHand)
                CheckOnePair(HandCombinations(Possibillty), OnePairHands, valueOfHand)
                CheckHighCard(HandCombinations(Possibillty), HighestCardHand, valueOfHand, HighestCardValue, SecondHighestCardValue, ThirdHighestCardValue, FourthHighestCardValue, FifthHighestCardValue)
            Next
            intHandValue = valueOfHand
            Select Case valueOfHand
                Case 1
                    'Doesnt matter which hand we add because if its a royal flush it wins or ties no matter what
                    _BestHand = RoyalFlushHands(0)
                Case 2
                    Dim HighestValue As Integer = 0
                    GetBestStraightFlush(StraightFlushHands, HighestValue)
                Case 3
                    Dim HighestKind As Integer = 0
                    Dim HighestKicker As Integer = 0
                    GetBestFourOf(FourOfAKindHands, HighestKind, HighestKicker)
                Case 4
                    Dim Highest3 As Integer = 0
                    Dim Highest2 As Integer = 0
                    GetBestFullHouse(FullHouseHands, Highest3, Highest2)
                Case 5
                    'Meaning the value of the highest card, second highest card etc etc
                    Dim HighestValue As Integer = 0
                    Dim FifthHighestValue As Integer = 0
                    Dim SecondHighestValue As Integer = 0
                    Dim ThirdHighestValue As Integer = 0
                    Dim FourthHighestValue As Integer = 0
                    GetBestFlush(FlushHands, HighestValue, SecondHighestValue, ThirdHighestValue, FourthHighestValue, FifthHighestValue)
                Case 6
                    Dim HighestValue As Integer = 0
                    GetBestStraight(StraightHands, HighestValue)
                Case 7
                    Dim HighestKickerCard As New Card
                    Dim SecondHighestKickerCard As New Card
                    Dim Highest3Value As Integer = 0
                    Dim HighestKicker As Integer = 0
                    Dim SecondHighestKicker As Integer = 0
                    GetBestThreeOf(ThreeOfAKindHands, Highest3Value, HighestKicker, SecondHighestKicker, HighestKickerCard, SecondHighestKickerCard)
                Case 8
                    Dim HighestPairVal As Integer = 0
                    Dim SecondHighestPairVal As Integer = 0
                    Dim HighestKickerVal = 0
                    GetBestTwoPair(TwoPairHands, HighestPairVal, SecondHighestPairVal, HighestKickerVal)
                Case 9
                    Dim Pair As New List(Of Card)
                    Dim NonPairs As New List(Of Card)
                    Dim HighestKicker, SecondHighestKicker, ThirdHighestKicker As New Card
                    Dim HighestPairVal As Integer = 0
                    Dim HighestKickerVal As Integer = 0
                    Dim SecondHighestKickerVal As Integer = 0
                    Dim ThirdHighestKickerVal As Integer = 0
                    GetBestOnePair(OnePairHands, HighestPairVal, HighestKickerVal, SecondHighestKickerVal, ThirdHighestKickerVal)
                Case 10
                    _BestHand = HighestCardHand
            End Select
        End Sub
    End Class
    Class Computer
        Inherits Player
        Private ReadOnly intNumber As Integer
        Private stComputerChoice
        Private OddsOfWin As Double
        Private Bluffing As Boolean
        Private BluffHand As New List(Of Card)
        Public Property ComputerChoice As String
            Get
                Return stComputerChoice
            End Get
            Set(value As String)
                MsgBox("User cannot change the values of the computers choice")
            End Set
        End Property
        Public Property Number As String
            Get
                Return intNumber
            End Get
            Set(value As String)
                MsgBox("You cannot change the number of the computers")
            End Set
        End Property
        Public Sub New(name As String)
            Me.Name = name
            'Takes the last character of their name eg "computer1" and turns the 1 to an integer
            intNumber = Convert.ToInt16(name(name.Length - 1)) - 48
        End Sub
        Protected Overrides Sub CallBet(ByRef Pot As Integer, ByRef CurrentHighestBet As Integer)
            MyBase.CallBet(Pot, CurrentHighestBet)
        End Sub
        Protected Overrides Sub AllIn(ByRef Pot As Integer, ByRef CurrentHighestBet As Integer)
            MyBase.AllIn(Pot, CurrentHighestBet)
        End Sub
        Public Overrides Sub ResetAttributes()
            intCurrentBet = 0
            _BestHand = New List(Of Card)
            _HoleCards = New List(Of Card)
            intHandValue = Nothing
            boolHasFolded = False
            boolIsAllIn = False
            Bluffing = False
            BluffHand = New List(Of Card)
        End Sub
        Private Sub ComputerBet(ByRef Pot As Integer, ByRef CurrentHighestBet As Integer)
            Dim Amount As Integer
            'Making sure they can never bet all their chips if they have many
            Dim MaxAmount As Integer = intChips * BetAmountAdjuster
            If Bluffing Then
                'This is so the else condition works has the bet amount adjuster could mean the max amount is less than 5 
                If MaxAmount <= MinimumBetAmount / BetAmountAdjuster Then
                    Amount = intChips
                Else
                    'If we are bluffing and have little chips bet a random amount between 5 and the max we can bet
                    Amount = Rand.Next(MinimumBetAmount, MaxAmount)
                End If

            Else
                'We don't want the bots betting too highly so we can take off 50% of their odds of winning and use that amount to bet
                Amount = OddsOfWin - BluffThreshold / PercentageMultiplier * MaxAmount
            End If
            If Amount >= intChips Then 'Was a case where had negative chips on the showdown
                MyBase.AllIn(Pot, CurrentHighestBet)
            ElseIf Amount < BigBlind Then
                MyBase.AllIn(Pot, CurrentHighestBet)
            Else
                Pot += Amount
                intChips -= Amount
                intCurrentBet += Amount
                If intCurrentBet > CurrentHighestBet Then
                    CurrentHighestBet = Amount
                End If
            End If
        End Sub
        Public Sub GetChoice(CurrentHighestBet As Integer, Pot As Integer, GameState As String, CommunityCards As List(Of Card), Players As Integer, position As Integer)
            Dim Decision As String
            'if they are all in or have put more than or equal the current highest and the current highest bet isnt 0 we skip them
            If boolIsAllIn OrElse (intCurrentBet >= CurrentHighestBet And CurrentHighestBet <> 0) Then
                Decision = "skip"
            ElseIf HasFolded = True Then
                Decision = "skip"
            Else
                If intCurrentBet = CurrentHighestBet Then
                    Decision = GetBetCheck(CurrentHighestBet, Pot, GameState, CommunityCards, Players, position)
                Else
                    Decision = GetRaiseCallFold(CurrentHighestBet, Pot, GameState, CommunityCards, Players, position)
                End If
            End If
            stComputerChoice = Decision
        End Sub
        Public Overloads Sub DoChoice(ByRef CurrentHighestBet As Integer, ByRef Pot As Integer)
            Dim Amount As Integer
            Dim Valid As Boolean = False
            While Not Valid
                Select Case ComputerChoice
                    Case "skip"
                        Exit Sub
                    Case "raise"
                        Amount = RaiseAmountAdjuster * CurrentHighestBet + CurrentHighestBet
                        Raise(Pot, CurrentHighestBet, Amount)
                        Valid = True
                    Case "call"
                        Dim AmountToCall As Integer = CurrentHighestBet - intCurrentBet
                        If AmountToCall >= intChips Then
                            AllIn(Pot, CurrentHighestBet)
                        Else
                            CallBet(Pot, CurrentHighestBet)
                        End If
                        Valid = True
                    Case "check"
                        Valid = True
                    Case "all in"
                        AllIn(Pot, CurrentHighestBet)
                        Valid = True
                    Case "bet"
                        ComputerBet(Pot, CurrentHighestBet)
                        Valid = True
                    Case "fold"
                        boolHasFolded = True
                        Valid = True
                End Select
            End While
        End Sub
        Private Function GetBetCheck(CurrentHighestBet As Integer, Pot As Integer, GameState As String, CommunityCards As List(Of Card), Players As Integer, Position As Integer) As String
            Dim PotEquity As Double = CurrentHighestBet / (CurrentHighestBet + Pot) * PercentageMultiplier
            OddsOfWin = MonteCarlo(CommunityCards, GameState, Players)
            Dim Decision As String
            'The lower the position (the earlier it acts on the round)
            'The higher its "place" and the higher chance it bets or raises
            Dim Place As Integer = (Players - Position) * BetChanceMultiplier
            'If the odds of win is higher than 75%
            If OddsOfWin > 75 Then
                If CurrentHighestBet >= intChips Then
                    Decision = "call"
                Else
                    Decision = "bet"
                End If
            ElseIf Bluffing = True Then
                Decision = "bet"
            Else
                'I dont want it to bluff when betting / checking too often
                If Rand.Next(BetProbAdjuster - Place) <= BluffProbAdjuster Then
                    Decision = "bet"
                    Bluffing = True
                Else
                    Decision = "check"
                End If
            End If
            Return Decision
        End Function
        Private Function GetRaiseCallFold(CurrentHighestBet As Integer, Pot As Integer, GameState As String, CommunityCards As List(Of Card), Players As Integer, Position As Integer) As String
            Dim PotOdds As Double = CurrentHighestBet / (CurrentHighestBet + Pot) * PercentageMultiplier
            OddsOfWin = MonteCarlo(CommunityCards, GameState, Players)
            Dim Decision As String
            Dim Place As Integer = (Players - Position) * BetChanceMultiplier
            'If the chance of winning is 15% higher than pot odds
            If OddsOfWin + 15 > PotOdds Then
                Decision = "call"
            Else
                'No point bluffing on the river if they are not first to act on the round
                'Maybe there is no point bluffing if they aren't the first to act in any round
                'Unless the first player checks
                If GameState <> "Pre-Flop" Then
                    If Rand.Next(CallRaiseAdjuster - Place) <= BluffProbAdjuster Then
                        Bluffing = True
                        Decision = "call"
                    Else
                        Decision = "fold"
                    End If
                Else
                    Decision = "fold"

                End If
            End If
            Return Decision
        End Function

        Private Function FindMonteCarloWinner(Splayers As List(Of Player)) As Integer()
            Dim sOutcomes(2) As Integer
            Dim MyHandValue As Integer = intHandValue
            SortPlayers(Splayers)
            Dim BestHandValue As Integer = Splayers(0).HandValue
            If MyHandValue < BestHandValue Then
                sOutcomes(0) += 1
            ElseIf MyHandValue = BestHandValue Then
                Dim Winners As New List(Of Player) From {
                Me
            }
                For item = 0 To Splayers.Count - 1
                    If Splayers(item).HandValue = BestHandValue Then
                        Winners.Add(Splayers(item))
                    End If
                Next
                Dim Winner As List(Of Player) = Tiebreak(Winners, BestHandValue)
                If Winner.Contains(Me) AndAlso Winner.Count = 1 Then
                    sOutcomes(0) += 1
                ElseIf Winner.Contains(Me) AndAlso Winner.Count <> 1 Then
                    sOutcomes(2) += 1
                Else
                    sOutcomes(1) += 1
                End If
            Else
                sOutcomes(1) += 1
            End If
            Return sOutcomes
        End Function
        Private Function SimulatePreFlop(KnownCards As List(Of Card), sPlayers As List(Of Player)) As Integer()
            Dim sCommunityCards As New List(Of Card)
            Dim sDeck As New Deck
            'Remove the cards we can see as no player can have them
            For Each Card In KnownCards
                sDeck.RemoveCard(Card)
            Next
            'Shuffle the simulated deck and deal out cards to simulated players
            sDeck.Shuffle()
            For Each P As Player In sPlayers
                sDeck.Deal2Cards(P.HoleCards)
            Next
            'Add 5 community cards to simulate a showdown
            sDeck.Deal2Cards(sCommunityCards)
            sDeck.Deal3Cards(sCommunityCards)

            'Evaluate the hands of all players in simulation
            For Each Player In sPlayers
                Player.EvaluateHand(sCommunityCards)
            Next
            EvaluateHand(sCommunityCards)
            'Figure out the best hand
            Dim sOutcomes() As Integer = FindMonteCarloWinner(sPlayers)
            Return sOutcomes
        End Function
        Private Function SimulateFlop(KnownCards As List(Of Card), CommunityCards As List(Of Card), Splayers As List(Of Player)) As Integer()
            Dim sCommunityCards As List(Of Card) = CommunityCards
            Dim sDeck As New Deck
            For Each Card In KnownCards
                sDeck.RemoveCard(Card)
            Next
            sDeck.Shuffle()
            For Each P As Player In Splayers
                sDeck.Deal2Cards(P.HoleCards)
            Next
            sDeck.Deal2Cards(sCommunityCards)
            For Each Player In Splayers
                Player.EvaluateHand(sCommunityCards)
            Next
            EvaluateHand(sCommunityCards)
            Dim sOutcomes() As Integer = FindMonteCarloWinner(Splayers)
            Return sOutcomes
        End Function
        Private Function SimulateTurn(KnownCards As List(Of Card), CommunityCards As List(Of Card), Splayers As List(Of Player)) As Integer()
            Dim sCommunityCards As List(Of Card) = CommunityCards
            Dim sDeck As New Deck
            For Each Card In KnownCards
                sDeck.RemoveCard(Card)
            Next
            sDeck.Shuffle()
            For Each P As Player In Splayers
                sDeck.Deal2Cards(P.HoleCards)
            Next
            sDeck.DealCard(sCommunityCards)
            For Each Player In Splayers
                Player.EvaluateHand(sCommunityCards)
            Next
            EvaluateHand(sCommunityCards)
            Dim sOutcomes() As Integer = FindMonteCarloWinner(Splayers)
            Return sOutcomes
        End Function
        Private Function SimulateRiver(KnownCards As List(Of Card), CommunityCards As List(Of Card), Splayers As List(Of Player)) As Integer()
            Dim sCommunityCards As List(Of Card) = CommunityCards
            Dim sDeck As New Deck
            For Each Card In KnownCards
                sDeck.RemoveCard(Card)
            Next
            sDeck.Shuffle()
            For Each P As Player In Splayers
                sDeck.Deal2Cards(P.HoleCards)
            Next
            For Each Player In Splayers
                Player.EvaluateHand(sCommunityCards)
            Next
            Dim sOutcomes() As Integer = FindMonteCarloWinner(Splayers)
            Return sOutcomes
        End Function
        Private Shared Function SetPlayers(Splayers As List(Of Player), Players As Integer) As List(Of Player)
            If Players = 2 Then
                Dim sPlayer1 As New Player With {
                    .Name = "sPlayer1"
                }
                Splayers.Add(sPlayer1)

            ElseIf Players = 3 Then
                Dim sPlayer1 As New Player With {
                    .Name = "sPlayer1"
                }
                Dim Splayer2 As New Player With {
                    .Name = "Splayer2"}
                Splayers.Add(sPlayer1)
                Splayers.Add(Splayer2)

            ElseIf Players = 4 Then
                Dim sPlayer1 As New Player With {
                    .Name = "sPlayer1"
                }
                Dim Splayer2 As New Player With {
                    .Name = "Splayer2"}
                Dim Splayer3 As New Player With {
                    .Name = "Splayer2"}
                Splayers.Add(sPlayer1)
                Splayers.Add(Splayer2)
                Splayers.Add(Splayer3)

            ElseIf Players = 5 Then
                Dim sPlayer1 As New Player With {
                    .Name = "sPlayer1"
                }
                Dim Splayer2 As New Player With {
                    .Name = "Splayer2"}
                Dim Splayer3 As New Player With {
                    .Name = "Splayer2"}
                Dim Splayer4 As New Player With {
                    .Name = "Splayer4"}
                Splayers.Add(sPlayer1)
                Splayers.Add(Splayer2)
                Splayers.Add(Splayer3)
                Splayers.Add(Splayer4)
            End If
            Return Splayers
        End Function
        Private Function MonteCarlo(CommunityCards As List(Of Card), GameState As String, Players As Integer) As Double
            Dim Outcomes() As Integer
            '(0) is a win, (1) is a loss (2) is a tie
            Dim OutcomeTally(2) As Integer
            Dim KnownCards As List(Of Card) = _HoleCards.Concat(CommunityCards).ToList
            Dim Splayers As New List(Of Player)
            Splayers = SetPlayers(Splayers, Players)
            Dim Temp As New List(Of Card)(_HoleCards)
            If Bluffing Then
                Bluff(CommunityCards)
                _HoleCards = BluffHand
            End If

            If GameState = "Pre-Flop" Then
                For num = 1 To Iterations
                    Outcomes = SimulatePreFlop(KnownCards, Splayers)
                    OutcomeTally(0) += Outcomes(0)
                    OutcomeTally(1) += Outcomes(1)
                    OutcomeTally(2) += Outcomes(2)
                    For Each P In Splayers
                        P.ResetAttributes()
                    Next
                Next

            ElseIf GameState = "Flop" Then
                For num = 1 To Iterations
                    Outcomes = SimulateFlop(KnownCards, New List(Of Card)(CommunityCards), Splayers)
                    OutcomeTally(0) += Outcomes(0)
                    OutcomeTally(1) += Outcomes(1)
                    OutcomeTally(2) += Outcomes(2)
                    For Each P As Player In Splayers
                        P.ResetAttributes()
                    Next
                Next

            ElseIf GameState = "Turn" Then
                For num = 1 To Iterations
                    Outcomes = SimulateTurn(KnownCards, New List(Of Card)(CommunityCards), Splayers)
                    OutcomeTally(0) += Outcomes(0)
                    OutcomeTally(1) += Outcomes(1)
                    OutcomeTally(2) += Outcomes(2)
                    For Each P As Player In Splayers
                        P.ResetAttributes()
                    Next
                Next

            Else
                ' GameState = River
                EvaluateHand(CommunityCards)
                For num = 1 To Iterations
                    Outcomes = SimulateRiver(KnownCards, New List(Of Card)(CommunityCards), Splayers)
                    OutcomeTally(0) += Outcomes(0)
                    OutcomeTally(1) += Outcomes(1)
                    OutcomeTally(2) += Outcomes(2)
                    For Each P As Player In Splayers
                        P.ResetAttributes()
                    Next
                Next
            End If

            'Resetting these back to default as simulate() would have changed them
            _BestHand = New List(Of Card)
            _HoleCards = Temp
            intHandValue = 0
            Dim WinNum As Double = OutcomeTally(0) / (OutcomeTally(0) + OutcomeTally(1) + OutcomeTally(2))
            Dim LoseNum As Double = OutcomeTally(1) / (OutcomeTally(0) + OutcomeTally(1) + OutcomeTally(2))
            Dim TieNum As Double = OutcomeTally(2) / (OutcomeTally(0) + OutcomeTally(1) + OutcomeTally(2))
            Return (WinNum + TieNum) * PercentageMultiplier
        End Function
        Private Sub Bluff(CommunityCards As List(Of Card))
            'The bluffing wont happen on the pre-flop
            Dim bDeck As New Deck
            Dim CardList As New List(Of Card)
            Dim RoyalFlushes As Integer = 0
            Dim StraightFlushes As Integer = 0
            Dim FourOfs As Integer = 0
            Dim FullHouses As Integer = 0
            Dim Flushes As Integer = 0
            Dim Straights As Integer = 0
            Dim ThreeOfs As Integer = 0
            Dim bPlayer As New Player
            'Remove community cards from created deck because we cannot have those cards
            For j = 0 To CommunityCards.Count - 1
                bDeck.RemoveCard(CommunityCards(j))
            Next
            For d = 0 To bDeck.Count - 1
                bDeck.DealCard(CardList)
            Next
            Dim hands As List(Of List(Of Card)) = Combinations(CardList, 2)
            Dim HighestHand As Integer = 11
            For i = 0 To hands.Count - 1
                bPlayer.HoleCards = hands(i)
                bPlayer.EvaluateHand(CommunityCards)
                'There has to be more than two types of the hand because the other player may have the only way of getting that hand so they would know we are bluffing
                Select Case bPlayer.HandValue
                    Case 1
                        RoyalFlushes += 1
                        If HighestHand > bPlayer.HandValue AndAlso RoyalFlushes >= 2 Then
                            BluffHand = hands(i)
                            HighestHand = 1
                        End If
                    Case 2
                        StraightFlushes += 1
                        If HighestHand > bPlayer.HandValue AndAlso StraightFlushes >= 2 Then
                            BluffHand = hands(i)
                            HighestHand = 2
                        End If
                    Case 3
                        FourOfs += 1
                        If HighestHand > bPlayer.HandValue AndAlso FourOfs >= 2 Then
                            BluffHand = hands(i)
                            HighestHand = 3
                        End If
                    Case 4
                        FullHouses += 1
                        If HighestHand > bPlayer.HandValue AndAlso FullHouses >= 2 Then
                            BluffHand = hands(i)
                            HighestHand = 4
                        End If
                    Case 5
                        Flushes += 1
                        If HighestHand > bPlayer.HandValue AndAlso Flushes >= 2 Then
                            BluffHand = hands(i)
                            HighestHand = 5
                        End If
                    Case 6
                        Straights += 1
                        If HighestHand > bPlayer.HandValue AndAlso Straights >= 2 Then
                            BluffHand = hands(i)
                            HighestHand = 6
                        End If
                    Case 7
                        ThreeOfs += 1
                        If HighestHand > bPlayer.HandValue AndAlso ThreeOfs >= 2 Then
                            BluffHand = hands(i)
                            HighestHand = 7
                        End If
                End Select
            Next
            If BluffHand.Count <> 2 Then
                Bluffing = False
            Else
            End If
        End Sub
    End Class
    Private Sub BtnCheck_Click(sender As Object, e As EventArgs) Handles BtnCheck.Click
        PlayerInput = "check"
    End Sub
    Private Sub BtnBet_Click(sender As Object, e As EventArgs) Handles BtnBet.Click
        PlayerInput = "bet"
        PlayerInputNumber = Val(TxtBetAmount.Text)
    End Sub
    Private Sub BtnFold_Click(sender As Object, e As EventArgs) Handles BtnFold.Click
        PlayerInput = "fold"
    End Sub
    Private Sub BtnCall_Click(sender As Object, e As EventArgs) Handles BtnCall.Click
        PlayerInput = "call"
    End Sub
    Private Sub BtnRaise_Click(sender As Object, e As EventArgs) Handles BtnRaise.Click
        PlayerInput = "raise"
        PlayerInputNumber = Val(TxtRaiseAmount.Text)
    End Sub
    Private Sub TxtRaiseAmount_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TxtRaiseAmount.KeyPress
        'Making sure the input is only a number or a backspace/delete key
        If Not Char.IsNumber(e.KeyChar) And Not e.KeyChar = Chr(Keys.Back) And Not e.KeyChar = Chr(Keys.Delete) Then
            e.Handled = True
            MsgBox("Only enter numbers for the raise amount")
        End If
    End Sub
    Private Sub TxtBetAmount_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TxtBetAmount.KeyPress
        'Making sure the input is only a number or a backspace/delete key
        If Not Char.IsNumber(e.KeyChar) And Not e.KeyChar = Chr(Keys.Back) And Not e.KeyChar = Chr(Keys.Delete) Then
            e.Handled = True
            MsgBox("Only enter numbers for the bet amount")
        End If
    End Sub
End Class