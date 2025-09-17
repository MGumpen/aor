# Branch struktur:

- Main branch – Produktsjon, stabil. Beskyttes via Pull Request fra andre brancher. Umulig å pushe til main. Kode skal kontrolleres av minst 1 person for     godkjenning av PR.

- Update – «Develop branch». Hit går alle oppdateringer før de kan merge til main.

- Andre brancher – Alle skal opprette egne brancher når de jobber med store deler av prosjektet. Disse skal deretter merges til update for å teste at de ikke ødelegger noe før de går til main.

- Det kjøres tester med ci.yml filer som tester alle prosjekter før de pushes til main. Disse bygger prosjektet og gir tilbakemelding om det kjører riktig.


# Github kommandoer

- Git pull origin main – Henter siste oppdateringer fra main.

- Git checkout «branch» - Navigerer til oppgitt branch.

- Git checkout -b «nytt branch navn» - Oppretter en ny branch til din oppgave.

- Git add . – Legger til oppdateringer fra koden.

- Git commit -m «Commit message her» - Commiter endringer i koden med en melding om hva som er endret.

- Git push origin «branch» - Pusher koden som er oppdatert til oppgitt branch. All kode skal pushes til branchen som du opprettet til din del av oppgaven. Umulig å pushe direkte til main. 
