# Testing

Dette dokumentet beskriver hvordan vi tester prosjektet, hvilke deler som er dekket av tester, og hvordan man kjører testene.

## Hvordan kjøre testene

For å kjøre alle tester i løsningen:

dotnet test
´´´bash
Test summary: total: 10; failed: 0; succeeded: 10; skipped: 0; duration: 1,2s
Build succeeded with 1 warning(s) in 2,4s

Prosjektstruktur (relevant for testing):
	•	AOR/ – webapplikasjon og domene/modeller
	•	UnitTests/ – NUnit unit tester for domene og repositories

Testprosjektet bruker:
	•	NUnit som test-rammeverk
	•	EF Core InMemory-database for å teste repositories uten ekte database

Hva de ulike testene gjør

Domene / modeller

AorDbContext_ShouldSeed_DefaultStatuses
Tester at databasen seedes med fem standardstatusverdier:
Pending, Approved, Rejected, Draft, Deleted.

ObstacleData_ShouldSetCreatedAt_OnCreation
Sikrer at CreatedAt setter seg automatisk når et nytt ObstacleData-objekt opprettes.

ObstacleData_ShouldRequireDescription_WhenTypeIsOther
Validerer at ObstacleDescription kreves når ObstacleType == "other".

ObstacleData_ShouldBeValid_WhenTypeOtherHasDescription
Tester at modellen er gyldig når type er other og beskrivelse er angitt.

⸻

Repository-tester

OrganizationRepository_ExistsAsync_ReturnsTrue_WhenOrganizationExists
Returnerer true hvis en organisasjon med gitt OrgNr finnes.

OrganizationRepository_ExistsAsync_ReturnsFalse_WhenOrganizationDoesNotExist
Returnerer false når organisasjonen ikke finnes.

OrganizationRepository_DeleteAsync_RemovesOrganization
Tester at sletting fungerer og kun riktig organisasjon fjernes.

ObstacleRepository_AddAndGetByIdAsync_PersistsObstacle
Sjekker at hinder lagres og kan hentes opp igjen riktig.

UserRepository_GetByOrganizationAsync_ReturnsOnlyUsersFromThatOrganization
Sikrer at kun brukere som tilhører en bestemt organisasjon returneres.

ReportRepository_UpdateStatusAndDelete_WorksAsExpected
Tester:
	1.	At UpdateStatusAsync oppdaterer status på riktig rapport
	2.	At DeleteAsync fjerner rapporten permanent

