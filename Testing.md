### Testing scenarier

# Log In
- Logg inn med de forskjellige brukerne og passordene som står i README.md, så skal du komme til forskjellige index sider.

# Unit tests:

LogInControllerTests:

1. Index_Post_ValidRegisterforer_RedirectsToRegisterforerIndex

Tester at når en bruker logger inn med riktige legitimasjon (reg@uia.no, 123), blir de sendt videre til riktig side — Registerforer/Index.
Denne testen bekrefter at RedirectToActionResult fungerer og at brukeren får riktig rolle.

2. Index_Post_InvalidUser_ReturnsViewWithError

Tester at hvis brukeren skriver feil brukernavn eller passord, så vises innloggingssiden igjen.
I tillegg kontrolleres det at ModelState inneholder en feilmelding, noe som betyr at innloggingen mislykkes.

3. Index_Get_ReturnsLoginView_WithViewData

Tester at GET /LogIn/Index returnerer innloggingssiden (ViewResult) med riktig modell (LogInData).
Den sjekker også at ViewData inneholder informasjon om database-tilkoblingen (DbConnected og DbError).


ObstacleControllerTests:

1. DataForm_Post_ValidModel_Returns_Overview_And_Sets_CreatedAt

Tester at når et gyldig hinder (Obstacle) sendes inn via POST, returneres “Overview”-viewet.
Testen bekrefter også at CreatedAt-feltet blir automatisk satt til riktig tidspunkt.

ForgotPasswordControllerTests:

1. Index_Post_ValidModel_RedirectsToLogIn

Tester at når brukeren skriver inn en gyldig e-post i “Glemt passord”-skjemaet, logges forespørselen.
Deretter sjekkes det at brukeren sendes videre til LogIn/Index-siden.

CrewControllerTests

1. Index_Returns_ViewResult

Tester at CrewController.Index() returnerer et gyldig ViewResult.
Denne testen sikrer at hovedsiden for Crew vises uten feil.

2. Privacy_Returns_ViewResult

Tester at CrewController.Privacy() returnerer riktig view.
Bekrefter at personvern-siden lastes og vises som forventet.

3.  Error_Returns_View_With_ErrorViewModel

Tester at CrewController.Error() returnerer et view med en modell av typen ErrorViewModel.
Den sjekker at RequestId ikke er tom og at feilhåndteringen fungerer som planlagt.
